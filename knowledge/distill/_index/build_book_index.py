#!/usr/bin/env python3
"""Book index DB for the docs/knowledge crash course (source set v2).

A standalone SQLite + FTS5 index over the four source books:
  * knowledge/md/*.md            — pandoc conversions of the 3 epubs
  * knowledge/src/the-elmish-book/chapters/**.md — the Elmish Book (already md)

Deliberately separate from the docs/ doc-impact index — same technique
(FTS5 with porter stemming + bm25), different database.

Purpose: while distilling one book at a time, use this index to quickly look
up where a term/concept appears across ALL books, so related knowledge stays
together in the same guide documents. Also powers audit_distill.py, which
verifies the published guide (docs/knowledge/) against the sources.

Usage (from repo root or this folder):
  python3 build_book_index.py build                    # (re)build _index/book-index.db
  python3 build_book_index.py search "two-track"       # bm25-ranked chunk search
  python3 build_book_index.py search "let!"            # punctuated -> LIKE scan
  python3 build_book_index.py term "aggregate"         # cross-book term coverage
  python3 build_book_index.py glossary                 # all glossary terms x books
  python3 build_book_index.py outline dmmf             # chapters + parts of a book
  python3 build_book_index.py chapter dmmf 7           # line range of a chapter
  python3 build_book_index.py section routing          # find an Elmish section

Books and their ids: dmmf, fsa, sf6, elmish.
"""

from __future__ import annotations

import os
import re
import sqlite3
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MD = os.path.join(REPO, "knowledge", "md")
ELMISH = os.path.join(REPO, "knowledge", "src", "the-elmish-book", "chapters")
DB_PATH = os.path.join(HERE, "book-index.db")

# The Elmish Book's authoritative reading order (sidebar array of index.html),
# as <part folder>/<section stem>. "" = the part README (introduction).
# failing-async.md is commented out in the author's own sidebar — not in the book.
ELMISH_ORDER = {
    "fable": ["", "hello-world", "development-mode", "counter", "observations",
              "compatibility", "fable-packages", "fable-bindings", "node-packages",
              "how-fable-works"],
    "elm": ["", "the-architecture", "counter", "render-html", "conditional-rendering",
            "using-css", "form-inputs", "react-in-elmish", "todo-app",
            "todo-app-part1", "todo-app-part2", "todo-app-part3",
            "todo-app-exercises", "project-structure"],
    "commands": ["", "commands", "definition", "async-updates", "async-to-cmd",
                 "async-state", "deferred-module-utilities", "recursive-updates",
                 "async-recursive-updates", "http", "xhr-elmish", "async-xhr",
                 "working-with-json", "using-json", "elmish-hackernews",
                 "elmish-hackernews-part1", "elmish-hackernews-part2",
                 "elmish-hackernews-part3", "elmish-hackernews-exercises"],
    "scaling": ["", "pages-as-programs", "splitting-programs", "integrating-commands",
                "composition-forms", "understanding-data-communication", "intent",
                "routing", "parsing-url-segments", "parsing-date-segments",
                "programmatic-navigation", "introducing-url-type",
                "modelling-nested-routes", "multi-page-routing"],
    "dev-flow": ["", "webpack-mode", "compiler-directives", "configuration-variables",
                 "hot-module-replacement", "understanding-webpack-loaders",
                 "static-images", "styling-with-sass", "introducing-femto"],
}

TAG_RE = re.compile(r"<[^>]*>")

# dmmf (pandoc): '#  Chapter 1 Introducing Domain-Driven Design' + '# Part 1 ...'
DMMF_CH_RE = re.compile(r"^#\s+Chapter\s+(\d+)\s+(.+)$")
DMMF_PART_RE = re.compile(r"^#\s+Part\s+(\d+)\s+(.+)$")
# fsa (pandoc, O'Reilly): '# Chapter 1. Introduction' + '# Part I. Foundations'
FSA_CH_RE = re.compile(r"^#\s+Chapter\s+(\d+)\.\s*(.+)$")
FSA_PART_RE = re.compile(r"^#\s+Part\s+([IVX]+)\.\s*(.+)$")
# sf6 (pandoc, Apress): '# 1. The Sense of Style'  (no parts)
SF6_CH_RE = re.compile(r"^#\s+(\d+)\.\s+(.+)$")


def clean(text: str) -> str:
    return text.replace("\u00a0", " ")


# --- FTS5 query handling -----------------------------------------------------
WORD = re.compile(r"[A-Za-z0-9]+")
PURE_WORDS = re.compile(r"[\w\s]+\Z")


def fts_query(query: str) -> tuple[str, str]:
    """Return (fts MATCH expression, mode) for a user query.

    mode is 'fts' or 'like'. Punctuated queries ("let!", "Cmd<'msg>",
    "Html.Lazy", "two-track") MUST take the LIKE path: FTS5's porter stemmer
    strips punctuation, so 'let!' indexes as 'let' and also matches 'lets'.
    Pure word/whitespace queries use the FTS index (phrase-quoted multi-word).
    """
    if PURE_WORDS.match(query.strip()) and WORD.search(query):
        words = WORD.findall(query)
        if len(words) == 1:
            return words[0], "fts"
        return f'"{" ".join(words)}"', "fts"
    return "", "like"


def read_lines(path: str):
    with open(path, encoding="utf-8") as f:
        return f.readlines()


def yield_markdown_chapters(path, ch_re, part_re=None):
    """Generic H1-chapter splitter for the pandoc conversions.

    Yields (chapter_no, chapter_title, part, [(line_no, line)]).
    Front matter (before the first chapter H1) yields as ch 0.
    """
    part = ""
    cur = (0, "Front matter")
    buf: list[tuple[int, str]] = []
    for i, line in enumerate(read_lines(path), 1):
        s = line.strip()
        if s.startswith("# ") and "```" not in s:
            m = ch_re.match(s)
            if m:
                if buf:
                    yield (*cur, part, buf)
                cur = (int(m.group(1)), TAG_RE.sub("", m.group(2)).strip())
                buf = []
                continue
            if part_re:
                m = part_re.match(s)
                if m:
                    part = " ".join(TAG_RE.sub("", g).strip() for g in m.groups())
        buf.append((i, line))
    if buf:
        yield (*cur, part, buf)


def parse_dmmf(path: str):
    yield from yield_markdown_chapters(path, DMMF_CH_RE, DMMF_PART_RE)


def parse_fsa(path: str):
    yield from yield_markdown_chapters(path, FSA_CH_RE, FSA_PART_RE)


def parse_sf6(path: str):
    yield from yield_markdown_chapters(path, SF6_CH_RE)


def parse_elmish():
    """Yield Elmish Book chapters from the chapters/ tree, in sidebar order.

    Chapter numbering follows the reading order across parts. Titles come
    from each file's first H1. Part folder READMEs are the introductions.
    """
    ch_no = 0
    for part, sections in ELMISH_ORDER.items():
        part_title = f"Part: {part}"
        for sec in sections:
            if sec == "":
                path = os.path.join(ELMISH, part, "README.md")
                fallback = f"{part} (introduction)"
            else:
                path = os.path.join(ELMISH, part, f"{sec}.md")
                fallback = sec
            if not os.path.exists(path):
                continue
            ch_no += 1
            lines = read_lines(path)
            h1 = next(
                (l.lstrip("# ").strip() for l in lines if l.startswith("# ")),
                fallback,
            )
            yield (ch_no, h1, part_title, [(i, l) for i, l in enumerate(lines, 1)])


PARSERS = {
    "dmmf": ("domain-modeling-made-functional.md", parse_dmmf),
    "fsa": ("fundamentals-of-software-architecture.md", parse_fsa),
    "sf6": ("stylish-fsharp-6.md", parse_sf6),
}


def md_section(frags):
    """Attribution helper for md sources: nearest '## ' heading above each line."""
    out = {}
    cur = ""
    for ln, txt in frags:
        s = txt.strip()
        if s.startswith("## ") and not s.startswith("### "):
            t = TAG_RE.sub("", s).lstrip("#").strip()
            if 3 < len(t) < 90:
                cur = t
        out[ln] = cur
    return out


def chunkify(frags, max_chars=1400):
    """frags: list of (line_no, text). Split into ~max_chars chunks at blank lines."""
    chunks = []
    cur_lines: list[str] = []
    cur_start = None
    cur_len = 0
    last_line = None

    def emit():
        nonlocal cur_lines, cur_start, cur_len
        if cur_lines:
            text = "\n".join(cur_lines).strip()
            if text:
                chunks.append((cur_start or 1, last_line or cur_start or 1, text))
        cur_lines, cur_start, cur_len = [], None, 0

    for ln, txt in frags:
        t = txt.rstrip("\n")
        if cur_start is None:
            cur_start = ln
        cur_lines.append(t)
        cur_len += len(t) + 1
        last_line = ln
        if cur_len >= max_chars and t.strip() == "":
            emit()
        elif cur_len >= max_chars * 2:
            emit()
    emit()
    return chunks


GLOSSARY = [
    # DDD (dmmf)
    "domain-driven design", "shared model", "ubiquitous language", "business event",
    "subdomain", "bounded context", "context map", "domain expert", "workflow",
    "value object", "entity", "aggregate", "identity", "invariant", "state machine",
    "simple value", "choice type", "record type", "units of measure", "integrity",
    "consistency", "transaction", "command-query separation", "DTO", "serialization",
    "persistence", "repository", "document database", "relational database",
    # functional craft (dmmf / sf6)
    "total function", "composition", "pipe", "function type", "type annotation",
    "type alias", "custom type", "case expression", "pattern matching", "record",
    "list", "tuple", "option", "Result", "two-track", "bind", "monad",
    "computation expression", "dependency injection", "partial application",
    "currying", "higher-order function", "recursion", "recursive type",
    "immutability", "pure function", "side effect", "railway oriented",
    # architecture (fsa)
    "architecture characteristics", "architectural style", "trade-off", "modularity",
    "coupling", "cohesion", "component", "layered architecture", "modular monolith",
    "pipeline architecture", "microkernel", "service-based", "event-driven",
    "space-based", "service-oriented", "microservices", "architecture decision",
    "fitness function", "architecture principles", "quantitative analysis",
    "diagram", "C4 model", "soft skills", "negotiation",
    # Elmish (elmish)
    "The Elm Architecture", "model", "update", "view", "message", "dispatch",
    "command", "subscription", "program", "render", "Fable", "virtual DOM",
    "React", "route", "routing", "hot module replacement", "webpack", "sass",
    "femto", "JSON", "Thoth", "promise", "nested program",
]

SCHEMA = """
CREATE TABLE IF NOT EXISTS meta (key TEXT PRIMARY KEY, value TEXT);
CREATE TABLE IF NOT EXISTS chapters (
  id INTEGER PRIMARY KEY,
  book TEXT NOT NULL,
  chapter_no INTEGER NOT NULL,
  chapter_title TEXT NOT NULL,
  part TEXT NOT NULL DEFAULT '',
  line_start INTEGER NOT NULL,
  line_end INTEGER NOT NULL,
  chunk_count INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS terms (
  term TEXT NOT NULL,
  book TEXT NOT NULL,
  chapter_no INTEGER NOT NULL,
  chapter_title TEXT NOT NULL,
  hits INTEGER NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_terms_term ON terms(term);
CREATE INDEX IF NOT EXISTS idx_terms_book ON terms(book);
CREATE VIRTUAL TABLE IF NOT EXISTS chunks_fts USING fts5(
  text, book UNINDEXED, chapter_no UNINDEXED, chapter_title UNINDEXED,
  part UNINDEXED, section UNINDEXED, line_start UNINDEXED, line_end UNINDEXED,
  tokenize = 'porter unicode61'
);
"""


def build() -> None:
    if os.path.exists(DB_PATH):
        os.remove(DB_PATH)
    con = sqlite3.connect(DB_PATH)
    con.executescript(SCHEMA)
    total_chunks = 0
    for book_id, (fname, parser) in PARSERS.items():
        path = os.path.join(MD, fname)
        n_chunks = 0
        for ch_no, ch_title, part, frags in parser(path):
            chunks = chunkify(frags)
            sec_of = md_section(frags)
            for (ls, le, text) in chunks:
                con.execute(
                    "INSERT INTO chunks_fts VALUES (?,?,?,?,?,?,?,?)",
                    (clean(text), book_id, ch_no, ch_title, part,
                     sec_of.get(ls, ""), ls, le),
                )
                n_chunks += 1
        con.execute(
            "INSERT INTO chapters (book, chapter_no, chapter_title, part, line_start, line_end, chunk_count) "
            "SELECT book, chapter_no, chapter_title, part, MIN(line_start), MAX(line_end), COUNT(*) "
            "FROM chunks_fts WHERE book=? GROUP BY chapter_no, chapter_title, part",
            (book_id,),
        )
        print(f"{book_id:7s} {n_chunks:5d} chunks")
        total_chunks += n_chunks

    # the Elmish Book
    n_chunks = 0
    for ch_no, ch_title, part, frags in parse_elmish():
        chunks = chunkify(frags)
        for (ls, le, text) in chunks:
            con.execute(
                "INSERT INTO chunks_fts VALUES (?,?,?,?,?,?,?,?)",
                (clean(text), "elmish", ch_no, ch_title, part, "", ls, le),
            )
            n_chunks += 1
    con.execute(
        "INSERT INTO chapters (book, chapter_no, chapter_title, part, line_start, line_end, chunk_count) "
        "SELECT book, chapter_no, chapter_title, part, MIN(line_start), MAX(line_end), COUNT(*) "
        "FROM chunks_fts WHERE book=? GROUP BY chapter_no, chapter_title, part",
        ("elmish",),
    )
    print(f"{'elmish':7s} {n_chunks:5d} chunks")
    total_chunks += n_chunks

    # glossary term coverage
    for term in GLOSSARY:
        rows = con.execute(
            "SELECT book, chapter_no, chapter_title, COUNT(*) FROM chunks_fts "
            "WHERE chunks_fts MATCH ? GROUP BY book, chapter_no, chapter_title",
            (f'"{term}"',),
        ).fetchall()
        for book, ch_no, ch_title, hits in rows:
            con.execute(
                "INSERT INTO terms VALUES (?,?,?,?,?)", (term, book, ch_no, ch_title, hits)
            )
    n_terms = con.execute("SELECT COUNT(*) FROM terms").fetchone()[0]
    con.execute("INSERT INTO meta VALUES ('books', ?)", ("dmmf,fsa,sf6,elmish",))
    con.commit()
    print(f"total chunks: {total_chunks}, term rows: {n_terms}")
    print(f"db: {DB_PATH}")


def db() -> sqlite3.Connection:
    if not os.path.exists(DB_PATH):
        sys.exit("index missing — run: python3 build_book_index.py build")
    return sqlite3.connect(DB_PATH)


def search(query: str, book: str | None = None, n: int = 8) -> None:
    con = db()
    q, mode = fts_query(query)
    if mode == "like" or not q:
        like = f"%{query}%"
        sql = (
            "SELECT book, chapter_no, chapter_title, section, line_start, line_end, "
            "1.0 AS score, substr(text, max(1, instr(text, ?) - 60), 180) "
            "FROM chunks_fts WHERE text LIKE ? "
        )
        args: list = [query, like]
    else:
        sql = (
            "SELECT book, chapter_no, chapter_title, section, line_start, line_end, "
            "bm25(chunks_fts) AS score, snippet(chunks_fts, 0, '>>', '<<', '…', 18) "
            "FROM chunks_fts WHERE chunks_fts MATCH ? "
        )
        args = [q]
    if book:
        sql += "AND book = ? "
        args.append(book)
    sql += "ORDER BY score LIMIT ?"
    args.append(n)
    for r in con.execute(sql, args):
        print(f"[{r[0]} ch{r[1]} «{r[2][:48]}» §{r[3][:38]} L{r[4]}-{r[5]}]  {r[6]:.2f}")
        print(f"   {r[7][:220]}")
        print()


def term(phrase: str) -> None:
    con = db()
    rows = con.execute(
        "SELECT book, chapter_no, chapter_title, SUM(hits) FROM terms "
        "WHERE term = ? GROUP BY book, chapter_no, chapter_title ORDER BY book, chapter_no",
        (phrase.lower(),),
    ).fetchall()
    if not rows:
        rows = con.execute(
            "SELECT book, chapter_no, chapter_title, SUM(hits) FROM terms "
            "WHERE term LIKE ? GROUP BY book, chapter_no, chapter_title ORDER BY book, chapter_no",
            (f"%{phrase.lower()}%",),
        ).fetchall()
    if not rows:
        print("no glossary hits; try: search")
        return
    cur_book = None
    for book, ch_no, title, hits in rows:
        if book != cur_book:
            print(f"— {book}:")
            cur_book = book
        print(f"   ch{ch_no:>2} «{title}» x{hits}")


def glossary() -> None:
    con = db()
    print(f"{'term':38s} dmmf    fsa   sf6   elmish")
    for term_ in GLOSSARY:
        counts = {}
        for book, hits in con.execute(
            "SELECT book, SUM(hits) FROM terms WHERE term=? GROUP BY book", (term_,)
        ):
            counts[book] = hits
        print(
            f"{term_:38s} {counts.get('dmmf', 0):6d} {counts.get('fsa', 0):5d} "
            f"{counts.get('sf6', 0):5d} {counts.get('elmish', 0):6d}"
        )


def outline(book: str) -> None:
    con = db()
    for ch_no, title, part, ls, le, n in con.execute(
        "SELECT chapter_no, chapter_title, part, line_start, line_end, chunk_count "
        "FROM chapters WHERE book=? ORDER BY chapter_no",
        (book,),
    ):
        p = f" ({part})" if part else ""
        print(f"ch{ch_no:>2} «{title}»{p}  lines {ls}-{le}  chunks {n}")


def chapter(book: str, no: int) -> None:
    con = db()
    row = con.execute(
        "SELECT chapter_title, line_start, line_end, chunk_count FROM chapters "
        "WHERE book=? AND chapter_no=?",
        (book, no),
    ).fetchone()
    if row:
        print(f"{book} ch{no} «{row[0]}» lines {row[1]}-{row[2]} chunks {row[3]}")


def section(key: str) -> None:
    """Find an Elmish Book chapter by title substring, e.g. 'routing'."""
    con = db()
    rows = con.execute(
        "SELECT chapter_no, chapter_title, part, line_start, line_end, chunk_count "
        "FROM chapters WHERE book='elmish' AND chapter_title LIKE ? "
        "ORDER BY chapter_no",
        (f"%{key}%",),
    ).fetchall()
    for ch_no, title, part, ls, le, n in rows:
        print(f"ch{ch_no:>2} «{title}» ({part}) lines {ls}-{le} chunks {n}")


def main(argv: list[str]) -> None:
    if not argv or argv[0] == "build":
        build()
    elif argv[0] == "search":
        q = argv[1]
        book = None
        n = 8
        rest = argv[2:]
        i = 0
        while i < len(rest):
            if rest[i] == "-b":
                book = rest[i + 1]
                i += 2
            elif rest[i] == "-n":
                n = int(rest[i + 1])
                i += 2
            else:
                i += 1
        search(q, book, n)
    elif argv[0] == "term":
        term(argv[1])
    elif argv[0] == "glossary":
        glossary()
    elif argv[0] == "outline":
        outline(argv[1])
    elif argv[0] == "chapter":
        chapter(argv[1], int(argv[2]))
    elif argv[0] == "section":
        section(argv[1])
    else:
        print(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
