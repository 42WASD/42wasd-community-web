namespace Community.Client.Tests

open System
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
            signOut = fun () -> async { return failwith "not used in pure tests" }
        }

    /// Two sample games keyed by id, used to assert cache normalization.
    let sampleGames : Game[] =
        [|
            { id = "game-1"; name = "Counter-Strike 2"; genre = "FPS"; description = "Tactical shooter"; imageUrl = "https://img/game1.jpg" }
            { id = "game-2"; name = "Dota 2"; genre = "MOBA"; description = "5v5 arena"; imageUrl = "https://img/game2.jpg" }
        |]

    /// One open and one closed tournament, used to assert the cross-feature
    /// toggle (Phase 14) — toggling one leaves the other untouched.
    let sampleTournaments : Tournament[] =
        [|
            { id = "t-1"; name = "CS2 Cup"; gameId = "game-1"; startsAt = System.DateTime(2026, 9, 1); prize = "$1k"; registrationOpen = true }
            { id = "t-2"; name = "Dota Invitational"; gameId = "game-2"; startsAt = System.DateTime(2026, 9, 5); prize = "$2k"; registrationOpen = false }
        |]
