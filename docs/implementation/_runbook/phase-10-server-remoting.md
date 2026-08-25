---
phase: 03-step-by-step-implementation/phase-10-server-remoting
---

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