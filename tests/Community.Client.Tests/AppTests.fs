namespace Community.Client.Tests

open Xunit
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Client.App
open TestData

/// Tests for App.update — the root orchestration boundary. These prove that
/// cross-feature effects are *translated* by the root (a shared update is
/// dispatched, never mutated), and page-level changes leave shared state
/// untouched.
module AppTests =

    [<Fact>]
    let ``page change preserves shared state`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.GotGames sampleGames)
        let m = { initModel with shared = loaded }
        let after, _ = update stubApi (SetPage Page.Games) m
        Assert.Equal(Page.Games, after.page)
        Assert.Equal(loaded.games, after.shared.games)

    [<Fact>]
    let ``GamesMsg ToggleFavorite is a translation, not direct mutation`` () =
        let mid, _ = update stubApi (GamesMsg (Games.ToggleFavorite "game-1")) initModel
        Assert.DoesNotContain("game-1", mid.shared.favoriteGames)
        let applied, _ = update stubApi (SharedMsg (Shared.ToggleFavoriteGame "game-1")) mid
        Assert.Contains("game-1", applied.shared.favoriteGames)

    [<Fact>]
    let ``TournamentsMsg Toggle delegates to the shared effect`` () =
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.GotTournaments sampleTournaments)
        let m = { initModel with shared = loaded }
        let mid, _ = update stubApi (TournamentsMsg (Tournaments.ToggleRegistration "t-1")) m
        let after, _ = update stubApi (SharedMsg (Shared.ToggleTournament "t-1")) mid
        match after.shared.tournaments with
        | Loaded x ->
            Assert.False(x["t-1"].registrationOpen, "shared effect flips registration")
            Assert.False(x["t-2"].registrationOpen, "other tournament untouched")
        | _ -> Assert.True(false, "Expected Loaded")

    [<Fact>]
    let ``RecvSignIn Some records the account in the root model`` () =
        let after, _ = update stubApi (SharedMsg (Shared.RecvSignIn (Some "alice"))) initModel
        Assert.Equal(Some "alice", after.shared.account)

    [<Fact>]
    let ``AccountMsg Submit on the AccountPage is translated without losing the page`` () =
        let pm : Bolero.PageModel<Account.Model> = { Model = Account.init }
        let m = { initModel with page = Page.AccountPage pm }
        let after, _ = update stubApi (AccountMsg Account.Submit) m
        Assert.Equal(m.page, after.page)
