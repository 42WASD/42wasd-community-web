---
phase: 03-step-by-step-implementation/phase-3-shared-domain-types
---

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