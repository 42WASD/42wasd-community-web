#!/usr/bin/env python3
"""Audit the docs/knowledge topic docs (the crash course) against the
source-book index (source set v2: dmmf, fsa, sf6, elmish).

Uses book-index.db (built by build_book_index.py) to ask, for every chapter
of the four source books:

  1. Coverage  — do the chapter's distinctive terms appear in ANY distill doc?
  2. Placement — do they appear in the doc(s) that *should* cover that chapter
                 (the category split defined in CHAPTER_MAP)?
  3. Alignment — per distill doc, do the books dominating its matched terms
                 match the books the doc claims as sources (DOC_BOOKS)?

Run after `build_book_index.py build` whenever sources or guide docs change:

    python3 audit_distill.py [top_terms_per_chapter]

Exit status: 0 = clean, 1 = findings printed.
"""

import collections
import math
import os
import re
import sqlite3
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
DISTILL = os.path.join(REPO, "docs", "knowledge")
DB = os.path.join(HERE, "book-index.db")

BOOKS = {"dmmf": "DMMF", "fsa": "FSA", "sf6": "SF6", "elmish": "Elmish"}

# The intended category split: which distill docs *should* cover each chapter.
# Chapter numbers follow build_book_index.py:
#   dmmf 1..13   fsa 1..27   sf6 1..14   elmish 1..66 (sidebar reading order)
CHAPTER_MAP = {
    "fsa": {
        **{c: ["architecture-thinking"] for c in (1, 2)},
        3: ["architecture-characteristics-and-governance"],
        **{c: ["architecture-characteristics-and-governance"] for c in (4, 5, 6, 7, 8)},
        9: ["architecture-styles", "architecture-characteristics-and-governance"],
        **{c: ["architecture-styles"] for c in (10, 11, 12, 13, 14, 15, 16, 17, 18, 19)},
        20: ["architecture-decisions-and-soft-skills", "architecture-styles"],
        **{c: ["architecture-decisions-and-soft-skills"] for c in (21, 22, 23, 24, 25, 26)},
        27: ["architecture-thinking", "architecture-decisions-and-soft-skills"],
    },
    "dmmf": {
        **{c: ["domain-driven-design"] for c in (1, 2, 3)},
        4: ["domain-modeling-with-types"],
        5: ["domain-modeling-with-types"],
        6: ["domain-modeling-with-types", "fsharp-style"],
        7: ["workflows-and-error-handling"],
        8: ["functions-and-composition"],
        9: ["workflows-and-error-handling", "functions-and-composition"],
        10: ["workflows-and-error-handling"],
        11: ["workflows-and-error-handling", "dev-workflow"],
        12: ["workflows-and-error-handling"],
        13: ["domain-modeling-with-types", "workflows-and-error-handling"],
    },
    "sf6": {
        1: ["fsharp-style"],
        2: ["domain-modeling-with-types", "functions-and-composition"],
        3: ["fsharp-style", "domain-modeling-with-types"],
        4: ["functions-and-composition"],
        5: ["fsharp-style"],
        6: ["fsharp-style"],
        7: ["domain-modeling-with-types"],
        8: ["fsharp-style", "domain-modeling-with-types"],
        9: ["functions-and-composition"],
        10: ["commands-and-async-data", "functions-and-composition"],
        11: ["workflows-and-error-handling"],
        12: ["functions-and-composition"],
        13: ["fsharp-style"],
        14: ["fsharp-style"],
    },
    "elmish": {
        # part 1: fable (chs 1-10)
        **{c: ["fable-and-elm-architecture", "dev-workflow"] for c in (1, 2, 3, 4, 5)},
        **{c: ["fable-and-elm-architecture", "dev-workflow"] for c in (6, 7, 8, 9, 10)},
        # part 2: elm architecture (chs 11-24)
        **{c: ["fable-and-elm-architecture"] for c in range(11, 25)},
        # part 3: commands (chs 25-43)
        **{c: ["commands-and-async-data"] for c in range(25, 44)},
        # part 4: scaling (chs 44-57)
        **{c: ["scaling-and-routing"] for c in range(44, 58)},
        # part 5: dev flow (chs 58-66)
        **{c: ["dev-workflow"] for c in range(58, 67)},
    },
}

# Books each doc claims as sources (for the alignment check).
# The Elmish Book is an F# book: its parts 1–2 teach the same type-driven
# idioms (records, choice types, Result) that the F#-concept docs cover,
# so its vocabulary legitimately dominates those pages.
DOC_BOOKS = {
    "architecture-thinking": {"fsa"},
    "domain-driven-design": {"dmmf", "fsa"},
    "fsharp-style": {"sf6", "elmish"},
    "domain-modeling-with-types": {"dmmf", "sf6", "elmish"},
    "workflows-and-error-handling": {"dmmf", "sf6", "elmish"},
    "functions-and-composition": {"sf6", "dmmf", "elmish"},
    "fable-and-elm-architecture": {"elmish"},
    "commands-and-async-data": {"elmish"},
    "scaling-and-routing": {"elmish"},
    "dev-workflow": {"elmish"},
    "architecture-characteristics-and-governance": {"fsa"},
    "architecture-styles": {"fsa"},
    "architecture-decisions-and-soft-skills": {"fsa"},
}

# Chapters that are front matter / boilerplate, not knowledge.
SKIP = {
    ("dmmf", 0),   # front matter
    ("fsa", 0),    # front matter
    ("fsa", 27),   # mostly generated index in the conversion
    ("sf6", 0),
    ("sf6", 14),   # summary/index-like
}

STOP = frozenset("""
a an the and or of to in is are was were be been being it its this that these
those as at by for from with without into onto over under then than so such
very can could should would may might must shall will do does did done have
has had having i we you your our their his her he she they them there here
when where which who whom what why how not no nor but if because while during
before after above below between through up down out off only own same too
just don now also more most other some any each few both all one two three
first second third next last new old use used uses using useful code example
examples chapter chapters book section sections figure figures listing
listings table tables note tips important summary wrapping topic topics page
pages like gets get got make makes made take takes given need needs needed
want things thing way ways part parts based avoid avoids sure well good
better best see via per etc within across instead rather either neither
following follows show shows shown let our method present small contains
value values type types function functions list string true false int unit
fsharp console writeline printfn static void public class new return null
override get set name names item items using namespace system object
""".split())

# Markup/boilerplate residue from conversions + sample-identifier noise.
ARTIFACT = re.compile(
    r"^(kbd|calibre\d*|kindle|xhtml|codeprefix|emph|inlinecode|cf|ic|"
    r"fnptr|footnote|sup|sub|png|jpg|jpeg|gif|href|span|div|img|images|image|"
    r"ss|sidebar|content|title|alt|src|class|id|http|https|www|com|html|"
    r"target|blank|noopener|valign|width|style|nbsp|quot|amp|apos|lt|gt|"
    r"assets|fsa2|ch0\d+|fig|eq|table|sec|ix|id\d+.*|split_\d+.*|.*_alt|"
    r"fable|elmish|feliz|webpack|npm|node|fsproj|dotnet|json|api|cmd|msg|"
    r"bulma|fontawesome|stylesheet|classname|css|html|browser|script|"
    r"meta|link|rel|href|body|head|div|button|input|element|todoid|"
    r"todo|todos|counter|count|increment|decrement|listing|figure|"
    r"equation|kata|katas|summary|index|appendix|authors|colophon|"
    r"prop|props|dispatch|program|state|update|render|model|view|"
    r"package|packages|package[a-z]*|dependency|dependencies|"
    r"install|installed|version|template|repo|repository|git|github|"
    r"sample|samples|demo|exercise|exercises|reader|readers|author|"
    r"authors|book|books|chapter|chapters|section|sections|part|parts)$"
)

TOKEN = re.compile(r"[A-Za-z][A-Za-z0-9]{2,}")

# Generic vocabulary any topic page legitimately uses; its presence in a
# "wrong" doc is not a placement error. Distinctive-domain terms (specific
# nouns, named concepts, identifiers) are what the audit should police.
GENERIC = frozenset("""
user users started path order classes response contact children architects
architectural text request price org none filter database customer userid
signature init javascript seq class list set map key value data document
code example section application system service component model view
design process project team software development code
callbacks sleep hidden apply element elements click
validated val url unit withreactsynchronous helper
they technical structure start resolved reactelement
""".split())


def toks(text):
    return [t.lower() for t in TOKEN.findall(text)
            if not ARTIFACT.match(t.lower())]


def load_topic_docs():
    docs = {}
    for name in sorted(os.listdir(DISTILL)):
        path = os.path.join(DISTILL, name, "index.md")
        if os.path.isdir(os.path.join(DISTILL, name)) and os.path.exists(path):
            with open(path, encoding="utf-8") as f:
                docs[name] = f.read().lower()
    return docs


def main(k=30):
    if not os.path.exists(DB):
        sys.exit("index missing — run: python3 build_book_index.py build")
    con = sqlite3.connect(DB)
    chapters = con.execute(
        "SELECT book, chapter_no, MIN(chapter_title) FROM chapters "
        "WHERE chapter_no > 0 GROUP BY book, chapter_no ORDER BY book, chapter_no"
    ).fetchall()
    texts = collections.defaultdict(str)
    for book, ch, text in con.execute("SELECT book, chapter_no, text FROM chunks_fts"):
        texts[(book, ch)] += "\n" + text

    counters, df = {}, collections.Counter()
    for book, ch, _ in chapters:
        c = collections.Counter(toks(texts[(book, ch)]))
        counters[(book, ch)] = c
        for t in c:
            df[t] += 1
    n = len(chapters)

    docs = load_topic_docs()
    tokens = {d: set(toks(t)) for d, t in docs.items()}

    def hits(term):
        return [d for d in docs if term in tokens[d]]

    findings = 0
    print(f"audit: {n} source chapters vs {len(docs)} distill topic docs "
          f"(top {k} distinctive terms per chapter)\n")

    top_by = {}
    # Distinctive-term pool: rare enough to discriminate (df <= 75% of
    # chapters) but recurring (df >= 3) — chapter-local sample identifiers
    # (latlon, todoid, checkaddressexists...) live in 1-2 chapters and are
    # not knowledge. Recurring terms are the concepts a distill must carry.
    MIN_DF = 3
    for book, ch, title in chapters:
        if (book, ch) in SKIP:
            print(f"{BOOKS[book]:6s} ch{ch:<2} {title[:44]:46s}  (front matter — skipped)")
            continue
        c = counters[(book, ch)]
        scored = [(f * math.log(1 + n / (1 + df[t])), t)
                  for t, f in c.items() if MIN_DF <= df[t] <= 0.75 * n]
        top = [t for _, t in sorted(scored, reverse=True)[:k]]
        top_by[(book, ch)] = top
        expected = CHAPTER_MAP.get(book, {}).get(ch, [])
        missing, misplaced = [], []
        for t in top:
            if df[t] < 2:
                continue  # chapter-local sample identifier, not shared vocabulary
            if t in GENERIC:
                continue  # generic word: appearing elsewhere is not an error
            h = hits(t)
            if not h:
                missing.append(t)
            elif expected and set(h).isdisjoint(expected):
                misplaced.append((t, h))
        cov = 100 * (len(top) - len(missing)) // max(len(top), 1)
        flag = "  <<" if (missing or misplaced) else ""
        print(f"{BOOKS[book]:6s} ch{ch:<2} {title[:44]:46s} cov {cov:3d}%{flag}")
        if missing:
            findings += 1
            print(f"        missing  : {', '.join(missing[:12])}")
        for t, h in misplaced:
            findings += 1
            print(f"        misplaced: {t} only in {', '.join(h)} "
                  f"(expected {', '.join(expected)})")

    print("\ndoc alignment (source books behind each doc's matched terms):")
    for d in sorted(docs):
        cnt = collections.Counter()
        for (book, ch), top in top_by.items():
            for t in top:
                if t in tokens[d]:
                    cnt[book] += 1
        ranked = ", ".join(f"{BOOKS.get(b, b)}:{c}" for b, c in cnt.most_common())
        claimed = DOC_BOOKS.get(d, set())
        dom = cnt.most_common(1)[0][0] if cnt else None
        ok = not claimed or (dom in claimed)
        flag = "" if ok else "   << DOMINANT BOOK UNCLAIMED"
        if not ok:
            findings += 1
        print(f"  {d:44s} {ranked}{flag}")

    print(f"\nfindings: {findings}")
    sys.exit(1 if findings else 0)


if __name__ == "__main__":
    main(int(sys.argv[1]) if len(sys.argv) > 1 else 30)
