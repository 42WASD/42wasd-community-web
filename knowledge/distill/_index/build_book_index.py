#!/usr/bin/env python3
"""Book index DB for the docs/knowledge crash course.

A standalone SQLite + FTS5 index over the four source books in knowledge/md/.
Deliberately separate from the docs/ doc-impact index — same technique
(FTS5 with porter stemming + bm25), different database.

Purpose: while distilling one book at a time, use this index to quickly look
up where a term/concept appears across ALL books, so related knowledge stays
together in the same guide documents. Also powers audit_distill.py, which
verifies the published guide (docs/knowledge/) against the sources.

Usage (from repo root or this folder):
  python3 build_book_index.py build              # (re)build _index/book-index.db
  python3 build_book_index.py search "two-track" # bm25-ranked chunk search
  python3 build_book_index.py term "aggregate"   # cross-book term coverage
  python3 build_book_index.py glossary           # all glossary terms x books
  python3 build_book_index.py outline elm        # chapters + sections of a book
  python3 build_book_index.py chapter elm 4      # line range of a chapter in source

Books and their ids: blazor, dmmf, elm, maui.
"""

from __future__ import annotations

import os
import re
import sqlite3
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MD = os.path.join(REPO, "knowledge", "md")
DB_PATH = os.path.join(HERE, "book-index.db")

BOOKS = {
    "blazor": "Blazor for ASP.NET Web Forms Developers.md",
    "dmmf": "Domain Modeling Made Functional.md",
    "elm": "Elm in Action.md",
    "maui": "Enterprise Application Patterns Using .NET MAUI.md",
}

TAG_RE = re.compile(r"<[^>]*>")
PAGEBREAK = "\x0c"

# MS ebooks (blazor, maui): running page headers like  "16 CHAPTER 4 \| Project structure ..."
MS_FOOTER_RE = re.compile(r"^\s*(\d{1,4})\s+CHAPTER\s+(\d+)\s+\\+\s*\|\s*([^\\\n]{3,90})")


def ms_chapter_title(title: str) -> str:
    """A page-header line ends the previous chapter and starts a new one.

    Normalize chapter titles of MS ebook page headers: drop everything from the
    first pagebreak onward and trim junk so chapter titles are clean.
    """
    title = title.split(PAGEBREAK)[0]
    return title.strip()

# DMMF: '# <span class="chapter-number"> Chapter 1</span> <span class="chapter-name">Title</span>'
DMMF_CH_RE = re.compile(r"chapter-number\">\s*Chapter\s+(\d+).*?chapter-name\">([^<]+)")
DMMF_PART_RE = re.compile(r"part-number\">([^<]+)</span>\s*([^<\n]+)")

# Elm: '## Chapter 2. <span ...></span>Your first Elm application'
ELM_CH_RE = re.compile(r"^##\s+Chapter\s+(\d+)\.\s*(?:<[^>]*></span>)?\s*(.*)$")
ELM_SEC_RE = re.compile(r"^##\s+(\d+\.\d+\.?\d*\.\s*.*)$")
ELM_PART_RE = re.compile(r"^##\s+Part\s+(\d+)\.\s*(?:<[^>]*></span>)?\s*(.*)$")


def clean(text: str) -> str:
    text = text.replace(PAGEBREAK, "\n")
    text = TAG_RE.sub("", text)
    text = text.replace("\u00a0", " ")
    return text


# --- FTS5 query handling -----------------------------------------------------
# FTS5 treats -, ., ! etc. inside raw queries as syntax (or as token
# separators, silently matching different words: "let!" matches "lets").
# The tokenizer (porter unicode61) splits identifiers at these characters,
# so a faithful phrase match is impossible for them. Strategy:
#   * split the user query into alphanumeric word tokens
#   * single token  -> plain term query (stemmed, matches any form)
#   * multi token   -> quoted phrase query (exact word sequence)
#   * queries that are pure punctuation/syntax ("let!", "C#") fall back to a
#     substring LIKE scan over chunk text so they still return real hits.
WORD = re.compile(r"[A-Za-z0-9]+")
PURE_WORDS = re.compile(r"[\w\s]+\Z")


def fts_query(query: str) -> tuple[str, str]:
    """Return (fts MATCH expression, mode) for a user query.

    mode is 'fts' or 'like' (LIKE uses the raw query as a literal substring).
    Punctuated queries ("let!", "Html.Lazy", "two-track", "C#") MUST take the
    LIKE path: FTS5's porter stemmer strips punctuation, so 'let!' indexes as
    'let' and also matches 'lets' (197 false hits in this corpus). Pure
    word/whitespace queries use the FTS index (phrase-quoted when multi-word).
    """
    if PURE_WORDS.match(query.strip()) and WORD.search(query):
        words = WORD.findall(query)
        if len(words) == 1:
            return words[0], "fts"
        return f'"{" ".join(words)}"', "fts"
    return "", "like"


def parse_dmmf(path: str):
    """Yield (chapter_no, chapter_title, part, line_no, text_fragment) pieces."""
    part = ""
    with open(path, encoding="utf-8") as f:
        lines = f.readlines()
    cur = (0, "Front matter")
    buf: list[tuple[int, str]] = []
    for i, line in enumerate(lines, 1):
        if line.startswith("# ") and "chapter-number" in line:
            m = DMMF_CH_RE.search(line)
            if m:
                if buf:
                    yield (*cur, part, buf)
                cur = (int(m.group(1)), m.group(2).strip())
                buf = []
                continue
        if line.startswith("# ") and "part-number" in line:
            m = DMMF_PART_RE.search(line)
            if m:
                part = f"{m.group(1).strip()} {m.group(2).strip()}"
        buf.append((i, line))
    if buf:
        yield (*cur, part, buf)


def parse_elm(path: str):
    with open(path, encoding="utf-8") as f:
        lines = f.readlines()
    cur = (0, "Front matter")
    part = ""
    buf: list[tuple[int, str]] = []
    for i, line in enumerate(lines, 1):
        if line.startswith("## "):
            m = ELM_CH_RE.match(line.strip())
            if m and "kindle_split" not in line.split("Chapter")[0]:
                if buf:
                    yield (*cur, part, buf)
                title = TAG_RE.sub("", m.group(2)).strip()
                cur = (int(m.group(1)), title)
                buf = []
                continue
            m = ELM_PART_RE.match(line.strip())
            if m:
                part = f"Part {m.group(1)} {TAG_RE.sub('', m.group(2)).strip()}"
        buf.append((i, line))
    if buf:
        yield (*cur, part, buf)


def parse_ms(path: str):
    """MS ebooks: attribute page bodies to their chapter.

    A page-header line looks like:  "16 CHAPTER 4 \\| Project structure ... <page text>".
    The page break (\x0c) inside it separates the header from the new page's
    body. Pages of the same chapter are aggregated into ONE fragment list so
    downstream section matching can span the whole chapter.
    """
    with open(path, encoding="utf-8") as f:
        lines = f.readlines()
    cur = (0, "Front matter")
    buf: list[tuple[int, str]] = []

    for i, line in enumerate(lines, 1):
        m = MS_FOOTER_RE.match(line)
        if m:
            head, _, tail = line.partition(PAGEBREAK)
            new_cur = (int(m.group(2)), ms_chapter_title(m.group(3)))
            if new_cur != cur and buf:
                yield (*cur, "", list(buf))  # copy: buf keeps accumulating
                buf = []
            cur = new_cur
            # The running header belongs to the page it opens.
            buf.append((i, head.strip() + "\n"))
            # The pagebreak merges the new page's first text onto this line;
            # give it its own line so flattened section headings become
            # findable as contiguous text (e.g. "...developers \x0cClient-side
            # web development All of the ...").
            if tail.strip():
                buf.append((i, tail.lstrip() + "\n"))
            continue
        buf.append((i, line))
    if buf:
        yield (*cur, "", list(buf))


# TOC dot-leader line:  "Some Heading ...... 12"  ('.' or '…' leaders)
TOC_HEAD_RE = re.compile(r"([A-Z][^…\.\n]{3,85}?)\s*[…\.]{4,}\s*\d{0,4}")


def harvest_toc(path: str) -> list[str]:
    """Harvest the ordered TOC heading list from an MS ebook's front matter.

    The MS ebook conversion flattens real headings into running text, so the
    book's own dot-leader TOC is the only usable heading list.
    """
    with open(path, encoding="utf-8") as f:
        front = f.read(24000)  # TOC lives in the front matter
    heads: list[str] = []
    seen: set[str] = set()
    for m in TOC_HEAD_RE.finditer(front):
        h = " ".join(m.group(1).split())
        # skip entries that name chapters rather than sections (they duplicate
        # the chapter_title we already have)
        if 3 < len(h) < 85 and h.lower() not in seen:
            seen.add(h.lower())
            heads.append(h)
    return heads


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
    # functional (dmmf / elm)
    "total function", "composition", "pipe", "function type", "type annotation",
    "type alias", "custom type", "case expression", "pattern matching", "record",
    "list", "tuple", "option", "Result", "two-track", "bind", "map", "monad",
    "computation expression", "async", "dependency injection", "partial application",
    "currying", "higher-order function", "recursion", "dictionary", "recursive type",
    "tree", "immutability", "pure function", "side effect", "repl", "let",
    # Elm architecture (elm)
    "The Elm Architecture", "model", "update", "view", "message", "dispatch",
    "command", "subscription", "flags", "port", "decoder", "encoder", "Html.Lazy",
    "virtual DOM", "sandbox", "element", "custom element", "Browser.element",
    "Random.Generator", "Http.request", "elm/http", "elm/json", "elm/url",
    "routing", "single-page application", "keyed", "fuzz test", "unit test",
    "expect", "elm-test", "compiler as assistant", "refactoring",
    # Blazor (blazor)
    "component", "Razor", "render tree", "parameter", "cascading value", "layout",
    "page", "route", "NavigationManager", "hosting model", "Blazor Server",
    "Blazor WebAssembly", "signalr", "circuit", "dependency injection", "middleware",
    "configuration", "appsettings", "environment", "authentication", "authorization",
    "Identity", "EditForm", "validation", "DataAnnotations", "state management",
    "localStorage", "EF Core", "Entity Framework", "HttpClient", "JSON",
    "Web Forms", "code-behind", "lifecycle", "event callback", "generic component",
    "templated component", "migration",
    # MAUI (maui)
    "MVVM", "ViewModel", "Model", "view model", "observable", "INotifyPropertyChanged",
    "RelayCommand", "AsyncRelayCommand", "source generator", "messenger", "message",
    "navigation service", "behavior", "validation rules", "settings", "preferences",
    "microservice", "containerization", "REST", "caching", "retry", "circuit breaker",
    "resilience", "bearer token", "IdentityServer", "OIDC", "authorization",
    "unit testing", "mock", "xUnit",
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
    for book_id, fname in BOOKS.items():
        path = os.path.join(MD, fname)
        parser = {"dmmf": parse_dmmf, "elm": parse_elm}.get(book_id, parse_ms)
        toc_heads = harvest_toc(path) if book_id in ("blazor", "maui") else []
        n_chunks = 0
        for ch_no, ch_title, part, frags in parser(path):
            chunks = chunkify(frags)
            # section attribution:
            #  - dmmf: real '## ' markdown headings in the source
            #  - elm:  '## N.M.' section headings
            #  - ms:   headings flattened into running text; match the TOC
            #          heading list against chunk text in reading order
            sections: dict[int, str] = {}
            if book_id in ("blazor", "maui") and ch_no:
                # Headings are flattened into running text, so the book's own
                # dot-leader TOC supplies the heading list. Locate each head's
                # first occurrence in the chapter's flattened text and assign
                # sections by position: heads that never occur verbatim are
                # skipped without stalling the sequential scan.
                tl = ch_title.casefold()
                heads = [
                    h
                    for h in toc_heads
                    # drop wrap fragments / entries that are the chapter
                    # title itself (they pollute the TOC harvest)
                    if h.casefold() != tl and not tl.endswith(h.casefold())
                ]
                flat_chunks = [" ".join(t.split()) for _, _, t in chunks]
                offsets: list[int] = []
                pos = 0
                for f in flat_chunks:
                    offsets.append(pos)
                    pos += len(f) + 1
                full = " ".join(flat_chunks)
                found = sorted(
                    (at, h) for h in heads if (at := full.find(h)) != -1
                )
                j = 0
                cur_sec = ""
                for idx, (ls, _le, _t) in enumerate(chunks):
                    end = offsets[idx] + len(flat_chunks[idx]) + 1
                    while j < len(found) and found[j][0] < end:
                        cur_sec = found[j][1]
                        j += 1
                    if cur_sec:
                        sections[ls] = cur_sec
            else:
                sec_lines: list[tuple[int, str]] = []
                for ln, txt in frags:
                    s = txt.strip()
                    if s.startswith("## ") or s.startswith("### "):
                        t = TAG_RE.sub("", s).lstrip("#").strip()
                        if not (3 < len(t) < 90):
                            continue
                        # elm sections are level-3 numbered headings
                        # ("### <span/>1.1. How Elm fits in"); require the
                        # N.M. prefix there, plain prose subsections in dmmf.
                        if book_id == "elm" and s.startswith("### ") and not re.match(r"^\d+\.\d+", t):
                            continue
                        sec_lines.append((ln, t))
                for (ls, _le, _text) in chunks:
                    for sln, st in sec_lines:
                        if sln <= ls:
                            sections[ls] = st
                        else:
                            break
            for (ls, le, text) in chunks:
                con.execute(
                    "INSERT INTO chunks_fts VALUES (?,?,?,?,?,?,?,?)",
                    (text, book_id, ch_no, ch_title, part, sections.get(ls, ""), ls, le),
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
    con.execute("INSERT INTO meta VALUES ('books', ?)", (",".join(BOOKS),))
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
        # punctuation-only or syntax query: substring scan (exact text match)
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
            print(f"— {BOOKS.get(book, book)}:")
            cur_book = book
        print(f"   ch{ch_no:>2} «{title}» x{hits}")


def glossary() -> None:
    con = db()
    print(f"{'term':38s} blazor  dmmf   elm   maui")
    for term_ in GLOSSARY:
        counts = {}
        for book, hits in con.execute(
            "SELECT book, SUM(hits) FROM terms WHERE term=? GROUP BY book", (term_,)
        ):
            counts[book] = hits
        print(
            f"{term_:38s} {counts.get('blazor', 0):6d} {counts.get('dmmf', 0):5d} "
            f"{counts.get('elm', 0):5d} {counts.get('maui', 0):6d}"
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
    else:
        print(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
