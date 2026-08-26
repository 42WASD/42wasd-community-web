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
    let ``GotGames normalizes the array into an id-keyed map`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.GotGames sampleGames)
        match shared.games with
        | Loaded m ->
            Assert.Equal(2, m.Count)
            Assert.Equal("Counter-Strike 2", m["game-1"].name)
            Assert.Equal("Dota 2", m["game-2"].name)
        | _ -> Assert.True(false, "Expected Loaded games")

    [<Fact>]
    let ``GotTournament preserves registrationOpen from the payload`` () =
        let shared, _ = Shared.update stubApi SharedModel.init (Shared.GotTournaments sampleTournaments)
        match shared.tournaments with
        | Loaded m ->
            Assert.True(m["t-1"].registrationOpen)
            Assert.False(m["t-2"].registrationOpen)
        | _ -> Assert.True(false, "Expected Loaded tournaments")

    [<Fact>]
    let ``ToggleTournament flips registrationOpen in the canonical cache`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.GotTournaments sampleTournaments)
        let after, _ = Shared.update stubApi loaded (Shared.ToggleTournament "t-1")
        match after.tournaments with
        | Loaded m ->
            Assert.False(m["t-1"].registrationOpen)
            Assert.False(m["t-2"].registrationOpen)
        | _ -> Assert.True(false, "Expected Loaded tournaments")

    [<Fact>]
    let ``ToggleTournament unknown id leaves the cache unchanged`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.GotTournaments sampleTournaments)
        let after, _ = Shared.update stubApi loaded (Shared.ToggleTournament "does-not-exist")
        Assert.Equal(loaded.tournaments, after.tournaments)

    [<Fact>]
    let ``ToggleFavoriteGame adds then removes the id in the set`` () =
        let added, _ = Shared.update stubApi SharedModel.init (Shared.ToggleFavoriteGame "game-1")
        Assert.Contains("game-1", added.favoriteGames)
        let removed, _ = Shared.update stubApi added (Shared.ToggleFavoriteGame "game-1")
        Assert.DoesNotContain("game-1", removed.favoriteGames)

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
    let ``GetGames sets only the games cache to Loading`` () =
        let shared, _ = Shared.update stubApi SharedModel.init Shared.GetGames
        Assert.Equal(Loading, shared.games)
        Assert.Equal(NotAsked, shared.servers)
        Assert.Equal(NotAsked, shared.tournaments)

    [<Fact>]
    let ``ClearError resets the error field`` () =
        let withError = { SharedModel.init with error = Some "boom" }
        let cleared, _ = Shared.update stubApi withError Shared.ClearError
        Assert.Equal(None, cleared.error)

    [<Fact>]
    let ``RecvSaveProfile sets the profileSaved flag`` () =
        let after, _ = Shared.update stubApi SharedModel.init Shared.RecvSaveProfile
        Assert.True(after.profileSaved)
