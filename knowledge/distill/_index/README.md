# Knowledge distill toolkit

Scripts that verify `docs/knowledge/` (the published crash course) against
the four source books in `knowledge/md/`:

- `build_book_index.py` — SQLite + FTS5 index over the source books.
  Commands: `build | search | term | glossary | outline | chapter`.
- `audit_distill.py` — TF·IDF coverage/placement/alignment audit of the
  guide against the index DB.
- `book-index.db` — generated; git-ignored (rebuild with `build`).

Paths (`REPO`, `MD`, `DISTILL`) assume the repo layout with the guide at
`docs/knowledge/` and sources at `knowledge/md/`. Everything here is
repo-local tooling — it is not part of the published site.

## Layout: sources vs published guide

```
knowledge/
  md/               ← source ebooks (NOT published; add new books here)
  distill/_index/   ← this toolkit (NOT published; index DB is git-ignored)
docs/knowledge/     ← the distilled crash course (PUBLISHED as a site tab)
```

MkDocs only builds pages under `docs/`, so the distilled guide lives there;
the sources and tooling stay out of the site on purpose.

## Adding a new source book and building more distill

1. **Add the source** — drop the book into `knowledge/md/`.
2. **Register it** — add an entry to `BOOKS` in `build_book_index.py`
   (`"mybook": "Filename.md"`). A new conversion format needs a small parser
   (`parse_ms` / `parse_dmmf` / `parse_elm` are the ~30-line templates); map
   it in `build()`'s `parser` dict. Add its short name to `BOOKS` in
   `audit_distill.py` too.
3. **Rebuild the index** — `python3 build_book_index.py build`.
4. **Distill** — write/extend topic pages in `docs/knowledge/<topic>/index.md`
   using the established shape: `> Source:` header → mermaid mindmap →
   sections → `## Cross-links`.
5. **Update the SSOT reading order** — add the topic to the numbered
   "Reading order" list in `docs/knowledge/index.md`. The nav generator
   (`scripts/docs/docs-generate-nav.py`, `knowledge_nav()`) parses exactly
   that list and hard-fails if a folder on disk is missing from it (or vice
   versa), so the site nav can never silently drift from the guide.
6. **Audit** — update `CHAPTER_MAP` / `DOC_BOOKS` in `audit_distill.py`, then
   `python3 audit_distill.py` to check coverage/placement/alignment of the
   new distill against the new source.
7. **Ship** — `python3 scripts/docs/docs-generate-nav.py`, `git add
   mkdocs.yml` (regenerated output must be committed together), run
   `bash scripts/docs/verify.sh` until `VERIFY OK`, commit. The `docs.yml`
   workflow deploys on push to `main`.
