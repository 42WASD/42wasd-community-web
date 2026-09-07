# Knowledge distill toolkit

Scripts that verify `docs/knowledge/` (the published crash course) against
the four source books:

- `build_book_index.py` — SQLite + FTS5 index over the sources.
  Commands: `build | search | term | glossary | outline | chapter | section`.
- `audit_distill.py` — TF·IDF coverage/placement/alignment audit of the
  guide against the index DB.
- `book-index.db` — generated; git-ignored (rebuild with `build`).

Paths (`REPO`, `MD`, `ELMISH`, `DISTILL`) assume the repo layout with the
guide at `docs/knowledge/`. Everything here is repo-local tooling — it is
not part of the published site.

## Source set v2 (2026-09-07)

| id | Book | Form | Parser |
| --- | --- | --- | --- |
| `dmmf` | Domain Modeling Made Functional | pandoc → `knowledge/md/domain-modeling-made-functional.md` | H1 `# Chapter N Title` + `# Part N` |
| `fsa` | Fundamentals of Software Architecture | pandoc → `knowledge/md/fundamentals-of-software-architecture.md` | H1 `# Chapter N. Title` + `# Part I.` |
| `sf6` | Stylish F# 6 | pandoc → `knowledge/md/stylish-fsharp-6.md` | H1 `# N. Title` (no parts) |
| `elmish` | The Elmish Book (Zaid Ajaj) | cloned repo `knowledge/src/the-elmish-book/` (`.git` removed) | `chapters/**.md` in the sidebar order hardcoded in `ELMISH_ORDER` (copied from `index.html`) |

Epub conversion: `pandoc BOOK.epub -t gfm-raw_html --wrap=none -o knowledge/md/slug.md`.
The DMMF conversion had piracy-watermark lines (`sanet.cd`) — deleted with
`sed -i '/sanet\.cd/d'` after conversion. Sources are git-ignored
(copyright): `knowledge/md/` and `knowledge/src/` never get committed.

## Layout: sources vs published guide

```
knowledge/
  md/               ← epub conversions (NOT published)
  src/              ← epubs + cloned book repos (NOT published)
  distill/_index/   ← this toolkit (NOT published; index DB is git-ignored)
docs/knowledge/     ← the distilled crash course (PUBLISHED as a site tab)
```

MkDocs only builds pages under `docs/`, so the distilled guide lives there;
the sources and tooling stay out of the site on purpose.

## Rebuilding the index

```
python3 build_book_index.py build
python3 build_book_index.py search "two-track"       # FTS; punctuated queries
python3 build_book_index.py search "let!"            # take the LIKE path
python3 build_book_index.py outline elmish           # reading-order chapters
python3 audit_distill.py 20                          # coverage/placement/alignment
```

Audit notes: distinctive terms must recur across chapters (`MIN_DF = 3`) so
chapter-local sample identifiers don't dominate; the Elmish book is claimed
as a source of the F#-idiom pages because it teaches those idioms too.
Findings are steering, not gates — read them, don't chase 100%.

## Adding a new source book and building more distill

1. **Add the source** — epub: convert with pandoc into `knowledge/md/`.
   Repo-hosted book: clone into `knowledge/src/`, **remove `.git`**.
2. **Register it** — add a parser + entry in `PARSERS` (or an `ELMISH_ORDER`
   -style section list) in `build_book_index.py`. Extend `BOOKS`,
   `CHAPTER_MAP`, `DOC_BOOKS` in `audit_distill.py`.
3. **Rebuild the index** — `python3 build_book_index.py build`.
4. **Distill** — write/extend topic pages in `docs/knowledge/<topic>/index.md`
   using the established shape: `> Source:` header → mermaid **flowchart LR**
   (boxed nodes, thin edges; NOT mindmaps — mermaid 11 mindmap section edges
   are unstyleable and paint over labels) → sections → `## Cross-links`.
   Style: bite-size, rule-first bullets, ❌/✅ examples.
5. **Update the SSOT reading order** — add the topic to the numbered
   "Reading order" list in `docs/knowledge/index.md`. The nav generator
   (`scripts/docs/docs-generate-nav.py`, `knowledge_nav()`) parses exactly
   that list and hard-fails if a folder on disk is missing from it (or vice
   versa), so the site nav can never silently drift from the guide.
6. **Audit** — `python3 audit_distill.py` to check coverage/placement/
   alignment of the distill against the sources; fix real gaps it reveals.
7. **Ship** — `uv run --project projects python scripts/docs/docs-generate-nav.py`,
   `git add mkdocs.yml` (regenerated output must be committed together), run
   `bash scripts/docs/verify.sh` until `VERIFY OK`, commit. The `docs.yml`
   workflow deploys on push to `main`.
