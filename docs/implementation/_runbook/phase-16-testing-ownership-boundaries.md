---
phase: 03-step-by-step-implementation/phase-16-testing-ownership-boundaries
---

**Phase 16 complete — ownership-boundary tests.** This phase adds a pure,
DOM-free test project that proves the MVU ownership rules stay intact as the
app grows, exactly the boundary the phase names as the test target.

### Reference

```text
- pure update functions
- routing round-trips
- message ownership (no cross-boundary state reach)
- normalization of entity caches
```

Tooling: *expecto / xUnit — pure tests need no DOM.* Verification: `dotnet test`.

### What the tests lock in

The four test targets map to three test modules over the app's pure layers:

| Target (reference) | Test file | What it proves |
|---|---|---|
| Pure update functions | `SharedUpdateTests.fs` | `Shared.update` normalizes arrays into id-keyed maps, flips toggles on one canonical source, tracks session state, and isolates the `Loading` cache transition |
| Message ownership | `AppTests.fs` | the root `update` is an *orchestration* boundary: cross-feature effects are **translated** (a shared update is dispatched, never mutated), page changes leave shared caches untouched, and Account `Submit` keeps the route |
| Routing round-trips | `RoutingTests.fs` | `router.setRoute`/`getRoute` round-trip; unknown paths fall back predictably |

Normalization of entity caches is asserted directly in `SharedUpdateTests` (the
`GotGames` case builds an id-keyed `Map`).

### The test project layout

```
tests/Community.Client.Tests/
  Community.Client.Tests.fsproj     # net10.0, xUnit, refs the Client project
  TestData.fs                       # stub Api (throws if called) + sample fixtures
  SharedUpdateTests.fs              # 10 tests on Shared.update
  AppTests.fs                       # 5 tests on root update orchestration
  RoutingTests.fs                   # 6 tests for routing round-trips
```

The key design decision: the stub `Api` **never resolves** — every field throws
if invoked. Request messages (`Get*`, `Send*`) only emit cmds (ignored in these
pure tests); response-bearing messages (`Got*`, `Toggle*`, `Recv*`) are fed
straight to the reducer. This keeps the tests deterministic, DOM-free, and free
of a remote or a running server.

### The Bolero router API (a real gotcha, verified in Bolero source)

Routing tests initially passed leading-slash paths (`"/games"`) and failed.
Reading `thirdparty/Bolero/src/Bolero/Router.fs` and `Components.fs` shows the
router works in **base-relative** paths: `getRoute` returns `"games"` (no
leading slash — `ForceSetState` even does `.TrimStart('/')`), and `setRoute`
expects the same. The tests therefore assert round-trips against
`"games"`/`"account"`/`""` (root), matching Bolero's own routing fixtures
(`thirdparty/Bolero/tests/Unit.Client/Routing.fs`).

### Files changed

```
Community.Web.sln                            (add tests/...fsproj to solution)
tests/Community.Client.Tests/                (new test project, 21 tests)
docs/implementation/progress.yaml            (phase-16: done)
docs/implementation/index.md                (regenerated)
docs/implementation/_runbook/phase-16-testing-ownership-boundaries.md  (this file)
```

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/Community.Client.Tests.fsproj
```

All **21** ownership-boundary tests pass, proving the root orchestration
translates (never mutates) cross-feature effects and every route round-trips.

`verify.sh` reports `VERIFY OK`.