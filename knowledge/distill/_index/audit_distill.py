#!/usr/bin/env python3
"""Audit the docs/knowledge topic docs (the crash course) against the
source-book index.

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

BOOKS = {"blazor": "Blazor", "dmmf": "DMMF", "elm": "Elm", "maui": "MAUI"}

# The intended category split: which distill docs *should* cover each chapter.
CHAPTER_MAP = {
    "blazor": {
        1: ["web-forms-migration"], 2: ["web-forms-migration"],
        3: ["blazor-app-services"],
        4: ["blazor-components", "blazor-app-services"],
        5: ["blazor-app-services"],
        6: ["blazor-components"], 7: ["blazor-components"],
        8: ["blazor-app-services"], 9: ["blazor-components"],
        10: ["remote-data-and-security"],
        11: ["web-forms-migration"],
        12: ["blazor-app-services"],
        13: ["remote-data-and-security", "web-forms-migration"],
        14: ["web-forms-migration"],
    },
    "dmmf": {
        **{c: ["domain-driven-design"] for c in (1, 2, 3)},
        **{c: ["functional-design-and-types"] for c in (4, 5, 6)},
        **{c: ["workflows-and-error-handling"] for c in (7, 8, 9, 10)},
        **{c: ["persistence-and-evolution"] for c in (11, 12, 13)},
    },
    "elm": {
        **{c: ["elm-architecture"] for c in (1, 2, 3, 4, 5)},
        6: ["testing-practices", "elm-in-production"],
        7: ["elm-in-production"], 8: ["elm-in-production"],
    },
    "maui": {
        **{c: ["mvvm-patterns"] for c in (2, 3, 4, 5, 6, 7, 8, 12)},
        **{c: ["remote-data-and-security"] for c in (9, 10, 11)},
        13: ["testing-practices"],
    },
}

# Books each doc claims as sources (for the alignment check).
DOC_BOOKS = {
    "domain-driven-design": {"dmmf"},
    "functional-design-and-types": {"dmmf"},
    "workflows-and-error-handling": {"dmmf"},
    "persistence-and-evolution": {"dmmf"},
    "elm-architecture": {"elm"},
    "elm-in-production": {"elm"},
    "blazor-components": {"blazor"},
    "blazor-app-services": {"blazor"},
    "web-forms-migration": {"blazor"},
    "mvvm-patterns": {"maui"},
    "remote-data-and-security": {"maui", "blazor"},
    "testing-practices": {"elm", "maui", "dmmf"},
}

# Chapters that are pure front matter (audience/scope/how-to-use), not knowledge.
SKIP = {("maui", 1)}

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
above following follows show shows shown let our method present small
contains deciding scaled specifies operating instant
""".split())

# Markup/boilerplate residue from the ebook→markdown conversions, plus
# sample-app identifier noise that carries no knowledge.
ARTIFACT = re.compile(
    r"^(kbd|calibre\d*|kindle|calibre_|xhtml|codeprefix|emph|inlinecode|cf|ic|"
    r"fnptr|footnote|sup|sub|png|jpg|jpeg|gif|href|span|div|img|images|image|"
    r"ss|sidebar|content|title|alt|src|class|id|http|https|www|com|html|"
    r"target|blank|noopener|valign|width|style|nbsp|quot|amp|apos|lt|gt|"
    r"iddle|split_\d+.*|.*_alt)$"
)

TOKEN = re.compile(r"[A-Za-z][A-Za-z0-9]{2,}")


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
    for book, ch, title in chapters:
        if (book, ch) in SKIP:
            print(f"{BOOKS[book]:6s} ch{ch:<2} {title[:44]:46s}  (front matter — skipped)")
            continue
        c = counters[(book, ch)]
        scored = [(f * math.log(1 + n / (1 + df[t])), t)
                  for t, f in c.items() if df[t] <= 0.75 * n]
        top = [t for _, t in sorted(scored, reverse=True)[:k]]
        top_by[(book, ch)] = top
        expected = CHAPTER_MAP.get(book, {}).get(ch, [])
        missing, misplaced = [], []
        for t in top:
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
        print(f"  {d:28s} {ranked}{flag}")

    print(f"\nfindings: {findings}")
    sys.exit(1 if findings else 0)


if __name__ == "__main__":
    main(int(sys.argv[1]) if len(sys.argv) > 1 else 30)
