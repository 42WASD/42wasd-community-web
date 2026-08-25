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

**14 / 20** phases/sections complete (**70%**).

<div class="progress-row" style="max-width:720px;padding:8px 0;"><div class="progress-track"><div class="progress-fill progress-fill--shimmer" style="--w:70.0%"></div></div><div class="progress-pct">70%</div></div>

| Status | Count |
|--------|-------|
| ✅ done | 14 |
| 🔶 in-progress | 0 |
| ⬜ not-started | 6 |
| ❌ blocked | 0 |
| ⏸️ deferred | 0 |

## Progress by part

### 70% — Part III — Step-by-step implementation

<div class="tip" style="display:flex;align-items:center;gap:8px;max-width:520px;padding:2px 0 10px;"><div class="progress-track"><div class="progress-fill" style="--w:70.0%"></div></div><div class="progress-pct" style="font-size:.85em;">70%</div><div class="tip-box"><strong>Done (14)</strong>
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
<hr style="opacity:.3;margin:6px 0;"><strong>Pending (6)</strong>
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

**Phase 13 complete — added authentication + an auth-gated `/profile` page.**
The session contract (`signIn`, `getUsername`, `signOut`) and the Account
(sign-in) page already existed from earlier phases. This phase made auth
meaningful by adding a second, **auth-gated** page: `/profile`, which shows a
profile editor to signed-in users and a sign-in prompt to everyone else.

### Reference

The reference asks for pages that are only reachable by signed-in users — a
gate that renders different content based on session state. The obvious fit
for the gaming community was a personal **profile** page: you must be signed
in to view or edit your own profile.

Design decision (user choice): **New `/profile` page (gated)** rather than
gating an existing page, so we demonstrate the gate on a purpose-built page.

### Session contract (unchanged)

`Community.Web.Shared/Remoting/CommunityApi.fs` already exposed:

```fsharp
signIn: string * string -> Async<option<string>>
getUsername: unit -> Async<string>
signOut: unit -> Async<unit>
```

`Community.Web.Server/CommunityApiService.fs` implements them (sign-in
returns `Some username` for `password = "password"`; `getUsername` is wrapped
in `ctx.Authorize`, returning the identity name or 401). The session persists
via an auth cookie, so a full page refresh keeps you signed in. **No change to
the contract or service was needed this phase** — auth already existed.

### Auth gate templates

`wwwroot/main.html` gained two templates:

- `Profile` — "Your profile" title, "Signed in as **${Username}**", a form with
  `${Handle}` (input) and `${Bio}` (textarea) bound fields, a Save submit
  (`onsubmit="${Save}"`), and a Sign out button (`onclick="${SignOut}"`).
- `ProfileSignedOut` — "Profile" title, "Sign in to view and edit your
  profile.", and a "Sign in" link (`href="/account"`).

### Feature-owned page

`Pages/Profile.fs` — the feature-owned, auth-gated page:

```fsharp
module Profile =
    type Model = { handle: string; bio: string }
    type Msg =
        | SetHandle of string
        | SetBio of string
        | Save

    let init = { handle = ""; bio = "" }

    let update msg model =
        match msg with
        | SetHandle h -> { model with handle = h }, Cmd.none
        | SetBio b -> { model with bio = b }, Cmd.none
        | Save -> model, Cmd.none

    let view (form: Model) (username: option<string>)
             (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        cond username <| function
        | None ->
            Layout.ProfileSignedOut().Elt()   // auth gate
        | Some name ->
            Layout.Profile().Username(name).Handle(...).Bio(...)
                .Save(...).SignOut(...).Elt()
```

The page owns only the profile-edit draft (transient, carried by the route's
PageModel). Session state (who is signed in) lives on the root
`SharedModel.account`; sign-out is passed in as a callback — a cross-feature
session effect owned by the root/Shared.

### Wiring (same shape as the Account PageModel page)

- `.fsproj` — registered `Pages/Profile.fs` before `App/App.fs`.
- `App/App.fs` `Page` union — added `| [<EndPoint "/profile">] ProfilePage of
  PageModel<Profile.Model>`.
- `App/App.fs` `Message` — added `| ProfileMsg of Profile.Msg`, with an
  `update` case that matches `model.page` on `ProfilePage pm`, runs
  `Profile.update msg pm.Model`, calls `Router.definePageModel pm m`, and lifts
  with `Cmd.map ProfileMsg` (mirroring `AccountMsg`).
- `App/App.fs` router `defaultPageModel` — added `ProfilePage pm ->
  Router.definePageModel pm Profile.init` so a fresh empty draft is supplied on
  route entry (state-lifetime rule).
- `Ui/Layout.fs` — added the "Profile" menu item and the
  `| ProfilePage pm -> Profile.view pm.Model model.shared.account ...` body
  case. The gate reads `model.shared.account` (the root's session state).

The `/profile` route shares the `PageModel` pattern established for the
Account page, so the profile-edit draft is transient and resets on fresh
navigation.

### MVU verification (browser console trace)

Signed out:

```
New message:: SharedMsg GetSignedInAs        // on app start
New message:: SharedMsg (RecvSignedInAs ...) // 401 -> None -> gate renders
```

Navigate to `/profile` → renders "Sign in to view and edit your profile." with
a Sign in link.

Sign in (on `/account`, username + password, press Enter to submit):

```
New message:: SharedMsg (SendSignIn ("alice", "password"))
New message:: SharedMsg (RecvSignIn (Some "alice"))
New message:: AccountMsg Clear               // root clears the form on success
```

Navigate to `/profile`: renders the editor — "Signed in as **alice**", Handle +
Bio fields, Save + Sign out buttons.

Full page refresh on `/profile`: session cookie persists, so the editor still
renders (still signed in as alice). A logged-out refresh stays on the gate.

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/profile # 200 (SPA route)
```

Browser: `/profile` gated when signed out, editor when signed in, session
survives refresh. `verify.sh` reports `VERIFY OK`.

### Files changed

```
src/Community.Web.Client/Pages/Profile.fs        (new)
src/Community.Web.Client/wwwroot/main.html        (+ Profile, ProfileSignedOut)
src/Community.Web.Client/App/App.fs               (+ ProfilePage, ProfileMsg)
src/Community.Web.Client/Ui/Layout.fs             (+ Profile menu + body case)
src/Community.Web.Client/Community.Web.Client.fsproj
docs/implementation/progress.yaml                 (phase-13: done)
docs/implementation/index.md                     (regenerated)
docs/implementation/_runbook/phase-13-authentication.md  (this file)
```

</details>

- ⬜ `not-started` — [Phase 14 — Cross-feature effects](../reference-design/03-step-by-step-implementation/phase-14-cross-feature-effects/index.md)
- ⬜ `not-started` — [Phase 15 — Rendering optimization](../reference-design/03-step-by-step-implementation/phase-15-rendering-optimization/index.md)
- ⬜ `not-started` — [Phase 16 — Testing ownership boundaries](../reference-design/03-step-by-step-implementation/phase-16-testing-ownership-boundaries/index.md)
- ⬜ `not-started` — [Phase 17 — Design system](../reference-design/03-step-by-step-implementation/phase-17-design-system/index.md)
- ⬜ `not-started` — [Phase 18 — Production hardening](../reference-design/03-step-by-step-implementation/phase-18-production-hardening/index.md)
- ⬜ `not-started` — [Phase 19 — Rollout order](../reference-design/03-step-by-step-implementation/phase-19-rollout-order/index.md)

<!-- END_GENERATED_IMPLEMENTATION -->