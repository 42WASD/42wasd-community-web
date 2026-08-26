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
/// with a `Header` holding the brand + horizontal `RadzenMenu` nav, a `body`
/// carrying the active page, and a `footer` with the shared error alert.
///
/// Responsive design comes from the Radzen primitives themselves: the
/// horizontal menu collapses to a hamburger below its breakpoint, and page
/// content uses the Radzen 12-col grid. No Bulma templates remain.

/// A horizontal top-nav item. `matchAll` selects the exact route (Home/root);
/// all other pages use Prefix-match so nested routes stay highlighted.
let navItem (page: Page) (text: string) =
    RadzenUI.menuItem text (Some (router.Link page)) (page = Home)

/// The single root view. Only the layout shell lives here; each page renders
/// itself via its owning feature module. The shared error notification is
/// cross-feature UI and stays here.
///
/// Navigation is a horizontal `RadzenMenu` in the header (hover-to-open for
/// any future submenus) — the responsive sidebar is gone, replaced by a clean
/// top nav bar per the Nav/Layout cleanup.
let view (model: Model) (dispatch: Message -> unit) =
    RadzenUI.layout (concat {
        // Host for the imperative Radzen services (Dialog/Notification/
        // Tooltip). Without this, NotificationService.Notify etc. are dropped.
        RadzenUI.components
        RadzenUI.header (concat {
            RadzenUI.hStackGap "0.75rem" (concat {
                // Brand lockup: the 42WASD logo + wordmark, linking to Home.
                a {
                    attr.href (router.Link Home)
                    attr.``class`` "brand"
                    img {
                        attr.src "wasd-icon.png"
                        attr.alt "42WASD"
                    }
                    RadzenUI.text RadzenUI.heading4 "42WASD"
                }
                RadzenUI.menu false (concat {
                    navItem Home "Home"
                    navItem Games "Games"
                    navItem Servers "Servers"
                    navItem Tournaments "Tournaments"
                    // Community is a flyout submenu holding Members + Teams.
                    RadzenUI.menuSubmenu "Community" (concat {
                        navItem (MembersPage Router.noModel) "Members"
                        navItem Teams "Teams"
                    })
                    navItem About "About"
                    navItem (AccountPage Router.noModel) "Account"
                })
            })
            // A profile menu appears in the header when signed in.
            cond model.shared.account <| function
            | None -> empty()
            | Some name ->
                RadzenUI.profileMenu
                    (RadzenUI.text RadzenUI.subtitle1 name)
                    (fun action ->
                        if action = "signout" then
                            dispatch (SharedMsg Shared.SendSignOut))
                    (concat {
                        RadzenUI.profileMenuItem "Sign out" "signout"
                    })
        })
        RadzenUI.body (concat {
            // Fade/slide the active page in on navigation (see index.css).
            div {
                attr.``class`` "fade-in"
                cond model.page <| function
                | Home -> Home.view model.shared
                | Games -> Games.view model.shared (fun msg -> dispatch (GamesMsg msg))
                | Servers -> Servers.view model.shared
                | Tournaments ->
                    Tournaments.view model.shared (fun msg -> dispatch (TournamentsMsg msg))
                | MembersPage pm ->
                    Members.view pm.Model model.shared (fun msg -> dispatch (MembersMsg msg))
                | Teams -> Teams.view model.shared
                | About -> About.view ()
                | AccountPage pm ->
                    Account.view pm.Model model.shared.account model.shared.signInFailed
                        (fun msg -> dispatch (AccountMsg msg))
                        (fun () -> dispatch (SharedMsg Shared.SendSignOut))
            }
        })
        RadzenUI.footer (concat {
            cond model.shared.error <| function
            | None -> empty()
            | Some err -> RadzenUI.alert RadzenUI.dangerAlert err
        })
    })