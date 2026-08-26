# Agent Instructions

You are working in a MkDocs documentation project built on a
**Single-Source-of-Truth (SSOT)** reading-order manifest, guarded by a
deterministic verification toolkit. Follow these conventions.

## Debugging: always use the MVU log trace first

When something is wrong at runtime (data not loading, wrong rendering, a page
misbehaving, a 401/404, an update not firing, etc.), **always debug with the
Elmish MVU message trace first** before writing new code or guessing:

1. Open the browser console (the Elmish trace runs there, not the server
   terminal).
2. Read the `dispatch` log: which `Message`s fire, in what order, and what they
   carry (e.g. `GetGames` → `GotGames`). Confirm each expected message is
   dispatched and arrives.
3. Use that trace to locate where the flow breaks (message never dispatched, an
   `Error`/`Recv...` arrives with the wrong payload, an update drops state,
   etc.) **before** changing any code.
4. Only after the MVU trace has pinpointed the faulty step do you fix that step.

Rule of thumb: trace first, fix second. Do not skip the console trace and jump
straight to edits.

## The core rule: the manifest is the source of truth

`docs/reference-design/_sequence.yaml` is the **only** place ordering is
defined. All numerals (I, II, III…) and phase numbers (1, 2, 3…) are
**derived from list position** — never stored in page files. To add, remove, or
reorder any part/section/phase:

1. Edit `docs/reference-design/_sequence.yaml`.
2. Re-run `bash scripts/docs/verify.sh` (regenerates nav + progress and proves
   correctness).

Do **not** hand-edit generated output (`mkdocs.yml` nav block,
`docs/implementation/index.md`) or add number prefixes to page H1s — the
generators own those.

## Always run the verification pipeline — MANDATORY before commit

- **Before committing ANY change** to docs, the manifest, a generator, or
  `mkdocs.yml`, you MUST run the full verification pipeline and it MUST pass:

  ```bash
  bash scripts/docs/verify.sh          # full: validate -> tests -> strict build
  bash scripts/docs/verify.sh --stage  # skip the slow mkdocs build (fast)
  ```

  This is exactly what CI runs, so **local = CI**. A change is not "done" until
  `verify.sh` reports **`VERIFY OK`**. Never commit, open a PR, or push if the
  pipeline fails or was skipped.
- The **golden test** asserts generators are idempotent: it fails if committed
  generated output (`mkdocs.yml` nav, `docs/implementation/index.md`) doesn't
  match what the generators produce. When you edit the manifest or a generator,
  you must regenerate and **commit the regenerated output together** with the
  change.
- If you only changed docs/markdown and want a fast loop before the final
  `verify.sh`, you may run `verify.sh --stage`; but the **full** `verify.sh`
  must still pass once before committing.

## Python environment — always use uv

- Dependencies live in `projects/` (`pyproject.toml` + `uv.lock`).
- Install/update deps: `cd projects && uv sync`.
- Run tools without activating: `uv run <cmd>` (e.g.
  `uv run mkdocs build --strict -f ../mkdocs.yml`).
- Do **not** use `pip`, `python3 -m venv`, or hand-written `requirements.txt`.
- When adding a plugin/dependency, add it to `projects/pyproject.toml` and let
  `uv sync` update the lockfile.

## Repo layout

```
mkdocs.yml                      # config + strict validation block
docs/
  index.md
  reference-design/
    _sequence.yaml              # SSOT manifest (edit this)
    <part>/<section>/index.md   # content pages
  implementation/
    progress.yaml               # status per phase
    index.md                    # GENERATED progress page (don't hand-edit)
scripts/docs/
  docs_manifest.py              # loader + structural validation
  docs-generate-nav.py          # nav generator (SSOT-driven)
  docs-generate-implementation.py
  verify.sh                     # one-command pipeline (local = CI)
  README.md                     # the full technique guide
projects/
  pyproject.toml                # uv + pytest
  tests/                        # golden + parity tests
.github/workflows/              # CI: verify + deploy pages
```

## Launching a dev/test server — always use a new port

The dev server defaults to `http://localhost:5023` (from
`launchSettings.json`). To avoid "address already in use" crashes from a stale
or leftover process, follow these rules every time you start a server for
testing:

1. **Always pass an explicit `--urls` port** when launching. Never rely on the
   default, and never reuse a port you've already used this session.
2. **Increment by +1** from the last port you used. The first launch this
   session uses `5023`; the next `5024`; then `5025`, etc. (e.g.
   `dotnet run --project src/Community.Web.Server --urls http://localhost:5024`).
3. **Don't assume the port is free** — if the bind fails with
   "address already in use", increment again rather than killing whatever
   holds the port, unless you know it is a stale server you started.
4. Kill leftover servers you started **only** once you've confirmed via
   `ps`/`lsof` that they're yours, and prefer bumping the port over killing
   unless the conflict is genuinely your own stale process.
5. **If a page takes more than ~3 seconds to load or the browser appears
   stuck, bump the port by +1** and relaunch the server on the new port (per
   rule 2), then point the browser at the new port. A slow/hanging load is
   treated as a stuck server — do not keep retrying the same port.

The browser page you drive must always navigate to the **current** port you
started (the URLs must stay in sync).

## Adding a new validation check

1. Prefer `validate()` in `scripts/docs/docs_manifest.py` for structural
   invariants (slug / phase / file).
2. Otherwise add a deterministic `test_*` in `projects/tests/`.
3. For link/anchor/orphan concerns, tune the `validation:` block in
   `mkdocs.yml` instead of writing Python.

## Plugin philosophy

Keep the plugin set lean. Only add a plugin to `mkdocs.yml` +
`projects/pyproject.toml` when a real need appears — every plugin adds a
dependency, a failure surface, and warning risk against `--strict`.

Read `scripts/docs/README.md` for the full technique and the four verification
layers.

## Bolero work — REUSE before you write

When working with the Bolero codebase or writing F# for it:

1. **Read the Bolero source first** (`thirdparty/Bolero`) — it is the
   authoritative reference for how anything works.
2. **Reuse Bolero's existing functions/patterns** (Elmish hooks, Router,
   RemoteHandler, `Cmd` helpers). Do not write new F# functions before checking
   what Bolero already provides.
3. Debugging: the Elmish message trace runs in the **browser console**, not the
   server terminal.