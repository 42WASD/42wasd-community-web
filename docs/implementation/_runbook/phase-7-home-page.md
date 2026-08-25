---
phase: 03-step-by-step-implementation/phase-7-home-page
---

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