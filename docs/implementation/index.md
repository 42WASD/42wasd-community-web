# Implementation Status

This page tracks implementation progress against the
[Reference Design](../reference-design/index.md). Each phase/section is
assigned a status; a generator renders this page from
`docs/implementation/progress.yaml` and the reading-order SSOT manifest.

## Legend

| Status | Meaning |
|--------|---------|
| ✅ done | Implemented, verified, and reflected in the repo |
| 🔶 in-progress | Actively being implemented |
| ⬜ not-started | Not yet touched |
| ❌ blocked | Blocked on an external dependency |
| ⏸️ deferred | Intentionally postponed to a later stage |

## How it works

- Source of truth for status: `docs/implementation/progress.yaml`
- Source of truth for order: `docs/reference-design/_sequence.yaml`
- Generator: `scripts/docs/docs-generate-implementation.py`
- Regenerate: `bash scripts/docs/verify.sh`
- Only **tracked** parts (in the manifest) appear below.

<!-- BEGIN_GENERATED_IMPLEMENTATION -->

## Overall progress

**1 / 3** phases/sections complete (**33%**).

<div class="progress-row" style="max-width:720px;padding:8px 0;"><div class="progress-track"><div class="progress-fill progress-fill--shimmer" style="--w:33.3%"></div></div><div class="progress-pct">33%</div></div>

| Status | Count |
|--------|-------|
| ✅ done | 1 |
| 🔶 in-progress | 0 |
| ⬜ not-started | 2 |
| ❌ blocked | 0 |
| ⏸️ deferred | 0 |

## Progress by part

### 33% — Part II — Step-by-step implementation

<div class="tip" style="display:flex;align-items:center;gap:8px;max-width:520px;padding:2px 0 10px;"><div class="progress-track"><div class="progress-fill" style="--w:33.0%"></div></div><div class="progress-pct" style="font-size:.85em;">33%</div><div class="tip-box"><strong>Done (1)</strong>
• Create the project repository
<hr style="opacity:.3;margin:6px 0;"><strong>Pending (2)</strong>
• Set up the toolchain
• First deliverable</div></div>

- ✅ `done` — [Phase 0 — Create the project repository](../reference-design/02-step-by-step-implementation/create-the-project-repository/index.md)
- ⬜ `not-started` — [Phase 1 — Set up the toolchain](../reference-design/02-step-by-step-implementation/set-up-the-toolchain/index.md)
- ⬜ `not-started` — [Phase 2 — First deliverable](../reference-design/02-step-by-step-implementation/first-deliverable/index.md)

<!-- END_GENERATED_IMPLEMENTATION -->