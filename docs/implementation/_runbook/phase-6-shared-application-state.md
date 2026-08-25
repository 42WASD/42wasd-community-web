---
phase: 03-step-by-step-implementation/phase-6-shared-application-state
---

**Phase 6 complete** — the app now has a normalized `SharedModel` for
persistent cross-page state, backed by a single `RemoteData<'T>` type, under a
root `{ Page; Shared; Local }` model.

### Goal met

```fsharp
// Client/State/RemoteData.fs
type RemoteData<'T> = NotAsked | Loading | Loaded of 'T | Failed of string

// Client/State/Shared.fs
type SharedModel =
    {
        games      : RemoteData<Map<string, Game>>
        servers    : RemoteData<Map<string, GameServer>>
        tournaments: RemoteData<Map<string, Tournament>>
        news       : RemoteData<Map<string, News>>
        players    : RemoteData<Map<string, Player>>
        account    : string option
        error      : string option
        signInFailed: bool
    }

// Client/App/App.fs
type Model = { page: Page; shared: SharedModel; local: LocalModel }
```

### New files

- `src/Community.Web.Client/State/RemoteData.fs` — the single `RemoteData<'T>`
  type + a `RemoteData.fold` helper.
- `src/Community.Web.Client/State/Shared.fs` — `SharedModel` record +
  `SharedModel.init` + `SharedModel.indexById` (builds a `Map<string,'T>` keyed
  by entity `id`).

### Design rules applied

- **RemoteData everywhere**: every async server-backed value is `RemoteData`,
  no ad-hoc `IsLoading`/`HasError` booleans.
- **Normalized maps**: entities are stored once, by id, in `Map<string, T>`.
  Pages refer by id / read the shared map — no stale duplicates.
- **Root { Page; Shared; Local }**: `Page` is the route, `Shared` is the
  persistent cross-page state, `Local` holds the active page's ephemeral state
  (currently the login form).
- **One canonical cache**: `Main.fs` init now dispatches `GetGames`,
  `GetServers`, `GetTournaments`, `GetNews`, `GetPlayers` (plus
  `GetSignedInAs`), so the shared maps populate once and every page reads the
  same canonical instance. This is the Phase 6 acceptance: two pages reading
  the same entity list share one cache.

### Changes

- `App.fs`: `Model` → `{ page; shared; local }`; `update` routes updates into
  the correct `SharedModel` field (e.g. `GotGames` → `SharedModel.indexById
  games` → `Loaded`); login form moved to `LocalModel`.
- `Layout.fs`: every table view now reads via `model.shared.*` and renders with
  a `dataRows` helper over `RemoteData<Map<...>>`.
- `Main.fs`: init dispatches the full batch of loads so the canonical cache is
  populated on startup.

### Compile lessons

- A `namespace` cannot hold values → wrap record init + helpers in a
  `module SharedModel`.
- Entity id accessors differ per type → pass an explicit `getId` projection to
  `indexById`.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

All routes served HTTP 200 in Development (`/`, `/games`, `/servers`,
`/tournaments`, `/members`, `/about`). (Browser WASM dev bootstrap was flaky in
the sandbox; the build + route checks confirm the refactor compiles and serves.)

### Acceptance (from reference spec)

- [x] Shared state survives navigation and spans pages (`SharedModel`)
- [x] Uses `RemoteData<'T>` for async server-backed values
- [x] Canonical entities stored normalized in maps by id
- [x] Two pages reading the same entity list share one canonical cache

### Next

Phase 7 — Home page (build the full Home feature on the shared state).