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
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.GamesLoaded sampleGames))
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
        let loaded, _ = Shared.update stubApi SharedModel.init (Shared.DataLoaded (Shared.TournamentsLoaded sampleTournaments))
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
    let ``AccountMsg Login translates to SendSignIn with the submitted credentials`` () =
        let pm : Bolero.PageModel<Account.Model> = { Model = Account.init }
        let m = { initModel with page = Page.AccountPage pm }
        // A single Login intent carries both fields and preserves the page.
        let after, _ = update stubApi (AccountMsg (Account.Login ("alice", "password"))) m
        Assert.Equal(m.page, after.page)

    [<Fact>]
    let ``AccountMsg SaveProfile translates to a shared profile save, not a sign-in`` () =
        // Regression: the profile editor's Save button used to share the
        // `Submit`/sign-in message, which the root translated into SendSignIn
        // with EMPTY credentials — logging the user out. `SaveProfile` must be
        // translated into a Shared.SaveProfile effect, never a sign-in, and it
        // must not clear the session or the drafts.
        let pm : Bolero.PageModel<Account.Model> = { Model = { Account.init with handle = "bob_the_gamer"; bio = "FPS" } }
        let m = { initModel with page = Page.AccountPage pm }
        let after, _ = update stubApi (AccountMsg Account.SaveProfile) m
        // The page + its transient drafts are preserved.
        match after.page with
        | Page.AccountPage pm' ->
            Assert.Equal("bob_the_gamer", pm'.Model.handle)
            Assert.Equal("FPS", pm'.Model.bio)
        | _ -> Assert.True(false, "Expected AccountPage")
        // The shared account is untouched (still signed out / no session change).
        Assert.Equal(initModel.shared.account, after.shared.account)

    // --- Tournaments split-button action routing (regression) ---------------
    // RadzenSplitButtonItem has NO per-item click handler — item clicks bubble
    // to the parent's `Click` with the item's Value. We route on that Value so
    // "View details" must NEVER toggle registration. These prove the routing.

    [<Fact>]
    let ``split action "details" maps to ViewDetails, not a toggle`` () =
        match Tournaments.actionMsg "t-1" (Some "details") with
        | Tournaments.ViewDetails id -> Assert.Equal("t-1", id)
        | _ -> Assert.True(false, "Expected ViewDetails")

    [<Fact>]
    let ``split action main button and toggle item both toggle`` () =
        match Tournaments.actionMsg "t-1" None with
        | Tournaments.ToggleRegistration "t-1" -> ()
        | _ -> Assert.True(false, "main button should toggle")
        match Tournaments.actionMsg "t-1" (Some "toggle") with
        | Tournaments.ToggleRegistration "t-1" -> ()
        | _ -> Assert.True(false, "toggle item should toggle")

    [<Fact>]
    let ``split action unknown value safely toggles, never crashes`` () =
        match Tournaments.actionMsg "t-1" (Some "unexpected") with
        | Tournaments.ToggleRegistration "t-1" -> ()
        | _ -> Assert.True(false, "unknown value should fall back to toggle")
