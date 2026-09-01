namespace Community.Client.Tests

open System
open Community.Web.Client.App
open Community.Web.Client.State
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting

/// Test fixtures for the ownership-boundary tests. The stub Api never resolves
/// (it throws if invoked); the pure Shared/App update functions are tested by
/// feeding response-bearing messages (Got*, Toggle*, Recv*) directly, so the
/// remote is never called. This keeps the tests deterministic and DOM-free.
module TestData =

    /// A CommunityApi that throws if any remote call is actually made. Request
    /// messages (Get*, Send*) only emit cmds (ignored in these pure tests);
    /// response-bearing messages are fed directly to the reducer.
    let stubApi : CommunityApi =
        {
            getGames = fun () -> async { return failwith "not used in pure tests" }
            getServers = fun () -> async { return failwith "not used in pure tests" }
            getTournaments = fun () -> async { return failwith "not used in pure tests" }
            getNews = fun () -> async { return failwith "not used in pure tests" }
            getPlayers = fun () -> async { return failwith "not used in pure tests" }
            getTeams = fun () -> async { return failwith "not used in pure tests" }
            signIn = fun _ -> async { return failwith "not used in pure tests" }
            getUsername = fun () -> async { return failwith "not used in pure tests" }
            saveProfile = fun _ -> async { return failwith "not used in pure tests" }
            setTournamentRegistration = fun _ -> async { return failwith "not used in pure tests" }
            setFavoriteGames = fun _ -> async { return failwith "not used in pure tests" }
            setReadNews = fun _ -> async { return failwith "not used in pure tests" }
            signOut = fun () -> async { return failwith "not used in pure tests" }
        }

    /// Two sample games keyed by id, used to assert cache normalization.
    let sampleGames : Game[] =
        [|
            { id = "game-1"; name = "Counter-Strike 2"; genre = "FPS"; description = "Tactical shooter"; imageUrl = "https://img/game1.jpg" }
            { id = "game-2"; name = "Dota 2"; genre = "MOBA"; description = "5v5 arena"; imageUrl = "https://img/game2.jpg" }
        |]

    /// Sample game servers covering every status for the Servers tabs.
    let sampleServers : GameServer[] =
        [|
            { id = "s-1"; name = "CS2 Competitive #1"; gameId = "game-1"; address = "10.0.0.1:27015"; onlinePlayers = 7; maxPlayers = 12; status = "online" }
            { id = "s-2"; name = "Dota 2 Lobby"; gameId = "game-2"; address = "10.0.0.2:27016"; onlinePlayers = 3; maxPlayers = 10; status = "online" }
            { id = "s-3"; name = "CS2 Practice"; gameId = "game-1"; address = "10.0.0.3:27017"; onlinePlayers = 0; maxPlayers = 8; status = "offline" }
        |]

    /// Two tournaments: one open, one closed — used to assert the cross-
    /// feature toggle (toggling one leaves the other untouched).
    let sampleTournaments : Tournament[] =
        [|
            { id = "t-1"; name = "CS2 Cup"; gameId = "game-1"; startsAt = System.DateTime(2026, 9, 1); prize = "$1k"; registrationOpen = true }
            { id = "t-2"; name = "Dota Invitational"; gameId = "game-2"; startsAt = System.DateTime(2026, 9, 5); prize = "$2k"; registrationOpen = false }
        |]

    /// News posts for the Home page's latest-news section.
    let sampleNews : News[] =
        [|
            { id = "n-1"; title = "Season 3 starts"; body = "New season begins."; publishedAt = System.DateTime(2026, 8, 20) }
            { id = "n-2"; title = "New servers online"; body = "Two CS2 servers added."; publishedAt = System.DateTime(2026, 8, 25) }
        |]

    /// Players for the Members page. bob carries a persisted favourite
    /// (game-1) + read-news (n-9) set to exercise the per-user seeding.
    let samplePlayers : Player[] =
        [|
            { id = "p-1"; username = "alice"; discord = Some "alice#1"; handle = Some "Alice"; bio = Some "FPS player"; favoriteGames = []; readNews = [] }
            { id = "p-2"; username = "bob"; discord = None; handle = Some "Bob"; bio = None; favoriteGames = [ "game-1" ]; readNews = [ "n-9" ] }
        |]

    /// Teams for the Teams page.
    let sampleTeams : Team[] =
        [|
            { id = "team-1"; name = "Alpha Squad"; players = [| samplePlayers.[0] |] }
            { id = "team-2"; name = "Beta Squad"; players = [| samplePlayers.[1] |] }
        |]

    /// Every DataLoaded payload at once — the fully-loaded shared cache used
    /// by the UX probe so conditional rendering is fully resolved (real
    /// cards/tabs instead of skeletons).
    let loadedShared : SharedModel =
        [ Shared.GamesLoaded sampleGames
          Shared.ServersLoaded sampleServers
          Shared.TournamentsLoaded sampleTournaments
          Shared.NewsLoaded sampleNews
          Shared.PlayersLoaded samplePlayers
          Shared.TeamsLoaded sampleTeams ]
        |> List.fold
            (fun shared payload ->
                let m, _ =
                    Shared.update stubApi shared (Shared.DataLoaded payload)
                m)
            SharedModel.init
