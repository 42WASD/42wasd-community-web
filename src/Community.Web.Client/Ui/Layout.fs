module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Community.Web.Client.App
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// The single shared layout shell, rebuilt on Radzen primitives (Phase 17b).
/// This is cross-feature UI and lives only here: a responsive `RadzenLayout`
/// with a `Header` (brand + sidebar toggle), a responsive `sidebar` holding the
/// `RadzenPanelMenu` nav, a `body` carrying the active page, and a `footer`
/// with the shared error alert.
///
/// Responsive design comes from the Radzen primitives themselves: the sidebar
/// auto-collapses below 768px, the toggle flips `model.sidebarExpanded`, and
/// page content uses the Radzen 12-col grid. No Bulma templates remain.
let menuItem (model: Model) (page: Page) (text: string) =
    RadzenUI.panelMenuItem text (router.Link page) (page = Home)

/// The single root view. Only the layout shell lives here; each page renders
/// itself via its owning feature module. The shared error notification is
/// cross-feature UI and stays here.
let view (model: Model) (dispatch: Message -> unit) =
    RadzenUI.layout (concat {
        RadzenUI.header (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.sidebarToggle (fun () -> dispatch ToggleSidebar)
                RadzenUI.text RadzenUI.heading4 "42WASD"
            })
        })
        RadzenUI.sidebarExpanded model.sidebarExpanded (fun _ -> dispatch ToggleSidebar) (concat {
            RadzenUI.panelMenu (concat {
                menuItem model Home "Home"
                menuItem model Games "Games"
                menuItem model Servers "Servers"
                menuItem model Tournaments "Tournaments"
                menuItem model Members "Members"
                menuItem model Teams "Teams"
                menuItem model About "About"
                menuItem model (AccountPage Router.noModel) "Account"
            })
        })
        RadzenUI.body (concat {
            cond model.page <| function
            | Home -> Home.view model.shared
            | Games -> Games.view model.shared (fun msg -> dispatch (GamesMsg msg))
            | Servers -> Servers.view model.shared
            | Tournaments ->
                Tournaments.view model.shared (fun msg -> dispatch (TournamentsMsg msg))
            | Members -> Members.view model.shared
            | Teams -> Teams.view model.shared
            | About -> About.view ()
            | AccountPage pm ->
                Account.view pm.Model model.shared.account model.shared.signInFailed
                    (fun msg -> dispatch (AccountMsg msg))
                    (fun () -> dispatch (SharedMsg Shared.SendSignOut))
        })
        RadzenUI.footer (concat {
            cond model.shared.error <| function
            | None -> empty()
            | Some err -> RadzenUI.alert RadzenUI.dangerAlert err
        })
    })