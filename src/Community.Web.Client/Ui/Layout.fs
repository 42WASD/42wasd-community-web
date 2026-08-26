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

/// A nav entry for the mobile drawer (RadzenSidebar + RadzenPanelMenu).
let drawerItem (page: Page) (text: string) =
    RadzenUI.panelMenuItem text (router.Link page) (page = Home)

/// The site navigation, defined ONCE and rendered by both the desktop
/// horizontal menu and the mobile drawer. A `Leaf` is a plain page link; a
/// `Group` is an expandable flyout (desktop submenu / drawer panel group)
/// holding its own leaves. Adding or removing a page touches only this list —
/// the two navigation surfaces can never drift apart.
type NavItem =
    | Leaf of Page * string
    | Group of string * (Page * string) list

let navItems: NavItem list =
    [
        Leaf (Home, "Home")
        Leaf (Games, "Games")
        Leaf (Servers, "Servers")
        Leaf (Tournaments, "Tournaments")
        Group ("Community", [ (MembersPage Router.noModel, "Members"); (Teams, "Teams") ])
        Leaf (About, "About")
        Leaf (AccountPage Router.noModel, "Account")
    ]

/// Render a nav item list as the desktop horizontal menu.
let private menuFrom (items: NavItem list) =
    concat {
        for item in items do
            match item with
            | Leaf (page, label) -> navItem page label
            | Group (group, leaves) ->
                RadzenUI.menuSubmenu group (concat {
                    for (page, label) in leaves do
                        navItem page label
                })
    }

/// Render the same nav item list as the mobile drawer panel menu.
let private drawerFrom (items: NavItem list) =
    concat {
        for item in items do
            match item with
            | Leaf (page, label) -> drawerItem page label
            | Group (groupLabel, leaves) ->
                RadzenUI.panelMenuItemExpandable groupLabel (concat {
                    for (page, label) in leaves do
                        drawerItem page label
                })
    }

/// The single root view. Only the layout shell lives here; each page renders
/// itself via its owning feature module. The shared error notification is
/// cross-feature UI and stays here.
///
/// Navigation: a horizontal `RadzenMenu` in the header for desktop, plus a
/// `RadzenSidebar` drawer (shown only on mobile via CSS) that gives the small-
/// screen nav a proper opaque panel instead of a transparent stretched bar.
let view (model: Model) (dispatch: Message -> unit) =
    RadzenUI.layout (concat {
        // Host for the imperative Radzen services (Dialog/Notification/
        // Tooltip). Without this, NotificationService.Notify etc. are dropped.
        RadzenUI.components
        // Mobile-only drawer. `Expanded` is driven by model.sidebarOpen; it is
        // hidden on desktop via `.mobile-nav-drawer { display:none }` in CSS.
        // `.is-open` is toggled from F# so the wrapper overlay slides in/out
        // in lock-step with the Radzen sidebar state.
        // Mobile drawer. `mobile-nav-drawer` is a hook class so the Radzen
        // internal `.mobile-nav-drawer .rz-sidebar` override in index.css still
        // scopes correctly; the layout/positioning itself is pure Tailwind
        // utilities. Base state slides it off-screen to the left; `.is-open`
        // (F#-driven) slides it in, in lock-step with the Radzen sidebar state.
        let drawerBase =
            "mobile-nav-drawer fixed inset-y-0 left-0 w-[var(--rz-sidebar-width)] "
            + "z-[var(--rz-sidebar-z)] -translate-x-full transition-transform "
            + "bg-[var(--rz-base-background-color)] "
            // Elevation from Radzen's shadow token (no hardcoded rgba) — the
            // largest level so the drawer reads as an overlay above content.
            + "shadow-[var(--rz-shadow-9)] md:hidden"
        div {
            attr.``class`` (if model.sidebarOpen then drawerBase + " translate-x-0" else drawerBase)
            RadzenUI.sidebarExpanded model.sidebarOpen
                (fun open' -> dispatch (SetSidebarOpen open'))
                (RadzenUI.panelMenu (drawerFrom navItems))
        }
        RadzenUI.header (concat {
            RadzenUI.hStackGap "0.75rem" (concat {
                // Mobile-only hamburger that opens the drawer. `md:hidden`
                // shows it below 768px and hides it on larger screens.
                div {
                    attr.``class`` "inline-flex md:hidden"
                    RadzenUI.sidebarToggle (fun () ->
                        dispatch (SetSidebarOpen (not model.sidebarOpen)))
                }
                // Brand lockup: just the 42WASD logo (SVG, higher quality),
                // linking to Home. No wordmark text — the logo is the brand.
                // Pure Tailwind: flex lockup + a 44px logo with a soft cyan
                // glow (from the Radzen --rz-primary token) that brightens and
                // scales on hover.
                a {
                    attr.href (router.Link Home)
                    attr.``class`` "inline-flex items-center gap-2 no-underline"
                    img {
                        attr.src "42wasd.svg"
                        attr.alt "42WASD"
                        attr.title "42WASD"
                        attr.``class``
                            ("block w-11 h-11 object-contain will-change-[transform,filter] "
                             + "drop-shadow-[0_0_6px_color-mix(in_srgb,var(--rz-primary)_55%,transparent)] "
                             + "transition-[transform,filter] duration-300 ease-out "
                             + "hover:scale-[1.08] hover:-rotate-3 hover:drop-shadow-[0_0_10px_color-mix(in_srgb,var(--rz-primary)_90%,transparent)] "
                             + "active:scale-100 active:duration-75 motion-reduce:transition-none")
                    }
                }
                // Desktop horizontal menu (hidden on mobile — the drawer
                // replaces it).
                RadzenUI.menu false (menuFrom navItems)
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
            // Fade/slide the active page in on navigation (Tailwind
            // `animate-fade-in` — see the `--animate-fade-in` token in Index.fs).
            // `motion-reduce:animate-none` disables the entrance for users who
            // prefer reduced motion (accessibility best practice).
            div {
                attr.``class`` "animate-fade-in motion-reduce:animate-none"
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
                        model.shared.profileSaved model.shared.profileError
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