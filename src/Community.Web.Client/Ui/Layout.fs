module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
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
/// `icon` is a Radzen Material icon name — the audit's icon+label drawer row
/// (#10): each destination gets a scannable glyph, not a bare text line.
let drawerItem (page: Page) (text: string) (icon: string) =
    RadzenUI.panelMenuItem text (router.Link page) (page = Home) (Some icon)

/// The site navigation, defined ONCE and rendered by both the desktop
/// horizontal menu and the mobile drawer. A `Leaf` is a plain page link; a
/// `Group` is an expandable flyout (desktop submenu / drawer panel group)
/// holding its own leaves. Adding or removing a page touches only this list —
/// the two navigation surfaces can never drift apart.
type NavItem =
    | Leaf of Page * string * string          // page, label, icon
    | Group of string * string * (Page * string * string) list  // label, icon, leaves

let navItems: NavItem list =
    [
        Leaf (Home, "Home", "home")
        Leaf (Games, "Games", "sports_esports")
        Leaf (Servers, "Servers", "dns")
        Leaf (Tournaments, "Tournaments", "emoji_events")
        Group ("Community", "groups", [
            (MembersPage Router.noModel, "Members", "person")
            (Teams, "Teams", "shield")
            (InboxPage Router.noModel, "Notifications", "notifications")
        ])
        Leaf (About, "About", "info")
        Leaf (AccountPage Router.noModel, "Account", "account_circle")
    ]

/// Render a nav item list as the desktop horizontal menu.
let private menuFrom (items: NavItem list) =
    concat {
        for item in items do
            match item with
            | Leaf (page, label, _) -> navItem page label
            | Group (group, _, leaves) ->
                RadzenUI.menuSubmenu group (concat {
                    for (page, label, _) in leaves do
                        navItem page label
                })
    }

/// Render the same nav item list as the mobile drawer panel menu.
let private drawerFrom (items: NavItem list) =
    concat {
        for item in items do
            match item with
            | Leaf (page, label, icon) -> drawerItem page label icon
            | Group (groupLabel, icon, leaves) ->
                RadzenUI.panelMenuItemExpandable groupLabel (Some icon) (concat {
                    for (page, label, leafIcon) in leaves do
                        drawerItem page label leafIcon
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
        // Skip-to-content (42-audit #4): keyboard a11y baseline.
        // Skip-to-content (42-audit #4): keyboard a11y baseline.
        let skipClass =
            "sr-only focus:not-sr-only focus:absolute focus:z-[9999] focus:top-2 focus:left-2 "
            + "focus:bg-[var(--rz-base-background-color)] focus:px-3 focus:py-2"
        a {
            attr.href "#main"
            attr.``class`` skipClass
            "Skip to content"
        }
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
        // Native responsive sidebar (42-switches #1): RadzenSidebar with
        // Responsive=true hides/overlays itself below 768px via matchMedia —
        // the old hand-rolled fixed/-translate-x-full/md:hidden Tailwind
        // drawer and its CSS counterpart are gone. The wrapper div remains
        // only as a surface host (bg + shadow + z-index), rendered
        // conditionally by Radzen's own responsive behavior.
        // CRITICAL: the wrapper keeps `md:hidden` (Tailwind = display:none
        // ≥768px). RadzenSidebar's own responsive hide CANNOT be trusted for
        // desktop because its RadzenMediaQuery.OnChange fires ON SUBSCRIBE
        // (RadzenMediaQuery.cs OnAfterRenderAsync → Change.InvokeAsync
        // (matches)) and RadzenSidebar.OnChange force-writes
        // `expanded = !matches` + invokes ExpandedChanged — so on desktop it
        // overwrites our initial sidebarOpen=false with TRUE. Desktop hiding
        // therefore stays OURS (display:none); mobile show/hide uses the
        // Radzen expanded/collapsed classes (collapsed = width:0 !important).
        // Bug fixes (user report 2026-08-30):
        // 1. COLLAPSED BLACK BAR: the wrapper kept `fixed inset-y-0` + a
        //    background of its own, so when the Radzen sidebar inside
        //    collapsed to width:0, the WRAPPER still painted a 300px black
        //    column over the page. The wrapper is now collapsed too: it
        //    shrinks with the sidebar (w-0 when closed) and has NO
        //    background/border of its own — the surface is painted by the
        //    sidebar itself.
        // 2. EXPANDED MENU STARTS UNDER THE HEADER: the wrapper was
        //    `inset-y-0` (full height from the very top) so the sidebar and
        //    its items slid under the striped header. The drawer now starts
        //    BELOW the header (top-[var(--rz-header-height,56px)]) and the
        //    brand/close row was removed — the header itself is the anchor.
        //    `hidden md:hidden` keeps it off desktop entirely.
        let drawerBase =
            "mobile-nav-drawer fixed left-0 top-[var(--app-header-height,111px)] bottom-0 z-[var(--rz-sidebar-z)] "
            + (if model.sidebarOpen then "w-[var(--rz-sidebar-width)] " else "w-0 ")
            + "overflow-hidden transition-[width] "
            + "md:hidden"
        div {
            attr.``class`` drawerBase
            // The drawer starts BELOW the sticky header: `top` is
            // --app-header-height, published by a tiny script in Index.fs
            // that keeps it in sync with the live .rz-header height.
            RadzenUI.sidebarExpanded model.sidebarOpen
                (fun open' -> dispatch (SetSidebarOpen open'))
                (concat {
                    // No drawer-internal header: the app header (with its
                    // hamburger acting as the close toggle) stays visible and
                    // anchors the drawer below it. Nav items start directly.
                    RadzenUI.panelMenu (drawerFrom navItems)
                })
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
                             + "hover:scale-[1.08] hover:-rotate-3 "
                             + "hover:drop-shadow-[0_0_10px_color-mix(in_srgb,var(--rz-primary)_90%,transparent)] "
                             + "active:scale-100 active:duration-75 motion-reduce:transition-none")
                    }
                }
                // Desktop horizontal menu (hidden on mobile — the drawer
                // replaces it).
                RadzenUI.menu false (menuFrom navItems)
            })
            // Notification bell + INBOX POPUP (user request 2026-08-30):
            // the bell toggles a RadzenPopup anchored top-right holding the
            // latest news as inbox items. Badge = unread inbox count. Open/
            // close state lives in the MVU model (inboxOpen); the popup's
            // CloseOnClickOutside mirrors back via InboxMsg CloseInbox.
            div {
                attr.``class`` "relative inline-flex"
                // Larger, labeled bell (user request 2026-08-30). Size +
                // breathing room from the navbar edge live in CSS
                // (.app-bell-button); see index.css.
                RadzenUI.iconButtonLg "notifications" "Notifications"
                    (fun () -> dispatch (SetInboxOpen (not model.inboxOpen)))
                let unread =
                    match model.shared.news with
                    | Loaded ns ->
                        ns.Keys |> Seq.filter (model.shared.readNews.Contains >> not) |> Seq.length
                    | _ -> 0
                if unread > 0 then
                    div {
                        // pointer-events-none: the badge must never eat the
                        // bell's clicks (it overlaps the button corner).
                        attr.``class`` "absolute -top-1 -right-1 pointer-events-none"
                        RadzenUI.badgePill RadzenUI.dangerBadge (string unread)
                    }
                // The inbox popup. RadzenPopup's open/close is driven by
                // ToggleAsync(target) from JS (verified RadzenPopup.razor.cs
                // — `Visible` only gates rendering, it does NOT run the
                // open/close JS), so a Visible-driven popup would never
                // close. Strict-MVU equivalent: render the panel only when
                // `inboxOpen`, with a transparent full-screen backdrop that
                // closes on click (the popup-doc CloseOnClickOutside
                // behavior, no interop). The panel itself is Tailwind-styled.
                if model.inboxOpen then
                    concat {
                        button {
                            attr.``class`` "app-inbox-backdrop fixed inset-0"
                            attr.aria "label" "Close notifications"
                            on.click (fun (_: Microsoft.AspNetCore.Components.Web.MouseEventArgs) ->
                                dispatch (SetInboxOpen false))
                        }
                        // Panel geometry: below the live header, hugging the
                        // right edge, capped height, theme-token surface.
                        // `top`/`right` are anchored in index.css off the
                        // LIVE --app-header-height (ResizeObserver keeps it in
                        // sync) — do not add Tailwind top utilities here.
                        let panelClass =
                            "app-inbox-popup fixed right-2 left-auto "
                            + "w-[min(24rem,calc(100vw-1rem))] max-h-[min(60vh,30rem)] overflow-auto "
                            + "bg-[var(--rz-base-background-color)] border border-[var(--rz-border-color)] "
                            + "shadow-[var(--rz-shadow-9)] p-[0.75rem] z-[90] animate-pop"
                        div {
                            attr.``class`` panelClass
                            Inbox.popupContent model.shared.news model.shared.readNews
                                (fun id -> dispatch (SharedMsg (Shared.MarkNewsRead id)))
                                (fun () -> dispatch (SharedMsg Shared.MarkAllNewsRead))
                                (fun () -> dispatch (SharedMsg (Shared.Load Shared.News)))
                                (fun () -> dispatch (SetInboxOpen false))
                        }
                    }
                else empty ()
            }
            // A profile menu appears in the header when signed in.
            cond model.shared.account <| function
            | None -> empty()
            | Some name ->
                // Avatar trigger (audit #31): initials avatar via the
                // ProfileMenu <Template> slot instead of a text trigger.
                RadzenUI.profileMenu
                    (RadzenUI.hStackGap "0.5rem" (concat {
                        RadzenUI.initialsAvatar name
                        RadzenUI.text RadzenUI.subtitle1 name
                    }))
                    (fun action ->
                        if action = "signout" then
                            dispatch (SharedMsg Shared.SendSignOut))
                    (concat {
                        RadzenUI.profileMenuItem "Sign out" "signout"
                    })
        })
        // Global loading hairline (42-audit #6): a 2px indeterminate bar
        // under the header while any shared cache is loading.
        let is (rd: RemoteData<_>) =
            match rd with
            | RemoteData.Loading -> true
            | _ -> false
        let anyLoading =
            is model.shared.games || is model.shared.servers || is model.shared.tournaments
            || is model.shared.news || is model.shared.players || is model.shared.teams
        let loadingBarNode =
            if anyLoading then RadzenUI.loadingBar else empty ()
        loadingBarNode
        RadzenUI.body (concat {
            // Fade/slide the active page in on navigation (Tailwind
            // `animate-fade-in` — see the `--animate-fade-in` token in Index.fs).
            // `motion-reduce:animate-none` disables the entrance for users who
            // prefer reduced motion (accessibility best practice).
            div {
                attr.``class`` "animate-fade-in motion-reduce:animate-none"
                cond model.page <| function
                | Home -> Home.view model.shared
                | Games ->
                    Games.view model.gamesGenre model.gamesSearch model.gamesSort
                        (fun g -> dispatch (SetGameGenre g))
                        (fun q -> dispatch (SetGameSearch q))
                        (fun k -> dispatch (SetGameSort k))
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.Games)))
                        model.shared
                        (fun msg -> dispatch (GamesMsg msg))
                | Servers ->
                    Servers.view model.serversSelected
                        (fun gid -> dispatch (SelectServerGame gid))
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.Servers)))
                        (fun sid -> dispatch (SelectServerDetail sid))
                        model.shared
                | Tournaments ->
                    Tournaments.view
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.Tournaments)))
                        model.shared (fun msg -> dispatch (TournamentsMsg msg))
                | MembersPage pm ->
                    Members.view
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.Players)))
                        (fun pid -> dispatch (MemberDetail pid))
                        pm.Model model.shared (fun msg -> dispatch (MembersMsg msg))
                | InboxPage pm ->
                    // Pass pm.Model (NOT pm.Model.search): the router may hand
                    // a null Model to the view (SSR/trim); Inbox.view guards.
                    Inbox.view model.shared.news model.shared.readNews
                        (fun id -> dispatch (SharedMsg (Shared.MarkNewsRead id)))
                        (fun () -> dispatch (SharedMsg Shared.MarkAllNewsRead))
                        pm.Model
                        (fun q -> dispatch (InboxMsg (Inbox.SetSearch q)))
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.News)))
                | Teams ->
                    Teams.view
                        (fun () -> dispatch (SharedMsg (Shared.Load Shared.Teams)))
                        model.shared
                | About -> About.view ()
                | NotFound ->
                    // 404 (42-audit #39): branded dead-end with a way back.
                    RadzenUI.cardOutlinedClass "w-full max-w-md mx-auto text-center p-[var(--pad-page)]"
                        (RadzenUI.vStackGap "1rem" (concat {
                            RadzenUI.icon "error_outline"
                            RadzenUI.text RadzenUI.display3 "404"
                            RadzenUI.text RadzenUI.body1 "That page doesn't exist (or was misplaced by a respawn)."
                            RadzenUI.link "/" (RadzenUI.buttonAction "Back home" RadzenUI.primaryButton (fun () -> ()))
                        }))
                | AccountPage pm ->
                    Account.view pm.Model model.shared.account model.shared.signInFailed
                        model.shared.profileSaving
                        model.shared.profileSaved model.shared.profileError
                        (fun msg -> dispatch (AccountMsg msg))
                        (fun () -> dispatch (SharedMsg Shared.SendSignOut))
            }
        })
        RadzenUI.footer (concat {
            // Real footer content (42-audit #5): brand line + a source-code
            // link to the actual GitHub repo. (The Games/Tournaments/About
            // links were removed — user request 2026-08-30: the navbar
            // already covers navigation.)
            RadzenUI.hStackGapAlign "1rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                RadzenUI.text RadzenUI.caption "42WASD community hub — built by the community."
                RadzenUI.hStackGap "0.75rem" (concat {
                    RadzenUI.link "https://github.com/42WASD/42wasd-community-web" (RadzenUI.icon "code")
                })
            })
            cond model.shared.error <| function
            | None -> empty()
            | Some err ->
                RadzenUI.alertDismissible RadzenUI.dangerAlert err
                    (fun () -> dispatch (SharedMsg Shared.ClearError))
        })
    })