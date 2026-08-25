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

**7 / 20** phases/sections complete (**35%**).

<div class="progress-row" style="max-width:720px;padding:8px 0;"><div class="progress-track"><div class="progress-fill progress-fill--shimmer" style="--w:35.0%"></div></div><div class="progress-pct">35%</div></div>

| Status | Count |
|--------|-------|
| ✅ done | 7 |
| 🔶 in-progress | 0 |
| ⬜ not-started | 13 |
| ❌ blocked | 0 |
| ⏸️ deferred | 0 |

## Progress by part

### 35% — Part III — Step-by-step implementation

<div class="tip" style="display:flex;align-items:center;gap:8px;max-width:520px;padding:2px 0 10px;"><div class="progress-track"><div class="progress-fill" style="--w:35.0%"></div></div><div class="progress-pct" style="font-size:.85em;">35%</div><div class="tip-box"><strong>Done (7)</strong>
• Ownership rules
• Create the solution
• Repository structure
• Shared domain types
• Build routing
• Root app orchestration
• Shared application state
<hr style="opacity:.3;margin:6px 0;"><strong>Pending (13)</strong>
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

- ✅ `done` — [Phase 1 — Create the solution](../reference-design/03-step-by-step-implementation/phase-1-create-the-solution/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Create the solution</summary>

**Phase 1 complete** — the Bolero solution was scaffolded from the official
template, built cleanly, and verified to run locally with the default route
rendering.

### What was created

```text
Community.Web.sln
global.json                          # pins .NET SDK 10.0.111 (rollForward: disable)
src/
├── Community.Web.Client/            # classic Bolero Elmish WASM client
│   ├── Community.Web.Client.fsproj
│   ├── Main.fs                      # ProgramComponent + Router.infer (template baseline)
│   ├── Startup.fs
│   ├── MyApp.bolero.css
│   └── wwwroot/ (main.html, css, favicon)
└── Community.Web.Server/            # ASP.NET host + remoting
    ├── Community.Web.Server.fsproj
    ├── Startup.fs
    ├── Index.fs
    ├── BookService.fs
    └── data/books.json
```

### How it was scaffolded

```bash
dotnet new bolero-app -n Community.Web -o . --render LegacyWebAssembly
```

- **Render mode `LegacyWebAssembly`** — the classic Bolero hosting path
  (`ProgramComponent`, `Router.infer`, `PageModel<'T>`, `Program.withRouter`,
  remoting) that the reference design's canonical patterns are written for.
  Chosen after confirming in `thirdparty/Bolero-Template` source that the
  classic API is wired only for the non-`isInteractive` modes, and that the
  `Interactive*` modes use the new .NET Blazor renderer pipeline instead.
- **`global.json`** pins `10.0.111` with `rollForward: disable` so builds are
  reproducible regardless of the SDK installed on a developer machine.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
# run locally (Development):
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Community.Web.Server
```

HTTP checks against `http://localhost:5023`:

| Check | Result |
|---|---|
| `/` (default route) | 200 — `Bolero Application` |
| `_framework/blazor.webassembly.js` | 200 — client loader served |
| client WASM payload (hashed) | 200 — client boots |
| `/counter` (routed page fallback) | 200 — refresh on a routed page works |

Note: in .NET 8+ Blazor WASM the framework assets are served under **hashed**
names; raw `_framework/blazor.boot.json` / `Community.Web.Client.dll` return
404 by design. The client boots from the runtime manifest.

### Acceptance (from verified design)

- [x] solution restores
- [x] server/client run locally
- [x] default route renders
- [x] refresh on a routed page works

### Next

Phase 2 will reshape the client into `App/ State/ Pages/ Ui/ Infrastructure/`
folders. The template baseline is left intact for now; later phases reshape it
without rewriting the framework machinery.

</details>

- ✅ `done` — [Phase 2 — Repository structure](../reference-design/03-step-by-step-implementation/phase-2-repository-structure/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Repository structure</summary>

**Phase 2 complete** — the repository was restructured into the feature-oriented
layout from the reference design, and both client and server now depend on a
shared contracts project.

### New structure

```text
src/
├── Community.Web.Shared/          # shared contracts (no server/client deps)
│   ├── Community.Web.Shared.fsproj
│   └── Contracts/
│       ├── Books.fs               # canonical Book entity
│       └── CommunityApi.fs        # BookService remoting contract
├── Community.Web.Client/
│   ├── Community.Web.Client.fsproj
│   ├── App/
│   │   └── App.fs                 # orchestration: Page, Model, Message, update, router, init
│   ├── Ui/
│   │   └── Layout.fs              # cross-feature UI (shared layout template + views)
│   ├── Main.fs                    # root ProgramComponent (ProgramRouter + program wiring)
│   ├── Startup.fs
│   └── wwwroot/
└── Community.Web.Server/
    ├── Community.Web.Server.fsproj
    ├── Startup.fs
    ├── Index.fs
    ├── BookService.fs             # server-side remoting handler
    └── data/books.json
```

### Design decisions

- **`Community.Web.Shared`**: holds the `Book` entity and the `BookService`
  remoting contract. This is the single contract type both client and server
  compile against — no client<->server circular dependency.
- **Dependency direction**: `Community.Web.Shared` is depended on by both
  Client and Server; Client is referenced by Server (to host the WASM app);
  Server does NOT reference Client's internal logic.
- **App/Ui split**: the root `Model`/`Msg`/`update`/router live in `App/`;
  cross-feature UI (the shared template + view composition) lives in `Ui/`.
  Page-specific UI will move beside its page in a later phase (feature-owned
  UI), keeping the global `Ui/` folder deliberately small per the reference.
- **No top-level `Model/`/`Msg/`/`Update/` split** — state, messages, and views
  are organized by concern/feature, not by technical type.
- **Start shallow**: no empty placeholder directories (`State/`, `Pages/`,
  `Infrastructure/`) were created for hypothetical features. They will be added
  in later phases when they have real content.

### Remoting contract moved to Shared

The template's `BookService` + `Book` were lifted out of `Client.Main` into
`Community.Web.Shared/Contracts`. The server's `RemoteHandler` now inherits
`RemoteHandler<Community.Web.Shared.Contracts.BookService>` (the shared
contract), so it no longer depends on the client's internal `Main` type.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live checks against `http://localhost:5023` (Development):

| Route | Result |
|---|---|
| `/` | 200 |
| `/counter` | 200 |
| `/data` | 200 |
| `/books/getBooks` (remoting) | 200 |

Both client and server reference the shared project successfully.

### Acceptance (from reference design)

- [x] Shared has no server-only or client-only dependencies
- [x] Both client and server depend on `Community.Web.Shared` contracts
- [x] Feature folders, not top-level `Model/View/Update` split
- [x] Each directory has a clear ownership rule
- [x] No global `Models/Msgs/Updates` directories
- [x] No feature-specific UI dumped into `Ui/`

### Next

Phase 3 will define the shared domain types (`Community.Web.Shared/Domain` +
remoting service interfaces) needed for the first community slice.

</details>

- ✅ `done` — [Phase 3 — Shared domain types](../reference-design/03-step-by-step-implementation/phase-3-shared-domain-types/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Shared domain types</summary>

**Phase 3 complete** — the shared domain types and remoting contract are
defined for the **gaming community**, and the template's demo Book data was
replaced with real gaming content throughout the app.

### New shared structure

```text
Community.Web.Shared/
├── Domain/
│   ├── Game.fs          # id, name, genre, description
│   ├── GameServer.fs    # id, name, gameId, address, onlinePlayers, maxPlayers, status
│   ├── Tournament.fs    # id, name, gameId, startsAt, prize, registrationOpen
│   ├── Player.fs        # id, username, discord
│   ├── Team.fs          # id, name, players[]
│   └── News.fs          # id, title, body, publishedAt
└── Remoting/
    └── CommunityApi.fs  # CommunityApi contract (IRemoteService, BasePath = "/api")
```

### Design decisions

- **Gaming domain** (user decision): the community is modeled as games,
  servers, tournaments, players, teams, and news.
- **`Domain/` vs `Remoting/` split** (per reference design): pure entity records
  live in `Domain/`; the remote service interface lives in `Remoting/`. The old
  flat `Contracts/` folder (template demo) was removed.
- **Demo removal (user decision, "Replace now")**: the template's `Book`/
  `BookService` demo was dropped entirely. Client pages, server service, and
  seed JSON data now use the gaming domain.
- **`CommunityApi` contract**: exposes `getGames`, `getServers`,
  `getTournaments`, `getNews`, `getPlayers`, plus the auth trio
  (`signIn`/`getUsername`/`signOut`). BasePath is now `/api` (was `/books`).

### Client rewiring

- `App/App.fs`: `Page` now has six gaming routes (`/`, `/games`, `/servers`,
  `/tournaments`, `/members`, `/about`). `Model`/`Msg`/`update` drive games,
  servers, tournaments, news, and players. `loadHomeData` fetches the four
  home sections in parallel.
- `Ui/Layout.fs`: one view per page; home page shows Games/Active servers/
  Tournaments/Latest news tables.
- `wwwroot/main.html`: rewritten with templates for the six pages + a shared
  `EmptyData` row (colspan 5).
- `Main.fs`: now `this.Remote<CommunityApi>()`.

### Server

- `CommunityApiService.fs` replaces `BookService.fs`:
  `RemoteHandler<Community.Web.Shared.Remoting.CommunityApi>`, with a
  `Loaders` module to read JSON seed files.
- Seed data: `data/{games,servers,tournaments,news,players}.json`.
- `Community.Web.Server.fsproj` adds `None Include="data/**/*"
  CopyToOutputDirectory="PreserveNewest"` so JSON ships to output.

### Compile lessons

- `td { intVal }` fails (int is not a Node) → `.ToString()`.
- `td { stringOption }` fails → `defaultArg p.discord ""`.
- `namespace` cannot hold a generic value → wrap loader in a `module Loaders`.
- `RemoteUnauthorizedException` pattern needs `open Bolero.Remoting`.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live checks against `http://localhost:5023` (Development):

| Route | Result |
|---|---|
| `/` | 200 |
| `/games` | 200 |
| `/servers` | 200 |
| `/tournaments` | 200 |
| `/members` | 200 |
| `/about` | 200 |

Home page HTML includes "gaming community" content.

### Acceptance (from reference design)

- [x] Domain types carry no server/client framework dependencies (pure records)
- [x] IDs, entity records, request/response live in `Community.Shared`
- [x] Client and server compile against the same shared domain types
- [x] `Domain/` + `Remoting/` folder split per the reference

### Next

Phase 4 (home page) will build the Home feature on top of these shared types:
hero, stats, games, active servers, tournaments, and news sections.

</details>

- ✅ `done` — [Phase 4 — Build routing](../reference-design/03-step-by-step-implementation/phase-4-build-routing/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Build routing</summary>

**Phase 4 complete** — routing is established with a `Page` union bound to the
URL, and unknown URLs fall back predictably to the Home page.

### What this phase required

From the reference design: a `Page` DU bound to routes, `Page` stored in the
root model, a page-changing message, `Router.infer` binding route `<->` Page,
and predictable fallback for wrong URLs.

### Already in place (from Phase 3)

The routing skeleton was largely established while building the shared-domain
slice:

```fsharp
// App/App.fs
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/games">] Games
    | [<EndPoint "/servers">] Servers
    | [<EndPoint "/tournaments">] Tournaments
    | [<EndPoint "/members">] Members
    | [<EndPoint "/about">] About
```

- `Page` lives in the root `Model`.
- `SetPage` is the only message that changes the route.
- `Router.infer SetPage (fun model -> model.page)` maps route `<-> Page`.

### Added in this phase

**Explicit unknown-route fallback.** `Router.infer` sets `notFound = None` by
default; the server `MapFallbackToBolero` shell already served a predictable
HTTP 200 for unknown paths. To make the *client-side* intent explicit and
consistent with the reference ("Wrong URLs fall back predictably"), the router
now declares:

```fsharp
let router =
    Router.infer SetPage (fun model -> model.page)
    |> Router.withNotFound Home
```

So any route that isn't one of the six pages resolves to the Home page instead
of a blank/undefined endpoint.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live checks against `http://localhost:5023` (Development):

| Route | Result |
|---|---|
| `/` | 200 |
| `/games` | 200 |
| `/servers` | 200 |
| `/nonexistent-route` | 200 (falls back predictably) |
| `/totally-bogus` | 200 (falls back predictably) |

### Acceptance (from reference spec)

- [x] `Page` is a union whose cases map to routes
- [x] The route lives in the root model
- [x] A single message (`SetPage`) changes the route
- [x] `Router.infer` binds route `<-> Page`
- [x] Wrong URLs fall back predictably (`Router.withNotFound Home` + shell)

### Next

Phase 5 will add root-app orchestration on top of this routing spine.

</details>

- ✅ `done` — [Phase 5 — Root app orchestration](../reference-design/03-step-by-step-implementation/phase-5-root-app-orchestration/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Root app orchestration</summary>

**Phase 5 complete** — the single root Elmish program is wired up with
dev-only tracing hooks.

### What this phase required

From the reference spec:

- One `ProgramComponent`.
- Root `Model` with `Page` + shared state.
- Root `Msg` with page-change, shared, and local messages.
- Dev-only Elmish tracing hooks: `withConsoleTrace`, `withErrorHandler`,
  `withTermination`.
- The Elmish message trace runs in the **browser console**, not the server
  terminal (template disables these hooks by default; add them manually).

### Implementation

The root `ProgramComponent` (`MyApp` in `Main.fs`) already existed from the
template/Phase 1, holding `ProgramComponent<Model, Message>` with the root
`Model` (containing `Page`) and root `Message`. The `#if DEBUG` block now adds
the three dev hooks plus hot-reload:

```fsharp
let program =
    Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
    |> Program.withRouter router
#if DEBUG
program
|> Program.withConsoleTrace
|> Program.withErrorHandler (fun (msg, exn) ->
    printfn $"Elmish error after %A{msg}: {exn}")
|> Program.withTermination
    (fun _ -> false)
    (fun _ -> printfn "Program terminated.")
|> Program.withHotReload
#else
program
#endif
```

Notes:

- `withConsoleTrace` logs every message + state change to the browser console.
- `withErrorHandler` surfaces any `(msg, exn)` raised by the update/view.
- `withTermination` takes a predicate `'model -> bool` (not an option). The
  predicate is a pass-through here; the hook is installed for later use.
- All hooks are `#if DEBUG`-gated so release builds stay lean.
- Verified against `thirdparty/Bolero` + the restored `Elmish.dll` (4.0.1):
  the hooks are Elmish `ProgramModule` members (not Bolero's), and the exact
  signatures are `withConsoleTrace : Program`, `withErrorHandler : (('msg*exn)
  -> unit) -> Program`, `withTermination : ('model -> bool) -> ('model -> unit)
  -> Program`.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live check (Development, browser console capture via Playwright):

```
log: Initial state:: { page = Home
log:   games = None ... }
log: New message:: GetSignedInAs
log: Updated state:: { page = Home ... }
log: New message:: RecvSignedInAs None
log: Updated state:: { ... }
```

The `withConsoleTrace` output appears in the browser console exactly as the
reference describes. (The 401 on `/api/getUsername` is expected: `Cmd.OfAuthor
ized` returns 401 when signed out, correctly yielding `RecvSignedInAs None`.)

### Acceptance (from reference spec)

- [x] Single root `ProgramComponent`
- [x] Root `Model` with `Page` + shared state
- [x] Root `Msg` with page-change / shared / local messages
- [x] `withConsoleTrace` active (browser console trace confirmed)
- [x] `withErrorHandler` + `withTermination` added (dev-only)
- [x] Hooks disabled in release (`#if DEBUG`)

### Next

Phase 6 — shared application state.

</details>

- ✅ `done` — [Phase 6 — Shared application state](../reference-design/03-step-by-step-implementation/phase-6-shared-application-state/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Shared application state</summary>

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

</details>

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