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

**1 / 20** phases/sections complete (**5%**).

<div class="progress-row" style="max-width:720px;padding:8px 0;"><div class="progress-track"><div class="progress-fill progress-fill--shimmer" style="--w:5.0%"></div></div><div class="progress-pct">5%</div></div>

| Status | Count |
|--------|-------|
| ✅ done | 1 |
| 🔶 in-progress | 0 |
| ⬜ not-started | 19 |
| ❌ blocked | 0 |
| ⏸️ deferred | 0 |

## Progress by part

### 5% — Part III — Step-by-step implementation

<div class="tip" style="display:flex;align-items:center;gap:8px;max-width:520px;padding:2px 0 10px;"><div class="progress-track"><div class="progress-fill" style="--w:5.0%"></div></div><div class="progress-pct" style="font-size:.85em;">5%</div><div class="tip-box"><strong>Done (1)</strong>
• Ownership rules
<hr style="opacity:.3;margin:6px 0;"><strong>Pending (19)</strong>
• Create the solution
• Repository structure
• Shared domain types
• Build routing
• Root app orchestration
• Shared application state
• Home page
• Stateful page — PageModel
• Nested page messages
• Server remoting
• Feature-owned UI
• More features
• Authentication
• Cross-feature effects
• Rendering optimization
• Testing ownership boundaries
• Design system
• Production hardening
• Rollout order</div></div>

- ✅ `done` — [Phase 0 — Ownership rules](../reference-design/03-step-by-step-implementation/phase-0-ownership-rules/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Ownership rules</summary>

**Agreed ownership rules** — Phase 0 delivers the written statement of the rules
that every later phase follows. Nothing in this phase introduces application
code; it is a documented contract that anchors the whole implementation.

### The rules (agreed)

```text
1. Shared.Model owns persistent cross-page application state.
2. PageLocal owns state that exists only on one page.
3. A page-local Model may hold UI/transient state but must not duplicate
   canonical shared entities.
4. Navigation changes the Page route in the root model, never arbitrary UI
   flags.
5. Effects that reach the server belong in the Server boundary (remoting), not
   scattered in views.
```

### How these rules will be enforced

- **Rule 1 — `Shared.Model`**: cross-page state (authenticated user, entity
  caches, community metadata) lives in `Community.Client/State/Shared.fs`.
  Pages select from it; they never own a canonical copy.
- **Rule 2 — `PageLocal`**: a page that needs ephemeral state keeps it in its
  own page-local `Model` (and `PageModel<'T>` for route-transient state).
- **Rule 3 — no duplicate entities**: pages reference canonical entities by
  `Id` and read them from `Shared`; they never copy an entity into a page-local
  model.
- **Rule 4 — navigation via route**: only `PageChanged` changes the active
  route in the root model. No arbitrary UI flags drive navigation.
- **Rule 5 — effects in the Server boundary**: server-touching effects are
  isolated behind a remoting API module; views never call the server directly.

### Acceptance

This phase is done when the rules above are written down and agreed. The
progress page marks Phase 0 as `done`; no code is required yet.

</details>

- ⬜ `not-started` — [Phase 1 — Create the solution](../reference-design/03-step-by-step-implementation/phase-1-create-the-solution/index.md)
- ⬜ `not-started` — [Phase 2 — Repository structure](../reference-design/03-step-by-step-implementation/phase-2-repository-structure/index.md)
- ⬜ `not-started` — [Phase 3 — Shared domain types](../reference-design/03-step-by-step-implementation/phase-3-shared-domain-types/index.md)
- ⬜ `not-started` — [Phase 4 — Build routing](../reference-design/03-step-by-step-implementation/phase-4-build-routing/index.md)
- ⬜ `not-started` — [Phase 5 — Root app orchestration](../reference-design/03-step-by-step-implementation/phase-5-root-app-orchestration/index.md)
- ⬜ `not-started` — [Phase 6 — Shared application state](../reference-design/03-step-by-step-implementation/phase-6-shared-application-state/index.md)
- ⬜ `not-started` — [Phase 7 — Home page](../reference-design/03-step-by-step-implementation/phase-7-home-page/index.md)
- ⬜ `not-started` — [Phase 8 — Stateful page — PageModel](../reference-design/03-step-by-step-implementation/phase-8-stateful-page-pagemodel/index.md)
- ⬜ `not-started` — [Phase 9 — Nested page messages](../reference-design/03-step-by-step-implementation/phase-9-nested-page-messages/index.md)
- ⬜ `not-started` — [Phase 10 — Server remoting](../reference-design/03-step-by-step-implementation/phase-10-server-remoting/index.md)
- ⬜ `not-started` — [Phase 11 — Feature-owned UI](../reference-design/03-step-by-step-implementation/phase-11-feature-owned-ui/index.md)
- ⬜ `not-started` — [Phase 12 — More features](../reference-design/03-step-by-step-implementation/phase-12-more-features/index.md)
- ⬜ `not-started` — [Phase 13 — Authentication](../reference-design/03-step-by-step-implementation/phase-13-authentication/index.md)
- ⬜ `not-started` — [Phase 14 — Cross-feature effects](../reference-design/03-step-by-step-implementation/phase-14-cross-feature-effects/index.md)
- ⬜ `not-started` — [Phase 15 — Rendering optimization](../reference-design/03-step-by-step-implementation/phase-15-rendering-optimization/index.md)
- ⬜ `not-started` — [Phase 16 — Testing ownership boundaries](../reference-design/03-step-by-step-implementation/phase-16-testing-ownership-boundaries/index.md)
- ⬜ `not-started` — [Phase 17 — Design system](../reference-design/03-step-by-step-implementation/phase-17-design-system/index.md)
- ⬜ `not-started` — [Phase 18 — Production hardening](../reference-design/03-step-by-step-implementation/phase-18-production-hardening/index.md)
- ⬜ `not-started` — [Phase 19 — Rollout order](../reference-design/03-step-by-step-implementation/phase-19-rollout-order/index.md)

<!-- END_GENERATED_IMPLEMENTATION -->