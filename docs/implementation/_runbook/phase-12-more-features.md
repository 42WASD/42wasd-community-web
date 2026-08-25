---
phase: 03-step-by-step-implementation/phase-12-more-features
---

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