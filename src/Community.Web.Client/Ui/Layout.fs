module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Community.Web.Client.App
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// The single shared layout template (`Layout`) and the `dataRows` helper live
/// in `Ui/Templates.fs`, which compiles early so feature pages can reuse them.
/// Per the reference design, the global `Ui/` folder holds ONLY cross-feature
/// UI: the layout shell, the menu, and the shared error notification. Every
/// page-specific view lives beside its owning feature under `Pages/`.
let menuItem (model: Model) (page: Page) (text: string) =
    Layout.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

/// The single root view. Only the layout shell lives here; each page renders
/// itself via its owning feature module. The shared error notification is
/// cross-feature UI and stays here.
let view (model: Model) (dispatch: Message -> unit) =
    Layout()
        .Menu(concat {
            menuItem model Home "Home"
            menuItem model Games "Games"
            menuItem model Servers "Servers"
            menuItem model Tournaments "Tournaments"
            menuItem model Members "Members"
            menuItem model Teams "Teams"
            menuItem model About "About"
            menuItem model (AccountPage Router.noModel) "Account"
        })
        .Body(
            cond model.page <| function
            | Home -> Home.view model.shared
            | Games -> Games.view model.shared
            | Servers -> Servers.view model.shared
            | Tournaments -> Tournaments.view model.shared
            | Members -> Members.view model.shared
            | Teams -> Teams.view model.shared
            | About -> About.view ()
            | AccountPage pm ->
                Account.view pm.Model model.shared.account model.shared.signInFailed
                    (fun msg -> dispatch (AccountMsg msg))
                    (fun () -> dispatch (SharedMsg Shared.SendSignOut))
        )
        .Error(
            cond model.shared.error <| function
            | None -> empty()
            | Some err ->
                Layout.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch (SharedMsg Shared.ClearError))
                    .Elt()
        )
        .Elt()