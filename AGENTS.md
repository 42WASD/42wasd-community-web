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

## Local builds — output inside the repo, never /tmp

- **All local build/publish output goes in a repo-local disposable folder:**
  `.tmp/` (e.g. `dotnet publish ... -o .tmp/aot-serve`). This folder is
  git-ignored and safe to delete anytime: `rm -rf .tmp/`.
- **NEVER build to `/tmp`** or other system temp dirs. Previous attempts to
  publish to `/tmp` with high parallelism (`-m:16`) consumed all system RAM
  and crashed the machine. Keep heavy AOT/publish work inside the repo where
  it is easy to clean up and is subject to normal `.gitignore` rules.
- **Cap parallelism conservatively.** Use `-m:4` (or `nproc`/2) at most.
  AOT (`wasm-ld`) and msbuild each spawn many processes; high `-m` values
  multiply memory usage dramatically on AOT publishes. On an 8-core box,
  `-m:4` is a safe ceiling for a `RunAOTCompilation=true` publish.

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

## Elevated privileges (sudo)

When a task needs root/sudo (e.g. installing a system-wide .NET SDK, apt
packages, writing to `/usr/lib`, `/usr/local`, `/var`, etc.):

1. **Always run the `sudo` command directly** and prompt the user to enter the
   password interactively in the terminal.
2. **NEVER work around missing sudo** — no "pending", no alternative approach
   (e.g. installing to `~/.dotnet` to avoid sudo), no skipping. Ask for sudo,
   let the user authenticate, and continue.
3. Run **one sudo command at a time**, wait for it to complete, then continue.
4. Secrets never pass through the model: the password is typed straight into
   the terminal by the user.

Rule of thumb: elevated work always goes through sudo; the user authenticates
interactively.

## Password-based SSH / remote commands (sshpass)

- When a remote host must be reached with a **password** (no key installed),
  never ask for it in chat for the first connection — prompt the user to run
  it interactively, or use key-based auth / `BatchMode` and let it fail
  cleanly.
- **Once the user has typed the password into the chat** (a later turn), you
  may use `sshpass` for subsequent commands non-interactively. Read it into
  an env var rather than inlining it:
  ```bash
  SSHPASS='<password-from-user>' sshpass -e ssh -o StrictHostKeyChecking=accept-new user@host 'command'
  ```
- `sshpass -e` (env var) over `sshpass -p '<pw>'` — keeps the password out of
  `ps` output and shell history.
- For recurring access, prefer installing a key (`ssh-copy-id`) so later
  turns need no password at all. Never echo the password back, never commit
  it, never store it in a file inside the repo.

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

## Knowledge pages (docs/knowledge/) — tutorial-first, always

The crash course exists to **teach the repo owner**, not to summarize the
source books. The owner learns from **narrative and fully worked examples
anchored in real (ideally repo-local) code**; factually-accurate rule lists
teach him nothing. This is a hard requirement, learned the hard way: the v2
pages were accurate but unreadable for learning ("scattered nuggets", no
story) and were rejected wholesale by the reader they were written for.

The proven format (owner-endorsed: **BBC Bitesize "learn" pages**; evidence:
the **worked-example effect** — the most replicated result of cognitive load
theory; novices learn from studying fully worked solutions, not from
exercises) is chunked and example-dense, with **no homework**:

1. **Open with a story, not a definition.** A real type/feature and the bug
   or pain it invites. Prefer this repo's own code as the protagonist
   (`Tournament.registrationOpen`, `RemoteData<'T>`, `Msg` DUs, `actionMsg`,
   `Cmd.OfAsync.either`).
2. **Define every technical term at first use, Bitesize-style** — one plain
   English sentence the moment the term appears (owner verdict 2026-10-14:
   "BBC Bitesize never fail to explain or define in a very easy to
   understand way"). No term is used before it is defined.
3. **✅ examples must be textbook-perfect F#** — taken from the source
   books (DMMF's `PaymentMethod`/`OrderId`/`CustomerEmail`, SF6's
   canonical forms) or blatantly clear canonical forms, NOT shapes copied
   from this repo (owner verdict 2026-10-14: repo code "might not be
   perfect example of F#"). Repo code appears only as the ❌ evidence of
   what's wrong.
4. **Teach in numbered chunks, Bitesize-style.** Each chunk = short point →
   ❌ the common mistake in real code → ✅ the correct F#, worked. The
   worked example is the teaching instrument itself, not a lead-in to
   practice. **Keep examples Bitesize-weight** (owner verdict 2026-10-14:
   long annotated examples are "boring, unimpressionable, hard to remember,
   too verbose"): ≤10 lines each, ONE point per block, the why in one plain
   prose sentence — never comment walls or per-line "why" annotations
   inside the code. Real Bitesize chemistry pages teach with one plain
   sentence + one small diagram; match that weight.
5. **Frame decisions the way the owner makes them**: process of elimination
   toward a stated product goal, trade-offs on the table, the *why* recorded.
   No hero-narratives, no role-talk, no abstract hypotheticals without
   grounded truth.
6. **No exercises, no call-to-action.** The owner explicitly rejected
   end-of-page "now go do X" sections (2026-10-14): pages teach *what is
   wrong and what correct F# looks like*, fully worked. Never assign work;
   never end the page at a demand.
7. **The mermaid flowchart is the Revise section, not the lesson.** Keep the
   diagram block byte-identical (existing rule), but place it at the END as
   the "Revise" review map (Bitesize's closing step), after the narrative.
   Walking its nodes in order is the encouraged structure for the tutorial
   body.
8. **❌/✅ snippets belong inside the narrative** as before/after evidence —
   never as free-floating bullet nuggets.

Bite-size rule-first bullets, comparison tables, and one-line concept
statements are a **review layer** — fine for `docs/reference-design/`, never a
teaching layer for `docs/knowledge/`.

**The full evidence-graded ruleset lives in
`knowledge/distill/_index/TUTORIAL-RULES.md`** — read it before writing or
rewriting a knowledge page. It covers tutorial format (segmenting,
pre-training, coherence, redundancy, signalling, contiguity, concreteness
fading, worked examples, plain language), the distillation macrorules for
books (Brown & Day: deletion, redundancy, superordination, selection,
invention), the compression budget (20–35%, never minimal), what must never
be cut (causal chain, failure case, boundary conditions, trade-offs), and
an explicit list of what the evidence does *not* support (SQ3R, learning
styles, "shorter is always better").

## Bolero work — REUSE before you write

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
## Argo CD health for on-prem Traefik ingresses

The platform's Argo CD reports `Synced / Healthy` for this app thanks to an
Ingress health customization in the iac repo
(`infra/kubernetes/bootstrap/argocd/argocd-config.yaml`): on-prem Traefik
never sets `status.loadBalancer`, so the built-in Ingress health check
would keep the app `Progressing` forever. If a future app of this repo
shows `Progressing` with all pods Running, check that customization
first — not the workload.

## Doc-impact — check docs for staleness after live commands

- **After any successful run of new/implementing commands** (anything that
  installed, configured, created, renamed, scaled, or removed something on
  the host or cluster), run a quick doc-impact check **before finishing the
  turn**:

  **Step 1 — smart diff:** describe what changed and retrieve the docs that
  talk about the same things (hybrid FTS5-BM25 + fuzzy over a committed
  SQLite index):
  ```bash
  cd projects
  uv run python ../scripts/docs/doc-impact/impact_search.py "what changed"
  ```
  **Step 2 — load and reconcile:** fix stale docs in the same turn.
  **Step 3 — regression battery:**
  ```bash
  uv run pytest tests/test_doc_impact.py -q -m quick   # fast host-only
  uv run pytest tests/test_doc_impact.py -q            # full probe
  ```
- Expectations live in `scripts/docs/doc-impact/live-expectations.yaml`
  (ships with skipped placeholders — delete `skip:` and point at real
  infrastructure). The committed index (`doc-index.db`) self-updates via the
  post-commit hook; enable once per clone:
  `bash scripts/docs/doc-impact/setup-git-hooks.sh`
