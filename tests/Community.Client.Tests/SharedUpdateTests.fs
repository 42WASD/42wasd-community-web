namespace Community.Client.Tests

open Xunit
open Community.Web.Client.State
open Community.Web.Client.App
open TestData

/// Tests for Shared.update — the pure shared state layer. These prove the
/// ownership-boundary core rules: entities are normalized into id-keyed maps,
/// and cross-feature toggles mutate one canonical source (never reaching into
/// another owner's state).
module SharedUpdateTests =

    [<Fact>]
    let ``Loaded (GamesLoaded) normalizes the array into an id-keyed map`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.GamesLoaded sampleGames))
        match shared.games with
        | Loaded m ->
            Assert.Equal(2, m.Count)
            Assert.Equal("Counter-Strike 2", m["game-1"].name)
            Assert.Equal("Dota 2", m["game-2"].name)
        | _ -> Assert.True(false, "Expected Loaded games")

    [<Fact>]
    let ``Loaded (TournamentsLoaded) preserves registrationOpen from the payload`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.TournamentsLoaded sampleTournaments))
        match shared.tournaments with
        | Loaded m ->
            Assert.True(m["t-1"].registrationOpen)
            Assert.False(m["t-2"].registrationOpen)
        | _ -> Assert.True(false, "Expected Loaded tournaments")

    [<Fact>]
    let ``ToggleTournament flips registrationOpen in the canonical cache`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.TournamentsLoaded sampleTournaments))
        let after, _ = Shared.update stubApi loaded (Shared.ToggleTournament "t-1")
        match after.tournaments with
        | Loaded m ->
            Assert.False(m["t-1"].registrationOpen)
            Assert.False(m["t-2"].registrationOpen)
        | _ -> Assert.True(false, "Expected Loaded tournaments")

    [<Fact>]
    let ``ToggleTournament unknown id leaves the cache unchanged`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.TournamentsLoaded sampleTournaments))
        let after, _ = Shared.update stubApi loaded (Shared.ToggleTournament "does-not-exist")
        Assert.Equal(loaded.tournaments, after.tournaments)

    [<Fact>]
    let ``ToggleFavoriteGame adds then removes the id in the set`` () =
        let added, _ = Shared.update stubApi SharedModel.init (Shared.ToggleFavoriteGame "game-1")
        Assert.Contains("game-1", added.favoriteGames)
        let removed, _ = Shared.update stubApi added (Shared.ToggleFavoriteGame "game-1")
        Assert.DoesNotContain("game-1", removed.favoriteGames)

    // --- persisted shared effects (DTO-backed, 2026-09-01) -----------------

    [<Fact>]
    let ``MarkNewsRead adds the id to the read set`` () =
        let after, _ = Shared.update stubApi SharedModel.init (Shared.MarkNewsRead "n-1")
        Assert.Contains("n-1", after.readNews)
        // Idempotent: marking again keeps the set the same.
        let again, _ = Shared.update stubApi after (Shared.MarkNewsRead "n-1")
        Assert.Equal<Set<string>>(after.readNews, again.readNews)

    [<Fact>]
    let ``MarkAllNewsRead marks every loaded news id read`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.NewsLoaded sampleNews))
        let after, _ = Shared.update stubApi loaded Shared.MarkAllNewsRead
        match after.news with
        | Loaded ns -> Assert.Equal<Set<string>>(Set.ofSeq ns.Keys, after.readNews)
        | _ -> Assert.True(false, "Expected Loaded news")

    [<Fact>]
    let ``PlayersLoaded seeds favoriteGames and readNews from the signed-in player record`` () =
        let signedIn, _ = Shared.update stubApi SharedModel.init (Shared.RecvSignIn (Some "bob"))
        let loaded, _ = Shared.update stubApi signedIn (Shared.DataLoaded (Shared.PlayersLoaded samplePlayers))
        // bob's persisted record carries favourite game-1 + read n-9.
        Assert.Equal<Set<string>>(Set.ofList [ "game-1" ], loaded.favoriteGames)
        Assert.Equal<Set<string>>(Set.ofList [ "n-9" ], loaded.readNews)

    [<Fact>]
    let ``PlayersLoaded with unknown account leaves the per-user sets empty`` () =
        let signedIn, _ = Shared.update stubApi SharedModel.init (Shared.RecvSignIn (Some "ghost"))
        let loaded, _ = Shared.update stubApi signedIn (Shared.DataLoaded (Shared.PlayersLoaded samplePlayers))
        Assert.Empty(loaded.favoriteGames)
        Assert.Empty(loaded.readNews)

    [<Fact>]
    let ``RecvSignIn Some records account and clears signInFailed`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.RecvSignIn (Some "alice"))
        Assert.Equal(Some "alice", shared.account)
        Assert.False(shared.signInFailed)

    [<Fact>]
    let ``RecvSignIn None sets signInFailed and no account`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.RecvSignIn None)
        Assert.Equal(None, shared.account)
        Assert.True(shared.signInFailed)

    [<Fact>]
    let ``RecvSignOut clears account and signInFailed`` () =
        let loggedIn, _ = Shared.update stubApi SharedModel.init (Shared.RecvSignIn (Some "alice"))
        let signedOut, _ = Shared.update stubApi loggedIn Shared.RecvSignOut
        Assert.Equal(None, signedOut.account)
        Assert.False(signedOut.signInFailed)

    [<Fact>]
    let ``Load Games sets only the games cache to Loading`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.Load Shared.Games)
        Assert.Equal(Loading, shared.games)
        Assert.Equal(NotAsked, shared.servers)
        Assert.Equal(NotAsked, shared.tournaments)

    [<Fact>]
    let ``ClearError resets the error field`` () =
        let withError = { SharedModel.init with error = Some "boom" }
        let cleared, _ = Shared.update stubApi withError Shared.ClearError
        Assert.Equal(None, cleared.error)

    [<Fact>]
    let ``RecvSaveProfile true sets the profileSaved flag`` () =
        let after, _ = Shared.update stubApi SharedModel.init (Shared.RecvSaveProfile true)
        Assert.True(after.profileSaved)
        Assert.Equal(None, after.profileError)

    [<Fact>]
    let ``RecvSaveProfile false sets the profileError instead of profileSaved`` () =
        let after, _ = Shared.update stubApi SharedModel.init (Shared.RecvSaveProfile false)
        Assert.False(after.profileSaved)
        Assert.True(after.profileError.IsSome)


