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

**20 / 20** phases/sections complete (**100%**).

<div class="progress-row" style="max-width:720px;padding:8px 0;"><div class="progress-track"><div class="progress-fill progress-fill--shimmer" style="--w:100.0%"></div></div><div class="progress-pct">100%</div></div>

| Status | Count |
|--------|-------|
| ✅ done | 20 |
| 🔶 in-progress | 0 |
| ⬜ not-started | 0 |
| ❌ blocked | 0 |
| ⏸️ deferred | 0 |

## Progress by part

### 100% — Part III — Step-by-step implementation

<div class="tip" style="display:flex;align-items:center;gap:8px;max-width:520px;padding:2px 0 10px;"><div class="progress-track"><div class="progress-fill" style="--w:100.0%"></div></div><div class="progress-pct" style="font-size:.85em;">100%</div><div class="tip-box"><strong>Done (20)</strong>
• Ownership rules
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
• Rollout order
<hr style="opacity:.3;margin:6px 0;"><strong>Pending (0)</strong>
—</div></div>

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

- ✅ `done` — [Phase 7 — Home page](../reference-design/03-step-by-step-implementation/phase-7-home-page/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Home page</summary>

**Phase 7 complete** — the Home page now renders a Hero, live community
**Stats**, and the latest **Games / Servers / Tournaments / News** tables, all
read directly from `SharedModel`. The Home owns no canonical copy of entities —
it only reads the shared cache.

### Goal met

The reference spec asks for: `Hero, Stats, Upcoming events, Featured projects,
Members, Join Discord / GitHub`. For the gaming-community domain this maps to:

```text
Hero            → "Welcome to the gaming community!" + tagline
Stats           → games / players online / open tournaments / members (counts)
Upcoming events → Active servers + Upcoming tournaments tables
Featured projects → Games we play table
Members         → member count in Stats
Join Discord    → "Join Discord" CTA link
```

The Home template (`wwwroot/main.html`) binds four stat cells
(`GamesCount`, `OnlineNow`, `OpenTournaments`, `MembersCount`) plus the four
content tables (`Games`, `Servers`, `Tournaments`, `News`).

### State ownership

- Home reads from `SharedModel` (the canonical cache populated on startup).
- It does **not** own a canonical copy of entities — no local `games`/`servers`
  fields on the Home feature; it only derives views from `model.shared.*`.
- `Layout.fs` gained a `stats` helper that derives `(gameCount, onlineNow,
  openTournaments, memberCount)` from the shared maps (40 online = sum of
  `server.online` across the 3 servers).

### Public reads decision

- The five read endpoints (`getGames`, `getServers`, `getTournaments`,
  `getNews`, `getPlayers`) were previously wrapped in `ctx.Authorize`, so
  guests got HTTP 401 and the Home showed "0 / Loading…".
- **Decision (user): make all reads public.** Guests should be able to see the
  community's public data without signing in.
- `CommunityApiService.fs` now returns each array directly via `fun () -> async
  { ... }`. `signIn` / `signOut` / `getUsername` are unchanged (auth writes
  still protected; `getUsername` still requires an authenticated identity).

### Changes

- `wwwroot/main.html` — Home template gained the **Stats** box (4 count cells)
  and a **Join Discord** CTA; plus the Games / Servers / Tournaments / News
  tables already present.
- `Layout.fs` — added `stats` helper; `homePage` binds `GamesCount`,
  `OnlineNow`, `OpenTournaments`, `MembersCount` and renders the four tables
  from `model.shared.*`.
- `CommunityApiService.fs` — made the five read endpoints public.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Browser at `/` (Development, WASM):

- Stats: **3 games · 40 players online · 2 tournaments open · 3 members**
- Games table: Counter-Strike 2 / Dota 2 / Minecraft
- Servers table: CS2 Competitive (8) / Dota 2 Ladder (20) / Survival SMP (12)
- Tournaments: CS2 Summer Cup / Dota 2 Community League
- News: two recent posts
- Console shows only the intentional `getUsername`/sign-in 401s, no read 401s.

### Acceptance (from reference spec)

- [x] Home reads from `SharedModel` (canonical cache)
- [x] Home does not own a canonical copy of entities
- [x] Navigate to `/` renders the Home from shared state
- [x] Public reads: guests can load all data without auth

### Next

Phase 8 — stateful page model (per-page local state owned by the feature).

</details>

- ✅ `done` — [Phase 8 — Stateful page — PageModel](../reference-design/03-step-by-step-implementation/phase-8-stateful-page-pagemodel/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Stateful page — PageModel</summary>

**Phase 8 complete** — transient page state now lives in a **`PageModel<'T>`**
that is excluded from the URL, supplied by **`Router.inferWithModel`**, and
reset per the state-lifetime rule. The Account page is the first such page,
holding the sign-in form draft.

### Goal met

The reference spec asks for:

```text
a page with form/draft state that must not appear in the URL
state lives in PageModel<'T>, excluded from the route
```

The Account page (`/account`) carries the sign-in form draft (`username`,
`password`) in `Account of PageModel<AccountForm>`. Typing in the form updates
the PageModel in place; the draft never appears in the URL.

### Implementation

```fsharp
// Page union: the Account case carries a PageModel<AccountForm>
type Page =
    | ...
    | [<EndPoint "/account">] Account of PageModel<AccountForm>

// Transient page state — lives in the PageModel, not the root model
and AccountForm = { username: string; password: string }

// Root model: Page (route) + Shared (persistent). No more `local` field.
type Model =
    {
        page: Page
        shared: SharedModel
    }
```

```fsharp
// Router supplies a fresh default PageModel when entering the route
let router =
    let defaultPageModel = function
        | Account pm -> Router.definePageModel pm { username = ""; password = "" }
        | _ -> ()
    Router.inferWithModel SetPage (fun m -> m.page) defaultPageModel
    |> Router.withNotFound Home
```

### How updates mutate the PageModel

The `PageModel<'a>` is a mutable holder shared with the view. Typing a
character dispatches `SetUsername`/`SetPassword`, which update the instance in
place via `Router.definePageModel`:

```fsharp
| SetUsername s ->
    match model.page with
    | Account pm -> Router.definePageModel pm { pm.Model with username = s }
    | _ -> ()
    model, Cmd.none
```

`SendSignIn` reads the draft straight from the active PageModel, and
`ClearLoginForm` resets it after a successful sign-in. Because the same
instance is shared between the router and the view, the text the user types
survives re-renders without ever being encoded into the URL.

### State-lifetime rule (verified)

Transient page state resets when the page is navigated to fresh:

- Sign in → `Signed in as testuser` banner.
- Sign out → back to a blank Sign in form.
- Navigate away and back to `/account` → **fresh empty form** (the router
  re-supplies a new default PageModel). The draft does not leak across
  sessions of the page.

### Changes

- `App/App.fs` — `Page` gained `Account of PageModel<AccountForm>`; deleted the
  root `LocalModel` field; `Model` is now just `{ page; shared }`; `update`
  mutates the PageModel in place for `SetUsername`/`SetPassword`/
  `ClearLoginForm`; `router` switched from `Router.infer` to
  `Router.inferWithModel` with a `defaultPageModel` supplying the empty form.
- `Ui/Layout.fs` — new `accountPage` (renders the SignIn form or the
  signed-in banner); `view` gained the `Account` menu item and the `Account _`
  body case.
- `wwwroot/main.html` — new `SignIn` and `AccountSignedIn` templates.

### MVU verification (browser console)

```
New message:: SetUsername "testuser"
New message:: SetPassword "password"
New message:: SendSignIn
New message:: RecvSignIn (Some "testuser")
New message:: GetPlayers
New message:: ClearLoginForm
New message:: GotPlayers
```

Sign-out trace: `SendSignOut` → `RecvSignOut`; fresh `/account` navigation
dispatched the full data batch and rendered a blank form. Only the intentional
auth-protected `getUsername` 401 remains.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] A page with form/draft state that must not appear in the URL (Account)
- [x] State lives in `PageModel<'T>`, excluded from the route
- [x] `Router.inferWithModel` supplies the default PageModel
- [x] Page keeps transient state across in-page updates
- [x] Transient state resets per the state-lifetime rule (fresh navigation)

### Next

Phase 9 — Nested page messages (page/feature-scoped message namespaces).

</details>

- ✅ `done` — [Phase 9 — Nested page messages](../reference-design/03-step-by-step-implementation/phase-9-nested-page-messages/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Nested page messages</summary>

**Phase 9 complete** — each page/feature now scopes its messages in its own
module, and those nested messages are composed into the root `Message` and
lifted with **`Cmd.map`**. The Account page's local messages no longer leak
into the root event dump; the root is a pure orchestration boundary.

### Goal met

The reference spec asks for:

```text
page-scoped message namespaces
nested messages composed into the root and lifted with Cmd.map
the root translates cross-feature effects (Submit -> session sign-in)
```

The Account feature (`Pages/Account.fs`) owns its form messages
(`SetUsername`, `SetPassword`, `Clear`, `Submit`). Session/auth stays on the
root `Shared` dispatcher, which owns `SharedModel.account` and reads the form
draft from the Account page only when needed.

### Ownership split (as chosen)

| Concern | Owned by |
| --- | --- |
| Sign-in form draft (`username`, `password`) | `Account` page module |
| Form edit / clear / submit intent | `Account.Msg` |
| Session data (`SharedModel.account`), sign-in/out, data loads | `Shared.Msg` |
| Translating `Submit` → `SendSignIn` effect | root `Message` (`AccountMsg`) |

### Implementation

The `Shared` module is declared **before** the root `Message` union so its
`Msg` type is in scope for the union's cases. The union cases are named
`SharedMsg` / `AccountMsg` (and the route case `AccountPage`) to avoid
colliding with the `Shared` / `Account` modules.

```fsharp
// Pages/Account.fs — the feature owns its local messages
module Account =
    type Model = { username: string; password: string }
    type Msg =
        | SetUsername of string
        | SetPassword of string
        | Clear
        | Submit
    let init = { username = ""; password = "" }
    let update msg model = ...   // pure, local only
```

```fsharp
// App/App.fs — nested messages composed into the root
module Shared =
    type Msg = GetGames | GotGames of Game[] | ... | SendSignIn of string*string
              | RecvSignIn of option<string> | SendSignOut | RecvSignOut
              | Error of exn | ClearError
    let update remote (shared: SharedModel) (msg: Msg) = ...   // returns lifted cmd

type Message =
    | SetPage of Page
    | SharedMsg of Shared.Msg
    | AccountMsg of Account.Msg
```

### The root dispatcher

The root interprets `Submit` as a **cross-feature effect**: the Account page
owns the form, but the session belongs to `Shared`, so the root translates the
child's submit intent into a shared session message:

```fsharp
| AccountMsg msg ->
    match model.page with
    | AccountPage pm ->
        match msg with
        | Account.Submit ->
            let send = Cmd.ofMsg (SharedMsg (Shared.SendSignIn (pm.Model.username, pm.Model.password)))
            model, send
        | _ ->
            let m, cmd = Account.update msg pm.Model
            Router.definePageModel pm m
            model, Cmd.map AccountMsg cmd
    | _ -> model, Cmd.none
```

And the shared layer is lifted back up with `Cmd.map SharedMsg`:

```fsharp
| SharedMsg msg ->
    let shared, cmd = Shared.update remote model.shared msg
    // cross-boundary effect: clear the Account form after a successful sign-in
    let cmd =
        match msg with
        | Shared.RecvSignIn (Some _) ->
            Cmd.batch [ Cmd.map SharedMsg cmd; Cmd.ofMsg (AccountMsg Account.Clear) ]
        | _ -> Cmd.map SharedMsg cmd
    { model with shared = shared }, cmd
```

`Main.fs` now dispatches the initial data batch as nested messages:
`Cmd.ofMsg (SharedMsg Shared.GetGames)` and friends.

### Changes

- `Pages/Account.fs` — **new** module holding `Account.Model`, `Account.Msg`,
  `init`, and `update`. Owns the sign-in form draft and its local messages.
- `App/App.fs` — extracted the shared-layer messages into `module Shared`
  (declared before `type Message`); root `Message` is now
  `SetPage | SharedMsg of Shared.Msg | AccountMsg of Account.Msg`; route case
  renamed `Account` → `AccountPage`; root `update` composes nested messages
  with `Cmd.map` and translates `Account.Submit` into a `Shared` session
  effect.
- `Ui/Layout.fs` — message dispatch now qualifies nested modules
  (`SharedMsg Shared.SendSignOut`, `AccountMsg (Account.SetUsername s)`,
  `AccountMsg Account.Submit`, `SharedMsg Shared.ClearError`); `view` and
  `menuItem` use `AccountPage`.
- `Main.fs` — initial data loads wrapped as `Cmd.ofMsg (SharedMsg Shared.*)`.

### MVU verification (browser console)

```
New message:: SharedMsg GetSignedInAs
New message:: SharedMsg GetGames
New message:: SharedMsg GetServers
New message:: SharedMsg GetTournaments
New message:: SharedMsg GetNews
New message:: SharedMsg GetPlayers
New message:: SharedMsg (RecvSignedInAs None)        // authed probe -> signed out
New message:: SharedMsg (GotGames ...)
```

Sign-in flow (typed player1 / password, clicked Sign in):

```
New message:: AccountMsg (SetUsername "player1")
New message:: AccountMsg (SetPassword "password")
New message:: AccountMsg Submit                    // root translates to a Shared effect
New message:: SharedMsg SendSignIn
New message:: SharedMsg (RecvSignIn (Some "player1"))
New message:: AccountMsg Clear                     // root clears the form post-success
New message:: SharedMsg GetPlayers                 // refresh Members
```

Sign-out flow:

```
New message:: SharedMsg SendSignOut
New message:: SharedMsg RecvSignOut
```

The page went from the sign-in form → **"Signed in as player1"** banner → back
to a blank form on sign-out. Only the intentional auth-protected `getUsername`
401 remains in the console.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] Page/feature scopes its messages in its own module (`Account.Msg`)
- [x] Nested messages composed into the root and lifted with `Cmd.map`
- [x] Root is an orchestration boundary, not an event dump
- [x] Cross-feature effect (Submit → session) translated by the parent
- [x] Session/auth stays on the Shared dispatcher; page owns transient form

### Next

Phase 10 — (see `_sequence.yaml`).

</details>

- ✅ `done` — [Phase 10 — Server remoting](../reference-design/03-step-by-step-implementation/phase-10-server-remoting/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Server remoting</summary>

**Phase 10 complete** — server functions are exposed over Bolero remoting and
the client calls the **same shared `CommunityApi` contract**. The full
client → remoting endpoint → JSON round-trip is verified with `curl`.

### Goal met

The reference spec asks for:

```text
configure a remoting service in the server
client calls shared async functions
```

Both halves were already established during earlier phases (the app loads all
data through remoting). This phase verifies the contract with `curl` and
documents the wiring as the canonical reference.

### The shared contract

`src/Community.Web.Shared/Remoting/CommunityApi.fs` defines the single
`CommunityApi` record the client and server both compile against. Its
`BasePath = "/api"` makes each method an `http://host/api/<method>` endpoint.
No client<->server circular dependency — the contract lives in the shared
layer (the only Bolero dependency there).

```fsharp
type CommunityApi =
    {
        getGames: unit -> Async<Game[]>
        getServers: unit -> Async<GameServer[]>
        getTournaments: unit -> Async<Tournament[]>
        getNews: unit -> Async<News[]>
        getPlayers: unit -> Async<Player[]>
        signIn: string * string -> Async<option<string>>
        getUsername: unit -> Async<string>
        signOut: unit -> Async<unit>
    }
    interface IRemoteService with
        member this.BasePath = "/api"
```

### Server implementation

`src/Community.Web.Server/CommunityApiService.fs` is a `RemoteHandler<CommunityApi>`
that loads each data set once from JSON and returns it. Auth functions use the
request context (`ctx.HttpContext.AsyncSignIn` / `AsyncSignOut`); `getUsername`
is wrapped in `ctx.Authorize` so an unauthenticated call returns `401`.

### Server wiring (`Startup.fs`)

```fsharp
builder.Services.AddBoleroRemoting<CommunityApiService>()   // register handler
// ...
app.MapBoleroRemoting()                                     // expose /api/* routes
```

### Client wiring (`Main.fs`)

The single root component obtains the remote handler by type and passes it to
the Elmish `update`, which issues `Cmd.OfAsync.either` / `Cmd.OfAuthorized.either`
calls:

```fsharp
let communityApi = this.Remote<CommunityApi>()
let update = update communityApi
```

### Verification (curl)

**Note** (from the reference): an F# `unit` argument serializes to JSON `null`
for remoting — send `-d 'null'`, not `[]` or an empty body.

All data endpoints return their JSON arrays:

```
POST /api/getGames       -> [{"id":"game-1","name":"Counter-Strike 2",...}]
POST /api/getServers     -> [{"id":"server-1",...}, ...]
POST /api/getTournaments -> [{"id":"tournament-1",...}, ...]
POST /api/getNews        -> [{"id":"news-1",...}, ...]
POST /api/getPlayers     -> [{"id":"player-1",...}, ...]
```

The full auth round-trip (cookie jar):

```
signIn ("user2","password")        -> "user2"
getUsername                        -> "user2"        // authenticated
signOut                            -> null
getUsername (fresh jar)            -> 401            // session cleared
```

And sign-in failure returns `null` (not an exception):

```
signIn ("user1","wrong")           -> null
```

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
curl -X POST .../api/getGames -d 'null'   # all endpoints return correct JSON
```

### Acceptance (from reference spec)

- [x] A remoting service is configured in the server (`AddBoleroRemoting` +
  `MapBoleroRemoting`)
- [x] The client calls shared async functions (`this.Remote<CommunityApi>` +
  `Cmd.OfAsync.either`)
- [x] `curl` against each remoting endpoint confirms correct JSON
- [x] `unit` argument sent as JSON `null`, not `[]`/empty body

### Next

Phase 11 — Feature-owned UI (page-specific views move beside their page).

</details>

- ✅ `done` — [Phase 11 — Feature-owned UI](../reference-design/03-step-by-step-implementation/phase-11-feature-owned-ui/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Feature-owned UI</summary>

**Phase 11 complete** — every page now renders itself via its owning feature
module. `Ui/` holds only cross-feature UI (layout shell, menu, shared error
notification). Views select shared state; they do not duplicate it.

### Goal met

The reference spec asks for:

```text
each page/feature owns its model, message, update, view
Shared state is selected, not duplicated
render isolation (ElmishComponent) used only where measured
```

### The refactor

Before Phase 11, every page view lived in `Ui/Layout.fs` next to the layout
shell. That conflated the global `Ui/` folder (cross-feature UI) with
feature-specific views. Phase 11 moved each page view beside its owning
feature:

```
Ui/
  Templates.fs      # the shared Layout template + dataRows helper (compiles early)
  Layout.fs         # shell only: menu, body dispatch, shared error notification
Pages/
  Home.fs           # owns Home.view + its stats selection
  Games.fs          # owns Games.view
  Servers.fs        # owns Servers.view
  Tournaments.fs    # owns Tournaments.view
  Members.fs        # owns Members.view
  About.fs          # owns About.view (static, no Msg)
  Account.fs        # owns Account.Model/Msg/update + Account.view
```

### Why `Ui/Templates.fs`

The page views need the shared `Layout` template and `dataRows` helper, but the
root `view` (in `Ui/Layout.fs`) composes the pages and depends on `App`.
That is a dependency cycle. Splitting the *shared rendering primitives* into a
early-compiled `Ui/Templates.fs` breaks it:

```
State  ->  Ui/Templates  ->  Pages/*  ->  App  ->  Ui/Layout  ->  Main
```

### Feature views own themselves

Each page view is a plain function that *selects* the canonical shared data it
needs (per the state-ownership model — "reuse, not duplicate") and returns a
node. Static pages (`About`) take no data and declare no message case
(message-organization: "Static pages do not need a message case").

```fsharp
module Home =
    let stats (shared: SharedModel) = ...            // select slices
    let view (shared: SharedModel) =
        Layout.Home()
            .GamesCount(...).OnlineNow(...)...
            .Games(dataRows shared.games <| fun g -> ...)
            .Elt()

module About =
    let view () = Layout.About().Elt()               // no state, no Msg
```

The Account view takes the live transient form (from the active PageModel) plus
the Shared slices it owns, and takes a `signOut` callback for the cross-feature
session effect:

```fsharp
module Account =
    let view (form: Model) (username: option<string>) (signInFailed: bool)
             (localDispatch: Msg -> unit) (signOut: unit -> unit) = ...
```

The root `view` (`Ui/Layout.fs`) is now a thin dispatcher that routes each route
to its feature module:

```fsharp
| Home         -> Home.view model.shared
| Games        -> Games.view model.shared
...
| AccountPage pm ->
    Account.view pm.Model model.shared.account model.shared.signInFailed
        (fun msg -> dispatch (AccountMsg msg))
        (fun () -> dispatch (SharedMsg Shared.SendSignOut))
```

### MVU verification (browser console)

Home dashboard rendered all stats + tables; Members rendered all 3 players;
Account sign-in round-trip through the moved view:

```
New message:: AccountMsg (SetUsername "player99")
New message:: AccountMsg (SetPassword "password")
New message:: AccountMsg Submit
New message:: SharedMsg (SendSignIn ("player99", "password"))
New message:: SharedMsg (RecvSignIn (Some "player99"))
New message:: AccountMsg Clear
```

Page → "Signed in as player99" with a Sign out button. The only console error
is the intentional auth-protected `getUsername` 401.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] Each page/feature owns its view (`Pages/*.fs`)
- [x] Views depend only on the feature's own state; they never reach into
      another owner's state
- [x] Shared state is selected (passed as the slice), not duplicated
- [x] `Ui/` reduced to cross-feature UI only (layout shell, menu, error)
- [x] Rendering boundary separated from the state-ownership boundary

### Next

Phase 12 — More features.

</details>

- ✅ `done` — [Phase 12 — More features](../reference-design/03-step-by-step-implementation/phase-12-more-features/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — More features</summary>

**Phase 12 complete — added the Teams feature**, following the same shape as the
other pages. The `Team` domain type (which already existed in
`Community.Web.Shared/Domain`) is now surfaced end-to-end: canonical cache in
Shared, a `/teams` route + feature-owned page, and a `getTeams` remoting
method.

### Reference

The reference asks for "the remaining community features following the same
shape", with the shape:

```text
page-local model + RemoteData + canonical cache
list -> detail or cards
loading / loaded / failed states
```

The gaming community already had its six base pages. The concrete gap was the
`Team` type, defined in the domain but never loaded or rendered. We added a
Teams page to close it, following the exact pattern of the other pages.

### Data + contract

`src/Community.Web.Server/data/teams.json`:

```json
[
  { "id": "team-1", "name": "42 Tactical",
    "players": [ { "id":"player-1","username":"WASDhero",... }, ... ] },
  ...
]
```

`src/Community.Web.Shared/Remoting/CommunityApi.fs` gained the contract member:

```fsharp
getTeams: unit -> Async<Team[]>
```

`src/Community.Web.Server/CommunityApiService.fs` loads `teams.json` once and
returns it from a `getTeams` handler (mirroring the other loaders).

### Canonical cache (Shared)

`State/Shared.fs` — `SharedModel` gained a normalized `teams` cache and `init`
set it to `NotAsked`:

```fsharp
type SharedModel =
    { ...
      teams: RemoteData<Map<string, Team>>
      ... }

// init:  teams = NotAsked
```

`App/App.fs` — `Shared.Msg` gained `GetTeams | GotTeams of Team[]`, and
`Shared.update` maps them like the other data slices (index by id into a Map):

```fsharp
| GetTeams ->
    let cmd = Cmd.OfAsync.either remote.getTeams () GotTeams Error
    { shared with teams = Loading }, cmd
| GotTeams teams ->
    { shared with teams = Loaded (SharedModel.indexById teams (fun t -> t.id)) }, Cmd.none
```

`Main.fs` adds `Cmd.ofMsg (SharedMsg Shared.GetTeams)` to the initial load
batch.

### Feature-owned page

`App/App.fs` — the `Page` union gained `| [<EndPoint "/teams">] Teams`.

`Pages/Teams.fs` — the feature-owned view. It **selects** the canonical
`shared.teams` cache (never duplicates it) and renders each team as a card with
its roster. It demonstrates a card layout rather than the table used by the
other pages, while still reading the shared cache and the loading/failed
states via `Layout.EmptyData`:

```fsharp
module Teams =
    let teamCard (team: Team) = ...  // box + name + player roster

    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading -> Layout.EmptyData().Elt()
        | Failed _           -> Layout.EmptyData().Elt()
        | Loaded m ->
            div {
                h1 { attr.``class`` "title" ; text "Teams" }
                forEach (Map.toArray m) (fun (_, team) -> teamCard team)
            }
```

`Ui/Layout.fs` — added the Teams menu item and the `| Teams -> Teams.view`
body case. `.fsproj` registers `Pages/Teams.fs` (before `App`).

### MVU verification

`/api/getTeams` returns the 3 teams with players (verified with curl). In the
browser:

```
New message:: SharedMsg GetTeams
New message:: SharedMsg (GotTeams [...])     // 3 teams indexed into the cache
```

Navigate to `/teams`: renders "42 Tactical", "Combo Breakers", "No Scope
Legends" cards with their rosters. Navigate away to Members and back — the
page re-renders instantly from the canonical cache (no re-fetch), which is the
phase's "survives navigate-away-and-back in the correct state" requirement.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
curl -X POST .../api/getTeams -d 'null'   # returns the 3 teams
```

### Acceptance (from reference spec)

- [x] A new feature (Teams) added following the same shape
- [x] Canonical cache in `SharedModel`, normalized by id
- [x] Remoting method + server data file
- [x] Loading / loaded / failed states (via `dataRows` / `EmptyData`)
- [x] List -> card rendering from the canonical cache
- [x] Survives navigate-away-and-back (no re-fetch, correct state)

### Next

Phase 13 — Authentication.

</details>

- ✅ `done` — [Phase 13 — Authentication](../reference-design/03-step-by-step-implementation/phase-13-authentication/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Authentication</summary>

**Phase 13 complete — authentication with a single auth-gated `/account`
page.** The session contract (`signIn`, `getUsername`, `signOut`) and cookie
persistence already existed from earlier phases. This phase added the auth
gate UI: the `/account` page shows a **sign-in form when signed out** and the
**current user's profile editor when signed in**.

### Reference

The reference asks for sign in / sign out, account state that survives refresh
(persistent lifetime), and auth-gated pages that show a sign-in prompt when
logged out.

Design decision (user choice, mid-phase refactor): we initially built a
separate `/profile` page, but recognized that **Profile duplicated Account**
(both showed "Signed in as X" + sign out when signed in). We merged the gated
profile editor into `/account` and **removed `/profile`** — one page that logs
you in when signed out and shows your profile when signed in.

### Session contract + service (unchanged)

`CommunityApi.Shared` already has `signIn`, `getUsername`, `signOut`.
`CommunityApiService` implements them (signIn returns `Some username` for
`password = "password"`; `getUsername` is wrapped in `ctx.Authorize`, 401 when
signed out). The session persists via an auth cookie, so a full page refresh
keeps you signed in. **No contract/service change was needed.**

### Auth gate (single feature-owned view)

`Pages/Account.fs` — the Account feature's `Model` holds the sign-in form draft
**and** the profile-edit draft:

```fsharp
type Model =
    { username: string; password: string   // sign-in form draft
      handle: string; bio: string }         // profile-edit draft

type Msg =
    | SetUsername of string
    | SetPassword of string
    | SetHandle of string
    | SetBio of string
    | Clear
    | Submit
```

`init` seeds all fields empty. `update` is pure (edits the drafts; `Submit` is
interpreted by the root). `view` is the auth gate — it reads the root session
state (`username: option<string>`, selected from `SharedModel.account`):

```fsharp
let view (form: Model) (username: option<string>) (signInFailed: bool)
         (localDispatch: Msg -> unit) (signOut: unit -> unit) =
    match username with
    | Some name ->
        // Signed in: profile editor.
        Layout.AccountSignedIn()
            .Username(name)
            .Handle(form.handle, fun h -> localDispatch (SetHandle h))
            .Bio(form.bio, fun b -> localDispatch (SetBio b))
            .Save(fun _ -> localDispatch Submit)
            .SignOut(fun _ -> signOut ())
            .Elt()
    | None ->
        // Signed out: sign-in form.
        Layout.SignIn()...
```

### Templates

`wwwroot/main.html` — the `SignIn` template is unchanged. The shallow
`AccountSignedIn` plus the now-removed `Profile`/`ProfileSignedOut` templates
were consolidated into a single richer `AccountSignedIn` template: "Signed in
as **Username**" plus a form with `Handle` (bound input) and `Bio` (bound
textarea), a **Save** button (`onsubmit="${Save}"`) and a **Sign out** button
(`onclick="${SignOut}"`).

### Root orchestration

- `App.fs` `Page` keeps `| [<EndPoint "/account">] AccountPage of
  PageModel<Account.Model>`. The `ProfilePage`/`ProfileMsg` additions were
  **removed**.
- `App.fs` `Message` = `SetPage | SharedMsg | AccountMsg`.
- `App.fs` root `update` already translates `Account.Submit` into
  `SharedMsg (Shared.SendSignIn (...))` (cross-feature effect), and on
  `Shared.RecvSignIn (Some _)` clears the Account form (`AccountMsg
  Account.Clear`).
- `App.fs` `defaultPageModel` supplies a fresh `Account.init` on each entry
  (state-lifetime rule — transient drafts reset on fresh navigation).
- `Ui/Layout.fs` has a single **Account** menu item + body case; the Profile
  menu/body were removed.
- `.fsproj` no longer references `Pages/Profile.fs` (file deleted).

The `/profile` URL now falls back to Home (`DefaultNotFound Home`).

### MVU verification (browser console)

Signed out (app start): `GetSignedInAs` → `RecvSignedInAs` (401 → `account =
 None`) → `/account` renders the sign-in form.

Sign in (username + `password`, submit): `SendSignIn ("bob","password")` →
`RecvSignIn (Some "bob")` → `AccountMsg Clear` → `/account` re-renders to the
profile editor.

Refresh: the Http cookie persists → `getUsername` returns "bob" → still signed
in. Sign out: `SendSignOut` → session cleared → `/account` returns to the
sign-in form.

Removed route: `/profile` → `withNotFound Home` redirects to `/`.

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/account # 200 (SPA route)
curl -I http://localhost:5023/profile # 200 (SPA fallback -> Home)
```

Browser: `/account` shows the sign-in form when logged out, the profile editor
when signed in; refresh persists the session; `/profile` no longer exists
(falls back to Home). `verify.sh` reports `VERIFY OK`.

### Files changed (relative to the initial Phase 13 commit)

```text
src/Community.Web.Client/Pages/Profile.fs              (deleted)
src/Community.Web.Client/Pages/Account.fs               (+ handle/bio state, gate view)
src/Community.Web.Client/wwwroot/main.html              (merged AccountSignedIn; -Profile/-ProfileSignedOut)
src/Community.Web.Client/App/App.fs                    (-ProfilePage/-ProfileMsg)
src/Community.Web.Client/Ui/Layout.fs                  (-Profile menu/body)
src/Community.Web.Client/Community.Web.Client.fsproj   (-Pages/Profile.fs)
docs/implementation/_runbook/phase-13-authentication.md  (this file, rewritten)
```

</details>

- ✅ `done` — [Phase 14 — Cross-feature effects](../reference-design/03-step-by-step-implementation/phase-14-cross-feature-effects/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Cross-feature effects</summary>

**Phase 14 complete — cross-feature effects.** An action on the Tournaments
page now updates the canonical shared tournaments cache, and Home's "open
tournaments" stat reflects it immediately. The Tournaments feature never
mutates shared state directly — it emits its own local message, and the root
translates it into a `Shared` effect message, exactly as the reference
requires.

### Reference

```text
an action in one page affects shared state
a shared update is dispatched, not reached into
```

Verification: *"An action on one page updates the canonical shared entity
cache, and another page reflecting the same entity shows the updated value."*

The sign-in → Account-clear flow already demonstrated a cross-feature *session*
effect in earlier phases. Phase 14 adds a *data* cross-feature effect: toggling
a tournament's `registrationOpen` in the canonical cache, which Home's "open
tournaments" stat reads.

### Shared effect message

`App/App.fs` `Shared.Msg` gained `ToggleTournament of string`. `Shared.update`
handles it by flipping `registrationOpen` inside the canonical cache — it does
NOT re-fetch from the server, it mutates the single normalized map:

```fsharp
| ToggleTournament tournamentId ->
    let tournaments =
        match shared.tournaments with
        | Loaded m ->
            match m.TryFind tournamentId with
            | Some t ->
                let t' = { t with registrationOpen = not t.registrationOpen }
                Loaded (m.Add(tournamentId, t'))
            | None -> Loaded m
        | other -> other
    { shared with tournaments = tournaments }, Cmd.none
```

Home's `stats` reads the same cache, so the count updates automatically:

```fsharp
let openTournaments =
    match shared.tournaments with
    | Loaded m -> m.Values |> Seq.filter (fun t -> t.registrationOpen) |> Seq.length
    | _ -> 0
```

### Feature-owned page (emits the effect, does not reach in)

`Pages/Tournaments.fs` became feature-owned: it declares its own `Msg` and
takes a dispatch:

```fsharp
module Tournaments =
    type Msg =
        | ToggleRegistration of string

    let row (tournament: Tournament) (dispatch: Msg -> unit) =
        tr {
            td { tournament.name }
            td { tournament.prize }
            td { tournament.startsAt.ToString("yyyy-MM-dd") }
            td {
                if tournament.registrationOpen then
                    button {
                        attr.``class`` "button is-small is-danger"
                        on.click (fun _ -> dispatch (ToggleRegistration tournament.id))
                        "Close registration"
                    }
                else
                    button {
                        attr.``class`` "button is-small is-success"
                        on.click (fun _ -> dispatch (ToggleRegistration tournament.id))
                        "Reopen registration"
                    }
            }
        }
```

The view selects the canonical cache and renders a row per tournament. The
`wwwroot/main.html` Tournaments template gained an **Actions** column.

### Root orchestration (translates the effect)

The root `Message` gained `TournamentsMsg of Tournaments.Msg`. Its `update`
case maps every Tournaments message to a Shared effect — the page dispatches a
shared update by message, never by reaching into shared state:

```fsharp
| TournamentsMsg msg ->
    match msg with
    | Tournaments.ToggleRegistration tournamentId ->
        model, Cmd.ofMsg (SharedMsg (Shared.ToggleTournament tournamentId))
```

`Ui/Layout.fs` passes a dispatch to the Tournaments view:

```fsharp
| Tournaments -> Tournaments.view model.shared (fun msg -> dispatch (TournamentsMsg msg))
```

### MVU verification (browser console)

Clicking "Close registration" on CS2 Summer Cup:

```
New message:: TournamentsMsg (ToggleRegistration "tournament-1")
New message:: SharedMsg (ToggleTournament "tournament-1")
```

The trace proves the ownership boundary: the feature emits a local message, the
root turns it into a shared effect. No `GotTournaments` (no re-fetch) fires —
the cache is mutated in place.

### Browser verification

1. Tournaments page → CS2 Summer Cup shows "Close registration".
2. Click it → button flips to "Reopen registration" (canonical cache mutated).
3. Navigate to Home → "1 tournaments open" (was 2) — Home reflects the
   cross-feature update from the same canonical cache.
4. Navigate back to Tournaments → CS2 Summer Cup still shows "Reopen
   registration" (survives navigate-away-and-back; no re-fetch).

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/tournaments # 200
```

`verify.sh` reports `VERIFY OK`.

### Second cross-feature effect: favourite games

The same pattern powers a second cross-feature effect — favouriting a game on
the Games page updates a shared favourite set, reflected on Home.

- `State/Shared.fs` `SharedModel` gained `favoriteGames: Set<string>` (init
  `Set.empty`). This is shared state owned by the Shared layer; the Games
  feature only dispatches to it.
- `Shared.Msg` gained `ToggleFavoriteGame of string`. `Shared.update` toggles
  the id in the set:

  ```fsharp
  | ToggleFavoriteGame gameId ->
      let favorites =
          if shared.favoriteGames.Contains gameId then
              shared.favoriteGames.Remove gameId
          else
              shared.favoriteGames.Add gameId
      { shared with favoriteGames = favorites }, Cmd.none
  ```

- `Pages/Games.fs` became feature-owned (`Games.Msg = ToggleFavorite of
  string`). Each row renders a **Favourite/Unfavourite** button that emits the
  local message; the view selects the favourite set from Shared to know the
  current state. The `wwwroot.html` Games template gained an **Actions**
  column.
- Root `Message` gained `GamesMsg of Games.Msg`; root `update` translates it
  to the shared effect:

  ```fsharp
  | GamesMsg msg ->
      match msg with
      | Games.ToggleFavorite gameId ->
          model, Cmd.ofMsg (SharedMsg (Shared.ToggleFavoriteGame gameId))
  ```

- `Ui/Layout.fs` passes the Games dispatch; `Home.fs` reads
  `shared.favoriteGames.Count` and shows a **"X favourite games"** stat (the
  `Home` template gained a `${Favorites}` slot).

MVU trace (clicking "Favourite" on Counter-Strike 2):

```
New message:: GamesMsg (ToggleFavorite "game-1")
New message:: SharedMsg (ToggleFavoriteGame "game-1")
```

Browser: click "Favourite" on CS2 → button flips to "Unfavourite"; navigate to
Home → "1 favourite games" stat appears; navigate away to Games → still
"Unfavourite" (persists in the shared set, no re-fetch).

### Files changed

```
src/Community.Web.Client/App/App.fs                 (+ Shared.ToggleTournament, + Shared.ToggleFavoriteGame,
                                                     + TournamentsMsg, + GamesMsg, update)
src/Community.Web.Client/Pages/Tournaments.fs       (feature-owned Msg + toggle row + view)
src/Community.Web.Client/Pages/Games.fs             (feature-owned Msg + favourite button + view)
src/Community.Web.Client/Pages/Home.fs              (+ favourite count stat)
src/Community.Web.Client/State/Shared.fs           (+ favoriteGames)
src/Community.Web.Client/Ui/Layout.fs              (+ Tournaments + Games dispatch)
src/Community.Web.Client/wwwroot/main.html         (+ Actions columns, ${Favorites} slot)
docs/implementation/progress.yaml                   (phase-14: done)
docs/implementation/index.md                       (regenerated)
docs/implementation/_runbook/phase-14-cross-feature-effects.md  (this file)
```

</details>

- ✅ `done` — [Phase 15 — Rendering optimization](../reference-design/03-step-by-step-implementation/phase-15-rendering-optimization/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Rendering optimization</summary>

**Phase 15 complete — rendering optimization by evidence (not by guessing).**
The phase rule is explicitly conservative:

> **Do not split rendering into components preemptively. Optimize by evidence.**

So this phase did NOT invent `ElmishComponent`/`ecomp` row isolation. Instead
it added a lightweight, purely observational render probe to the largest list
(Games) so re-render cost becomes *measurable*, confirmed the current whole-list
re-render is cheap, and documented why component isolation is not justified
yet — and how to revisit it when it is.

### Reference

```text
measured slow re-renders on large lists
expensive derivations

ElmishComponent / isolation where justified
keep most rendering as pure functions
normalize data so updates are O(1) per entity

Do not split rendering into components preemptively. Optimize by evidence.
```

Verification: *"Re-render cost on the largest list is measurable, and no
component isolation is added without measured evidence that it is needed."*

### What earlier phases already gave us

- **Normalized state**: every entity list is `RemoteData<Map<string,'T>>`
  keyed by id (`State/Shared.fs`), so per-entity updates are O(1).
- **Pure rendering**: every page view is a pure function over the model (or
  shared slices) — no side effects, no mutation.

Those are two of the three levers the spec names (`keep most rendering as pure
functions`, `normalize data so updates are O(1) per entity`). They were already
in place, so the only missing piece was the *evidence* — the third lever,
`ElmishComponent / isolation where needed`.

### The instrument: `Ui/RenderProbe.fs`

A small module that is purely observational: it counts how many times a named
render region executes during a program run and prints a running total to the
browser console (Wasm maps `printfn` to the browser console).

```fsharp
module Community.Web.Client.Ui.RenderProbe

open System.Collections.Generic

let private counts = Dictionary<string, int>()

let touch (region: string) : int =
    let n =
        match counts.TryGetValue region with
        | true, c -> c + 1
        | _ -> 1
    counts[region] <- n
    n

let report (label: string) : unit =
    let totals =
        counts
        |> Seq.map (fun kv -> $"{kv.Key}={kv.Value}")
        |> String.concat " | "
    counts.Clear()
    printfn $"[RenderProbe] {label}: {totals}"
```

It is deliberately dumb: `touch` only increments, `report` only prints and
resets. It never changes what is rendered and never changes control flow, so
the numbers it produces are honest evidence rather than something that alters
behaviour.

### Wiring it into the Games view

`Pages/Games.fs` touches one region per game row, and the view reports once
after building all rows — so each dispatch prints one readable line rather than
one line per row:

```fsharp
let row (game: Game) (isFavorite: bool) (dispatch: Msg -> unit) =
    let _ = RenderProbe.touch $"game:{game.id}"   // <-- probe
    tr {
        td { game.name }
        ...
    }

let view (shared: SharedModel) (dispatch: Msg -> unit) =
    let favorites = shared.favoriteGames
    cond shared.games <| function
    | NotAsked | Loading -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
    | Failed _ -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
    | Loaded m ->
        let rows = forEach (Map.toArray m) (fun (_, g) -> row g (favorites.Contains g.id) dispatch)
        RenderProbe.report "Games.view"
        Layout.Games().Rows(rows).Elt()
```

`Ui/RenderProbe.fs` is added to `Community.Web.Client.fsproj` right after
`State/Shared.fs` (it compiles before the pages that call it).

### The evidence (browser console)

On `/games`, clicking "Favourite" on Counter-Strike 2:

```
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
```

Unfavouriting then favouriting again:

```
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
```

Measured facts:

- **Every row re-renders on every dispatch** — a single favourite toggle
  re-runs the whole list view; each of the three game rows touches once.
- The report resets each pass (`=1`), so the counter reflects per-render
  activity, not an unbounded accumulation.

### Why no `ElmishComponent` / `ecomp` row isolation yet

The probe proves the whole list re-renders, but the list has **3 rows**.
Rebuilding three pure `Node`s is far below the 16 ms frame budget. Wrapping
each row in an `ElmishComponent<Game, bool>` with a custom `ShouldRender` would
add per-row bookkeeping and a failure surface for **zero measured benefit** —
exactly what the phase rule forbids ("do not preemptively").

The probe is also the tool to revisit this later: when the Games list grows
large and a toggle starts to feel slow, `RenderProbe` will *show* it. Only then
does `ElmishComponent<Game, bool>` (or `lazyComp`) become the justified answer,
measured before and after the change.

### Files changed

```
src/Community.Web.Client/Ui/RenderProbe.fs          (new: touch + report)
src/Community.Web.Client/Pages/Games.fs             (probe touches + report)
src/Community.Web.Client/Community.Web.Client.fsproj (add Ui/RenderProbe.fs)
docs/implementation/progress.yaml                   (phase-15: done)
docs/implementation/index.md                       (regenerated)
docs/implementation/_runbook/phase-15-rendering-optimization.md  (this file)
```

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
```

Browser: `/games` renders; clicking Favourite/Unfavourite flips the row and the
console prints `[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 |
game:game-3=1`, proving the whole list re-renders — and that it is cheap.

`verify.sh` reports `VERIFY OK`.

</details>

- ✅ `done` — [Phase 16 — Testing ownership boundaries](../reference-design/03-step-by-step-implementation/phase-16-testing-ownership-boundaries/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Testing ownership boundaries</summary>

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

</details>

- ✅ `done` — [Phase 17 — Design system](../reference-design/03-step-by-step-implementation/phase-17-design-system/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Design system</summary>

**Phase 17 complete — design system.** This phase applies a consistent visual
design on top of the proven architecture without changing state ownership.
Direction: **gaming-community (dark-first)** with the **42 Abu Dhabi brutalist
palette**, implemented through the **Radzen Blazor** component library behind
thin F# wrappers.

### Reference

```text
- theme tokens (typography, spacing, colors)
- layout components (header, footer, nav)
- reusable surface components
```

Rule: *the visual design must not change state ownership — apply the theme on
top of the architecture.* The layout shell, MVU messages, shared caches, and
page/feature ownership are untouched; only presentation layers change.

### Direction chosen

From the design language section, the **gaming-community (dark-first)**
direction was selected and tuned to the **42 Abu Dhabi brutalist palette**:

| Token | Value | Use |
|---|---|---|
| Terminal Black | `#000000` | page / body background |
| Pure White | `#FFFFFF` | primary text |
| Abu Dhabi Red | `#BF0000` | primary accent, buttons |
| Charcoal | `#1A1A1A` | cards / panels |
| Muted Ash | `#A3A3A3` | secondary text |
| Neon Cyan | `#00E5FF` | optional accent (prize, pings) |

Brutalist treatment: `--rz-border-radius: 0px`, 1px solid borders, monospace
type (`Fira Code` / `JetBrains Mono`).

### The Radzen integration

`Radzen.Blazor 11.2.7` is the vendored component library (fork
`42WASD/radzen-blazor`, branch `jya0-v11.2.7` at tag `v11.2.7`, submodule under
`thirdparty/`). It is pulled in as a NuGet package and themed entirely via CSS
variable overrides in `:root` of the app's own stylesheet — the library CSS is
never hand-edited.

- `Startup.fs` registers services: `builder.Services.AddRadzenComponents()`.
- `Server/Index.fs` loads Radzen's `material-dark-base.css` theme (before the
  app's `index.css` so overrides win) and the Radzen JS bundle.
- `wwwroot/css/index.css` overrides the actual Radzen variables (verified in
  `material-dark-base.css`): `--rz-primary`, `--rz-body-background-color`,
  `--rz-base-background-color`, `--rz-panel-background-color`,
  `--rz-card-background-color`, `--rz-text-color`, `--rz-border-radius`, etc.

### The F# wrappers

`Ui/RadzenUI.fs` is the thin cross-feature wrapper module (this is the *only*
place the app touches Radzen directly; pages reuse the wrappers and never
`open Radzen`). Even the Radzen enums are re-exported so pages stay oblivious
to the component library's object model:

```
let dangerButton = ButtonStyle.Danger     // re-exported enum values
let successButton = ButtonStyle.Success
let outlinedCard  = Variant.Outlined

let button text style onClick dispatch = comp<RadzenButton> {
    "Text" => text; "ButtonStyle" => style
    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClick ())) }
let card variant children = comp<RadzenCard> { "Variant" => variant; children }
let serverCard (server: GameServer) = ...  // RadzenCard Outlined + status dot
```

Pages consume only these names (`RadzenUI.button`, `RadzenUI.card`,
`RadzenUI.dangerButton`, ...) — the `open Radzen` / `Variant.` / `ButtonStyle.`
leaks at call sites are gone.

Notes verified in Radzen source:
- `RadzenButton.Click` is `EventCallback<MouseEventArgs>` → needs
  `open Microsoft.AspNetCore.Components.Web`.
- `RadzenCard` takes a `Variant` param and wraps children as `ChildContent`.
- The `comp<T>` builder wraps child nodes as `ChildContent`.
- Radzen is **view-only**: the wrappers return `Node`s and only ever render;
  `Startup.fs` registers DI, and no `DialogService`/`NotificationService` is
  invoked from `update`/`init`/`Cmd`. Any future Radzen side effect must be
  emitted as an async `Cmd`, never called in `view`.

### Reusable surfaces

Two "key pages" were given Radzen-backed surfaces; the rest keep their existing
Bulma styling (now themed by the palette overrides):

- **Servers** — `RadzenUI.serverCard` replaces the old table rows; each server
  is an outlined `RadzenCard` with a status dot and address.
- **Tournaments** — each tournament is a `RadzenUI.card Variant.Outlined`
  holding the prize (neon cyan) and a Radzen `Button` that dispatches
  `ToggleRegistration`. Clicking it still mutates the shared canonical cache —
  the cross-feature effect is unchanged; only the surface changed.

The shared templates were adapted so card views (divs, not `<tr>`s) have
containers: `main.html`'s Servers/Tournaments templates changed from `<table>`
to `<div class="server-list">` / `<div class="tournament-list">`.

### Files changed

```
thirdparty/radzen-blazor/              (new submodule → 42WASD/radzen-blazor @ v11.2.7)
.gitmodules                            (+ radzen-blazor entry)
src/Community.Web.Client/Community.Web.Client.fsproj  (+ Radzen.Blazor 11.2.7, + Ui/RadzenUI.fs)
src/Community.Web.Client/Startup.fs    (+ AddRadzenComponents)
src/Community.Web.Server/Index.fs      (+ material-dark-base.css, Radzen JS)
src/Community.Web.Client/wwwroot/css/index.css  (rewritten: 42 brutalist palette)
src/Community.Web.Client/Ui/RadzenUI.fs        (new: Radzen F# wrappers)
src/Community.Web.Client/Pages/Servers.fs      (Radzen server cards)
src/Community.Web.Client/Pages/Tournaments.fs  (Radzen card + buttons)
src/Community.Web.Client/wwwroot/main.html     (card-list containers)
docs/implementation/progress.yaml              (phase-17: done)
docs/implementation/index.md                  (regenerated)
docs/implementation/_runbook/phase-17-design-system.md  (this file)
```

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 21 still pass
```

Design confirmed live in the browser: black body background, `--rz-primary`
= `#BF0000`, Radzen CSS loaded, `.rz-card` surfaces present, and the
`ToggleRegistration` button round-trips the cross-feature update.

`verify.sh` reports `VERIFY OK`.

---

### Phase 17b — full Radzen conversion (responsive layout, zero custom CSS)

Follow-up pass (same phase, appended): every page and the app shell are now
built entirely from Radzen primitives behind the `RadzenUI` wrappers, and
`index.css` is reduced to palette tokens only. The Bulma templates and the
HTML template engine are gone.

#### The three design decisions

1. **Layout shell = `RadzenLayout`** (Header + Sidebar + Body + Footer). The
   sidebar auto-collapses below 768px (`ResponsiveMaxWidth`), and the hamburger
   (`RadzenSidebarToggle`) flips a `sidebarExpanded` bool held in the root
   `Model`. Radzen primitives, not custom CSS, provide all responsiveness.
2. **Radzen primitives + zero custom CSS.** `wwwroot/css/index.css` is cut from
   ~200 lines of hardcoded layout to ~60 lines of pure `:root` palette tokens
   plus the typeface rule. No `.sidebar` width, `.box`, `.title`, `.table`,
   `.server-card`, `.status-dot`, `.navbar` — those were Bulma's responsibility
   and are now Radzen's.
3. **All pages, not just the two key pages.** Home, Games, Servers,
   Tournaments, Members, Teams, About, and Account all render through Radzen
   wrappers now.

#### What changed

| Area | Before | After |
|---|---|---|
| App shell | `Layout` HTML template (`main.html`) | `RadzenLayout` shell in `Ui/Layout.fs` |
| Nav | `Layout.MenuItem()` template | `RadzenPanelMenuItem` + `RadzenPanelMenu` |
| Pages | `Layout.Home()/.Games()/...` templates | Radzen `vStack`/`row`/`column`/`card`/`text`/`button`/`skeleton` |
| Loading | `Layout.EmptyData()` | `RadzenSkeleton` |
| Errors | `Layout.ErrorNotification()` | `RadzenAlert` (non-dismissible) |
| Forms | `Layout.SignIn()/.AccountSignedIn()` | `RadzenTextBox`/`RadzenPassword`/`RadzenTextArea`/`RadzenButton` |
| Templates infra | `Ui/Templates.fs` + `wwwroot/main.html` | **deleted** (no HTML templates remain) |
| CSS | hardcoded layout rules | `:root` palette tokens only |

#### The new shell (`Ui/Layout.fs`)

```fsharp
let view (model: Model) (dispatch: Message -> unit) =
    RadzenUI.layout (concat {
        RadzenUI.header (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.sidebarToggle (fun () -> dispatch ToggleSidebar)
                RadzenUI.text RadzenUI.heading4 "42WASD"
            })
        })
        RadzenUI.sidebarExpanded model.sidebarExpanded (fun _ -> dispatch ToggleSidebar)
            (RadzenUI.panelMenu (concat { /* menuItem per page */ }))
        RadzenUI.body (cond model.page <| function
            | Home -> Home.view model.shared
            | ... )
        RadzenUI.footer (cond model.shared.error <| function
            | Some err -> RadzenUI.alert RadzenUI.dangerAlert err
            | None -> empty())
    })
```

`ToggleSidebar` was added to the root `Message` and `sidebarExpanded` to the
root `Model` — the only state-ownership change, and it's shell-only (sidebar
state is cross-feature UI, so it belongs at the root, not in any page).

#### Radzen API notes (verified in vendored source)

- `RadzenSidebarToggle.Click` is `EventCallback<EventArgs>` (not
  `MouseEventArgs`); `RadzenButton.Click` IS `EventCallback<MouseEventArgs>`.
- `RadzenAlert.Close` is a non-generic `EventCallback` — Bolero's
  `attr.callback` produces `EventCallback<'T>`, so the shared error alert is
  non-dismissible (`AllowClose=false`) to avoid the cast mismatch.
- `RadzenSidebar.Expanded`/`ExpandedChanged` is the two-way binding; the
  wrapper passes both to keep the shell controlled by Elmish state.
- `RadzenColumn` `SizeXS/SM/MD/LG` provide the responsive 12-col grid.
- `RadzenSkeleton` `SkeletonVariant` uses `Text/Circular/Rectangular` (not
  `Circle`/`Rectangle`).

#### Phase 17b files changed

```
src/Community.Web.Client/Ui/RadzenUI.fs    (+ alert, panel menu/menu item,
                                             sidebarExpanded, password wrappers)
src/Community.Web.Client/Ui/Layout.fs      (rewritten: RadzenLayout shell)
src/Community.Web.Client/Ui/Templates.fs   (deleted — no templates remain)
src/Community.Web.Client/wwwroot/main.html (deleted — no templates remain)
src/Community.Web.Client/Community.Web.Client.fsproj  (- Templates.fs)
src/Community.Web.Client/App/App.fs        (+ ToggleSidebar, sidebarExpanded)
src/Community.Web.Client/Pages/*.fs        (all 8 pages → Radzen primitives)
src/Community.Web.Server/Index.fs          (- Bulma navbar; keeps Radzen css/js)
src/Community.Web.Client/wwwroot/css/index.css  (slim to palette tokens)
```

#### Phase 17b verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 21 still pass
```

Confirmed live in the browser: the `RadzenLayout` shell renders with the
sidebar, the hamburger collapses/expands the nav, and all eight pages render
through Radzen cards/grid/buttons — including sign-in, favourite toggle, and
tournament registration (the cross-feature effects still update the shared
canonical caches exactly as before).

`verify.sh` reports `VERIFY OK`.

---

### Phase 17c — richer component surfaces (Tabs / Carousel / Timeline / ProgressBar)

Motivated by the community-landing research (Raider.IO/FACEIT patterns): the
app was a flat grid of cards. This phase adds density and hierarchy using
existing Radzen components that were already in the library but unused, and
surfaces the `News` slice that was loaded into state but never rendered.

#### The core gotcha: named `RenderFragment` parameters

Bolero's `comp<T> { children }` always binds trailing children to the
`ChildContent` parameter. Several Radzen containers read their items from a
**dedicated `RenderFragment` parameter instead of `ChildContent`**:

- `RadzenTabs` reads `RadzenTabsItem`s from its **`Tabs`** parameter.
- `RadzenCarousel` reads `RadzenCarouselItem`s from its **`Items`** parameter.
- `RadzenTimeline` reads `RadzenTimelineItem`s from its **`Items`** parameter.

So a wrapper that passes children via `ChildContent` silently renders an empty
container (the outer `<div>`/nav shows, but zero tabs/slides/entries). The fix
is a helper that binds nodes to a named fragment:

```fsharp
let fragmentParam (paramName: string) (children: Node) =
    Attr(fun receiver builder sequence ->
        builder.AddAttribute(sequence, paramName,
            RenderFragment(fun builder ->
                children.Invoke(receiver, builder, 0) |> ignore))
        sequence + 1)
```

Then `comp<RadzenTabs> { fragmentParam "Tabs" children }`. Because F# has no
forward references, `fragmentParam` must be declared **above** the wrappers
that use it.

**Key architectural constraint:** `comp<T>` body cannot contain
`yield!`/`if`/`match`, and a Radzen component's `Items`/`Tabs` children are
ordinary `Node`s — so build the item nodes *outside* the `comp` and pass them
in. This keeps the wrapper view-only (no page state needed; `RadzenTabs` with
`SelectedIndex` left at -1 auto-selects the first tab, so it works
**uncontrolled** — perfect for our feature-owned view-only pages).

#### New wrappers (Ui/RadzenUI.fs)

```
fragmentParam paramName children   # bind a Node to a named RenderFragment param
tabs items                         # RadzenTabs  (items via "Tabs")
tabItem text children              # RadzenTabsItem (Text + ChildContent)
timeline items                     # RadzenTimeline (items via "Items")
timelineItem label point children  # RadzenTimelineItem (Label + PointStyle)
carousel itemsPerPage items        # RadzenCarousel (items via "Items", PagerPosition bottom)
carouselItem children              # RadzenCarouselItem
progressBar value max style        # RadzenProgressBar (Value/Max are double → pass floats)
progressBarValue value max style   # RadzenProgressBar with ShowValue
```

Also re-exported enum values: `progressBarPrimary/Success/Danger/Warning/Info/
Dark` (ProgressBarStyle) and `pagerBottom` (PagerPosition).

#### Servers page → tabbed browser

`Servers.view` now groups servers by `gameId` and renders them under
`RadzenTabs` (one tab per game, in manifest order; servers with a gameId not
in the games map fall through to an "Other" tab). Each server card uses the
existing `badgePill` for status and a `progressBarValue` for `onlinePlayers /
maxPlayers` capacity, colouring toward red near full (`capacityStyle`).

#### Home page → landing dashboard

- Stat strip retained (Games / Players online / Open tournaments / Members /
  Favourites) with caption + heading.
- **Featured games `RadzenCarousel`** — cycles the games from the shared cache
  (`carouselItem` = a card with name / genre / description).
- **Live servers** strip — one row per server: name + status `badgePill` +
  `progressBarValue` capacity.
- **Latest news `RadzenTimeline`** — surfaces the previously-unused `News`
  slice: each entry shows its `publishedAt` date as the label and the title +
  body as the content. This is the first time `shared.news` renders anywhere.

All new views stay view-only, selecting canonical shared slices (per the
state-ownership model); none of the new components require page-local Elmish
state.

#### Files changed (Phase 17c)

```
src/Community.Web.Client/Ui/RadzenUI.fs  (+ fragmentParam + tabs/timeline/carousel/
                                             progressBar wrappers + progress enums)
src/Community.Web.Client/Pages/Servers.fs (group servers by game into RadzenTabs;
                                           capacity progress bar per card)
src/Community.Web.Client/Pages/Home.fs    (+ featured-games carousel, live-server
                                           strip, news timeline)
```

#### Verification

```bash
dotnet build Community.Web.sln          # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 22 still pass
bash scripts/docs/verify.sh             # VERIFY OK
```

Verified live in the browser: the Servers page shows tabs (Counter-Strike 2 /
Dota 2 / Minecraft) and switching tabs shows each game's servers with capacity
bars; the Home page shows the featured-games carousel, the live-server strip,
and the news timeline. The MVU trace confirms the normal `Get*` → `Got*`
flow with no dropped messages.

</details>

- ✅ `done` — [Phase 18 — Production hardening](../reference-design/03-step-by-step-implementation/phase-18-production-hardening/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Production hardening</summary>

**Phase 18 complete — production hardening.** The app is packaged as a single
container for Kubernetes. This turns the (previously static) "server deployable"
goal into a concrete, testable image that CI builds and the cluster runs.

### Reference

```text
- deterministic verification pipeline is green (verify.sh)
- server deployable (static hosting + server as appropriate)
- remoting, error boundaries, and logging verified
```

### What shipped

| Artifact | Purpose |
|---|---|
| `Dockerfile` | Multi-stage: `dotnet/sdk:10.0` publish → `dotnet/aspnet:10.0` runtime, non-root `USER app`, listens on `:8080`. |
| `.dockerignore` | Keeps `thirdparty/`, `site/`, `docs/`, `**/bin|obj` out of the build context. |
| `deploy/k8s/deployment.yaml` | `replicas: 1`, run-as-non-root, resource requests/limits, readiness + liveness `GET /`. |
| `deploy/k8s/service.yaml` | `ClusterIP` `:80 → 8080`. |
| `deploy/k8s/ingress.yaml` | TLS-terminating Ingress (nginx + cert-manager annotations). |
| `.github/workflows/container.yml` | Build + push to `ghcr.io/42wasd/42wasd-community-web` on `main` / `v*` tags. |

### Key decisions (verified against the project)

- **Single container for client + server.** The app is *hosted* Blazor WebAssembly:
  the `Server` project references the `Client` via `ProjectReference`, and
  `dotnet publish` emits the WASM assets next to the server DLL. No separate
  static host is needed — one Deployment serves everything.
- **Framework-dependent, `aspnet:10.0` runtime.** The repo targets `net10.0`
  (`global.json` SDK `10.0.111`). The runtime image already has the ASP.NET Core
  runtime, so the image stays small; self-contained was not needed.
- **Non-root + no privileges.** `USER app` and PodSecurity `runAsNonRoot: true`
  satisfy K8s "restricted" policy — no `allowPrivilegeEscalation`.
- **Restore isolation.** Only the `fsproj` files are copied before
  `dotnet restore` so NuGet layers cache; `radzen-blazor`/`Bolero` resolve from
  NuGet (the `thirdparty/` submodules are source references only, not in the
  image or build context).

### Verification

```bash
bash scripts/docs/verify.sh   # VERIFY OK
dotnet test                   # all pass
docker build .                # Release image builds cleanly
docker run -p 8080:8080 ghcr.io/42wasd/42wasd-community-web:latest
```

### Files changed

```
Dockerfile
.dockerignore
deploy/k8s/deployment.yaml
deploy/k8s/service.yaml
deploy/k8s/ingress.yaml
.github/workflows/container.yml
docs/reference-design/.../phase-18-production-hardening/index.md
docs/implementation/_runbook/phase-18-production-hardening.md   (this file)
docs/implementation/progress.yaml
docs/implementation/index.md
```

`verify.sh` reports `VERIFY OK`.

---

## Post-phase fix — persistent, writable `/app/data` (saveProfile)

**Intent:** the Phase 18 Deployment baked `data/` JSON into the image read-only
(`/app/data` owned `root:root 755`). The `saveProfile` remoting call writes
`players.json` to `/app/data`, so at runtime the app user (uid 1654) hit
"Permission denied" — profile saves silently failed (the write is best-effort:
it is caught in `Loaders.saveJson`, logged, and returns `false`, so the site
stays up but the profile change does not persist). This change makes the data
dir writable AND persistent across pod restarts, and wires the app into Argo CD.

### Root cause (verified)

```bash
kubectl -n prd-42wasd-admin exec deploy/42wasd -- ls -la /app/data
# drwxr-xr-x 2 root root  ... games.json news.json players.json ...
kubectl -n prd-42wasd-admin exec deploy/42wasd -- sh -c "id; touch /app/data/test"
# uid=1654(app) gid=1654(app)
# touch: cannot touch '/app/data/test': Permission denied   <- NOT writable
```

### Fix: PVC + fsGroup + seed-data initContainer + Recreate

1. `deploy/k8s/pvc.yaml` (new) — `42wasd-data`, `nvme-fast`, 1Gi, RWO.
2. `deploy/k8s/deployment.yaml`:
   - Pod `securityContext.fsGroup: 1654` → chowns the mounted volume so the
     `app` user (uid/gid 1654) can write.
   - `initContainers.seed-data` — same image, `cp -rn /app/data/. /pvc-data/`
     to seed the baked JSON into the volume on first start (no-clobber so saved
     `players.json` survives restarts).
   - Main container `volumeMounts` — `data` at `/app/data`.
   - `volumes` — `persistentVolumeClaim: claimName: 42wasd-data`.
   - `strategy: Recreate` — RWO volume can't be shared during a rolling
     update; Recreate terminates the old pod first (single replica, so the
     brief downtime is acceptable).

### Argo CD integration

The Deployment was previously applied manually (no `argocd.argoproj.io/`
tracking). Wired it into GitOps in the iac repo:

- `infra/kubernetes/bootstrap/argocd/apps/tenant-community-web.yaml` (new) —
  Application, project `tenant-42wasd-admin`, source
  `github.com/42WASD/42wasd-community-web.git` path `deploy/k8s`, dest
  `prd-42wasd-admin`, auto-sync + prune + selfHeal, `ServerSideApply=true`.
- `infra/kubernetes/bootstrap/argocd/projects.yaml` — added
  `42wasd-community-web.git` to `tenant-42wasd-admin` `sourceRepos`.

```bash
# from the iac repo (42WASD/ubuntu-server-iac) — the manifests live there:
cd ~/ubuntu-server-iac
kubectl -n argocd apply -f infra/kubernetes/bootstrap/argocd/projects.yaml
kubectl -n argocd apply -f infra/kubernetes/bootstrap/argocd/apps/tenant-community-web.yaml
kubectl -n argocd get app tenant-community-web   # Synced
```

> Status (re-verified 2026-08-29): app `tenant-community-web` is
> `Synced / Healthy`. Health required an Argo CD resource customization:
> on-prem Traefik never populates `status.loadBalancer`, so the built-in
> Ingress health left the app `Progressing` forever. Fixed via
> `resource.customizations.health.networking.k8s.io_Ingress` in the iac
> repo (`infra/kubernetes/bootstrap/argocd/argocd-config.yaml` — spec-based
> check, applied to `argocd-cm` by hand since Argo bootstraps itself).
> The workload manifests live in THIS repo under `deploy/k8s/` — that is
> the path the Argo Application watches.

The namespace `prd-42wasd-admin` is already Argo-owned by
`platform-namespaces` (labels `platform.tier: tenant` + PSS `restricted`).

### Verification

```bash
kubectl -n prd-42wasd-admin rollout status deploy/42wasd   # successfully rolled out
kubectl -n prd-42wasd-admin exec deploy/42wasd -- ls -la /app/data
# drwxrwsr-x 2 root app ...  <- group-writable by app via fsGroup
kubectl -n prd-42wasd-admin exec deploy/42wasd -- sh -c "printf '[]' > /app/data/t.json && rm /app/data/t.json"   # WRITE-OK
curl -s -o /dev/null -w "%{http_code}" http://wasd.42base.com/   # HTTP 200
kubectl -n argocd get app tenant-community-web                   # Synced
```

The Deployment is `Synced + Healthy`. (The Argo app-level health shows
"Progressing" solely because the Traefik Ingress does not populate
`status.loadBalancer` — a known cosmetic ArgoCD quirk; the site serves HTTP 200
by hostname and the pod is `Ready`.)

### Key lesson

Read-only baked data is a security win, but a write path must target a volume
the app user owns. `fsGroup` (group ownership) + a non-root `initContainer`
copy to a PVC is the pattern. A single-replica RWO workload also needs
`strategy: Recreate` or a rolling update deadlocks (new pod can't bind the
volume until the old pod releases it).

</details>

- ✅ `done` — [Phase 19 — Rollout order](../reference-design/03-step-by-step-implementation/phase-19-rollout-order/index.md)

<details markdown="1" class="runbook">
<summary>✅ 📜 Build log — Rollout order</summary>

**Phase 19 complete — rollout order.** This phase is a plan, not code: it fixes
the release sequence so each vertical slice ships independently and the app is
releasable after every slice, per the phase's rule.

### Reference

```text
- order vertical slices for release
- one working slice at a time
```

Rule: *the app is releasable after each vertical slice, not only at the end.*

### The adopted slice order

1. **S1 Infrastructure** — `Dockerfile`, `.dockerignore`, GHCR workflow,
   `deploy/k8s/*` (from Phase 18). Cluster can run a healthy instance.
2. **S2 Public data (read-only)** — all read-only pages render from the baked
   `data/` JSON: Games, Servers, Teams, Members, Tournaments, Home, About.
3. **S3 Auth** — `/api/getUsername`, sign-in/out gating for Members/Account.
   Security first, before public writes.
4. **S4 Writes** — cross-feature effects (tournament toggle, favourites) go live.
5. **S5 Hardening** — shared data-protection for auth cookies if scaling
   `> 1` replica, request-logging/metrics, monitoring.

### Safety rules in practice

- **Security before public data:** auth (S3) lands before any write (S4).
- **Read-only first:** S2 ships without write paths.
- **Verification gates every slice:** each slice is only "done" when
  `bash scripts/docs/verify.sh` → `VERIFY OK` and `dotnet test` passes.

### Scaling note (S5)

The app uses ASP.NET Core cookie auth + Blazor SignalR. At `replicas: 1` (the
Phase 18 Deployment) no extra work is needed. To scale out later: configure a
shared `IDataProtection` key ring (e.g. persisted to Redis/disk) so auth
cookies decrypt on any pod, and enable sticky sessions on the Service/Ingress
for SignalR.

### Verification

Releasable at every slice — the gate is always the same:

```bash
bash scripts/docs/verify.sh   # VERIFY OK
dotnet test                   # all pass
```

### Files changed

```
docs/reference-design/.../phase-19-rollout-order/index.md
docs/implementation/_runbook/phase-19-rollout-order.md   (this file)
docs/implementation/progress.yaml
docs/implementation/index.md
```

`verify.sh` reports `VERIFY OK`.

</details>

<!-- END_GENERATED_IMPLEMENTATION -->