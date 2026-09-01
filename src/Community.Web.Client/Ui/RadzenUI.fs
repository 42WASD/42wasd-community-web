namespace Community.Web.Client.Ui

open System
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Forms
open Microsoft.AspNetCore.Components.Rendering
open Microsoft.AspNetCore.Components.Routing
open Microsoft.AspNetCore.Components.Web
open Radzen
open Radzen.Blazor
open Community.Web.Client.State
open Community.Web.Shared.Domain

/// Thin F# wrappers around Radzen Blazor components, per design.md. Keeping
/// the component interop in ONE module keeps page views declarative and
/// oblivious to Blazor's object-oriented mechanics. Each wrapper takes an
/// Elmish `Msg` (or a value -> Msg) and a dispatch, and maps the Blazor
/// EventCallback to `dispatch`.
///
/// The wrappers are intentionally small and stay on top of the architecture —
/// they never touch state ownership and are **view-only** (they build `Node`s).
/// Radzen services (Dialog/Notification) are registered on the host (Startup.fs)
/// and, when a side effect is needed, the caller emits an Elmish Msg that an
/// update loop turns into an async Cmd (per design.md section 4).
///
/// Responsive design is provided by the Radzen primitives themselves — the
/// RadzenRow/RadzenColumn 12-col grid, RadzenLayout/Sidebar auto-collapse on
/// small screens, and RadzenStack — so no hardcoded pixel layout lives here or
/// in index.css. All wrappers are view-only; none are called from update/init.
///
/// NOTE: The `comp` computation expression supports `"Param" => value`,
/// `attr.*`, and child `Node`s — but NO `yield!`/`if`/`match` control flow
/// inside the body. Each wrapper therefore fixes its attribute set and passes
/// children directly.
module RadzenUI =

    // ---------------------------------------------------------------- enums
    // Re-exported so pages never `open Radzen`. (F# can't make these Literals,
    // so they are module-level lets.)
    let primaryButton = ButtonStyle.Primary
    let secondaryButton = ButtonStyle.Secondary
    let successButton = ButtonStyle.Success
    let dangerButton = ButtonStyle.Danger
    let lightButton = ButtonStyle.Light
    let darkButton = ButtonStyle.Dark

    let filled = Variant.Filled
    let flat = Variant.Flat
    let outlined = Variant.Outlined
    let textVariant = Variant.Text

    let primaryBadge = BadgeStyle.Primary
    let successBadge = BadgeStyle.Success
    let dangerBadge = BadgeStyle.Danger
    let warningBadge = BadgeStyle.Warning
    let infoBadge = BadgeStyle.Info
    let lightBadge = BadgeStyle.Light
    let darkBadge = BadgeStyle.Dark

    let infoAlert = AlertStyle.Info
    let successAlert = AlertStyle.Success
    let warningAlert = AlertStyle.Warning
    let dangerAlert = AlertStyle.Danger

    let navMatchPrefix = NavLinkMatch.Prefix
    let navMatchAll = NavLinkMatch.All

    let progressBarPrimary = ProgressBarStyle.Primary
    let progressBarSuccess = ProgressBarStyle.Success
    let progressBarDanger = ProgressBarStyle.Danger
    let progressBarWarning = ProgressBarStyle.Warning
    let progressBarInfo = ProgressBarStyle.Info
    let progressBarDark = ProgressBarStyle.Dark

    let circularSmall = ProgressBarCircularSize.Small
    let circularMedium = ProgressBarCircularSize.Medium
    let circularLarge = ProgressBarCircularSize.Large

    let pointPrimary = PointStyle.Primary
    let pointSecondary = PointStyle.Secondary
    let pointSuccess = PointStyle.Success
    let pointDanger = PointStyle.Danger
    let pointWarning = PointStyle.Warning
    let pointInfo = PointStyle.Info
    let pointDark = PointStyle.Dark

    let pointSizeExtraSmall = PointSize.ExtraSmall
    let pointSizeSmall = PointSize.Small
    let pointSizeMedium = PointSize.Medium
    let pointSizeLarge = PointSize.Large

    let pagerTop = PagerPosition.Top
    let pagerBottom = PagerPosition.Bottom
    let pagerTopAndBottom = PagerPosition.TopAndBottom

    let horizontal = Orientation.Horizontal
    let vertical = Orientation.Vertical

    let alignStart = AlignItems.Start
    let alignCenter = AlignItems.Center
    let alignEnd = AlignItems.End
    let alignStretch = AlignItems.Stretch
    let justifyStart = JustifyContent.Start
    let justifyCenter = JustifyContent.Center
    let justifyEnd = JustifyContent.End
    let justifyBetween = JustifyContent.SpaceBetween
    let justifyAround = JustifyContent.SpaceAround
    let wrapWrap = FlexWrap.Wrap
    let wrapNoWrap = FlexWrap.NoWrap

    let display1 = TextStyle.DisplayH1
    let display2 = TextStyle.DisplayH2
    let display3 = TextStyle.DisplayH3
    let display4 = TextStyle.DisplayH4
    let display5 = TextStyle.DisplayH5
    let display6 = TextStyle.DisplayH6
    let heading1 = TextStyle.H1
    let heading2 = TextStyle.H2
    let heading3 = TextStyle.H3
    let heading4 = TextStyle.H4
    let heading5 = TextStyle.H5
    let heading6 = TextStyle.H6
    let subtitle1 = TextStyle.Subtitle1
    let subtitle2 = TextStyle.Subtitle2
    let body1 = TextStyle.Body1
    let body2 = TextStyle.Body2
    let caption = TextStyle.Caption
    let overline = TextStyle.Overline
    let buttonText = TextStyle.Button

    let skeletonText = SkeletonVariant.Text
    let skeletonCircular = SkeletonVariant.Circular
    let skeletonRectangular = SkeletonVariant.Rectangular
    let skeletonPulse = SkeletonAnimation.Pulse
    let skeletonWave = SkeletonAnimation.Wave

    let alignLeft = TextAlign.Left
    let alignRight = TextAlign.Right
    let alignCenterText = TextAlign.Center

    // ---------------------------------------------------------------- layout

    /// A RadzenLayout — the responsive app shell.
    let layout (children: Node) =
        comp<RadzenLayout> {
            children
        }

    /// `RadzenComponents` — the singleton host that renders the imperative
    /// Radzen services: `RadzenDialog`, `RadzenNotification`, `RadzenTooltip`
    /// and `RadzenContextMenu`. It must be present somewhere in the render
    /// tree or `DialogService.Notify`/`NotificationService.Notify` messages are
    /// swallowed. It takes NO child content (it renders its fixed host set
    /// itself) — host it once at the layout root with no children.
    let components =
        Node(fun c b i ->
            b.OpenComponent<RadzenComponents>(i)
            b.CloseComponent()
            i)

    /// A RadzenHeader — the top bar (fixed when outside a RadzenLayout).
    let header (children: Node) =
        comp<RadzenHeader> {
            children
        }

    /// A RadzenFooter — the bottom bar (fixed when outside a RadzenLayout).
    let footer (children: Node) =
        comp<RadzenFooter> {
            children
        }

    /// A RadzenBody — the scrollable main content region.
    let body (children: Node) =
        comp<RadzenBody> {
            children
        }

    /// A responsive RadzenSidebar. Inside a RadzenLayout it auto-collapses
    /// below 768px. Controlled form: `expanded` + `onExpanded` two-way bind.
    let sidebarExpanded (expanded: bool) (onExpanded: bool -> unit) (children: Node) =
        // Native responsive sidebar (42-switches #1): Radzen hides/overlays
        // the sidebar itself below `ResponsiveMaxWidth` via matchMedia — no
        // hand-rolled Tailwind drawer or CSS media queries needed.
        // Verified: RadzenSidebar.razor.cs Responsive=true default,
        // ResponsiveMaxWidth="768px", Expanded/ExpandedChanged.
        comp<RadzenSidebar> {
            "Responsive" => true
            "ResponsiveMaxWidth" => "768px"
            "Expanded" => expanded
            attr.callback "ExpandedChanged" (fun (e: bool) -> onExpanded e)
            children
        }

    /// A RadzenSidebarToggle — the hamburger that toggles the sidebar.
    let sidebarToggle (onToggle: unit -> unit) =
        comp<RadzenSidebarToggle> {
            attr.callback "Click" (fun (_: EventArgs) -> onToggle ())
        }

    /// A responsive RadzenRow with a gap.
    let rowGap (gap: string) (children: Node) =
        comp<RadzenRow> {
            "Gap" => gap
            children
        }

    /// A RadzenRow with a gap plus vertical alignment and horizontal
    /// justification (see `alignCenter`/`justifyBetween` etc.). Lets a row
    /// distribute its columns with dynamic Radzen alignment rather than
    /// hardcoded CSS.
    let rowGapAlign (gap: string) (align: AlignItems) (justify: JustifyContent) (children: Node) =
        comp<RadzenRow> {
            "Gap" => gap
            "AlignItems" => align
            "JustifyContent" => justify
            children
        }

    /// A responsive RadzenColumn: full-width on mobile, then `sm`/`md`/`lg`.
    let columnResponsive (sm: int) (md: int) (lg: int) (children: Node) =
        comp<RadzenColumn> {
            "SizeXS" => 12
            "SizeSM" => sm
            "SizeMD" => md
            "SizeLG" => lg
            children
        }

    /// A responsive RadzenColumn with an extra Tailwind/Radzen `class`
    /// appended (e.g. `rz-p-4` for inner padding). Used inside `rz-p-0` cards
    /// so each column owns its own gutter, matching the official DataList
    /// demo pattern.
    let columnResponsiveClass (sm: int) (md: int) (lg: int) (cls: string) (children: Node) =
        comp<RadzenColumn> {
            "SizeXS" => 12
            "SizeSM" => sm
            "SizeMD" => md
            "SizeLG" => lg
            attr.``class`` cls
            children
        }

    /// A RadzenColumn with a fixed size (out of 12) at every breakpoint.
    let column (size: int) (children: Node) =
        comp<RadzenColumn> {
            "Size" => size
            children
        }

    /// A RadzenSplitter — a resizable panes container (side-by-side when
    /// `Orientation.Horizontal`, stacked when `Vertical`). Each pane is a
    /// `splitterPane`. A splitter needs an explicit height (via `Style`) or a
    /// flex fill parent, otherwise it collapses.
    let splitter (style: string) (children: Node) =
        comp<RadzenSplitter> {
            "Style" => style
            children
        }

    /// A resizable RadzenSplitterPane. Pass `size` (e.g. "300px", "40%") for a
    /// fixed initial size, or `None`/"" for an auto-sized pane.
    let splitterPane (size: string option) (children: Node) =
        comp<RadzenSplitterPane> {
            "Size" => (match size with Some s -> s | None -> "")
            children
        }

    /// A RadzenSidebarToggle is placed in the header; the layout shell passes
    /// the toggle callback through to the header when built via `layoutShell`.
    /// A vertical RadzenStack.
    let vStack (children: Node) =
        comp<RadzenStack> {
            children
        }

    /// A vertical RadzenStack with a gap.
    let vStackGap (gap: string) (children: Node) =
        comp<RadzenStack> {
            "Gap" => gap
            children
        }

    /// A horizontal RadzenStack.
    let hStack (children: Node) =
        comp<RadzenStack> {
            "Orientation" => horizontal
            children
        }

    /// A horizontal RadzenStack with a gap.
    let hStackGap (gap: string) (children: Node) =
        comp<RadzenStack> {
            "Orientation" => horizontal
            "Gap" => gap
            children
        }

    /// A horizontal RadzenStack with a gap plus vertical alignment and
    /// horizontal justification (see `alignCenter`/`justifyEnd` etc.). Used to
    /// right-align an action button inside a row without hardcoded CSS.
    let hStackGapAlign (gap: string) (align: AlignItems) (justify: JustifyContent) (children: Node) =
        comp<RadzenStack> {
            "Orientation" => horizontal
            "Gap" => gap
            "AlignItems" => align
            "JustifyContent" => justify
            children
        }

    // ---------------------------------------------------------------- content

    /// A RadzenText with a typography style and plain text content.
    let text (style: TextStyle) (s: string) =
        comp<RadzenText> {
            "TextStyle" => style
            "Text" => s
        }

    /// A RadzenCard container (filled by default).
    let card (children: Node) =
        comp<RadzenCard> {
            children
        }

    /// An outlined RadzenCard container.
    let cardOutlined (children: Node) =
        comp<RadzenCard> {
            "Variant" => outlined
            children
        }

    /// An outlined RadzenCard with an extra Tailwind/Radzen `class` appended.
    /// Used for cards whose inner columns own the padding (`rz-p-0` + `rz-p-*`
    /// columns), so the card doesn't add its own padding on top of the data
    /// list item's — giving clean, evenly-separated card interiors.
    let cardOutlinedClass (cls: string) (children: Node) =
        comp<RadzenCard> {
            "Variant" => outlined
            attr.``class`` cls
            children
        }

    /// Wrap any node in a plain div carrying extra utility classes. The
    /// generic escape hatch for "this subtree needs fixed width / alignment"
    /// tweaks (e.g. the live-server capacity column) without touching the
    /// wrapped component itself.
    let withClass (cls: string) (children: Node) =
        div {
            attr.``class`` cls
            children
        }

    /// An outlined RadzenCard with a hover lift + Material ripple. The hover
    /// lift is pure Tailwind (translate + glow), the ripple uses Radzen's own
    /// `rz-ripple` utility. The glow color comes from the Radzen `--rz-primary`
    /// token (via color-mix for alpha), so it tracks the brand accent instead
    /// of a hardcoded cyan. The `will-change` hints the browser to promote the
    /// layer to the GPU so the lift doesn't repaint the whole card.
    let cardHover (children: Node) =
        comp<RadzenCard> {
            "Variant" => outlined
            attr.``class``
                ("rz-ripple cursor-pointer w-full will-change-transform "
                 + "transition-[transform,box-shadow] duration-200 ease-out "
                 + "hover:-translate-y-1 "
                 + "hover:shadow-[0_8px_16px_color-mix(in_srgb,var(--rz-primary)_25%,transparent)] "
                 + "active:translate-y-0 active:duration-75")
            children
        }

    /// A responsive RadzenColumn that fills its height (`Size`-styled flex
    /// stretch). Radzen's row default aligns columns to stretch, but wrapping
    /// to a new line on smaller screens makes each wrapped column only as tall
    /// as its content. Adding `flex items-stretch` makes every column match
    /// the tallest in its line, so cards in a row align to equal height at
    /// every breakpoint — the "intelligent row height" pattern.
    let columnStretch (sm: int) (md: int) (lg: int) (children: Node) =
        comp<RadzenColumn> {
            "SizeXS" => 12
            "SizeSM" => sm
            "SizeMD" => md
            "SizeLG" => lg
            attr.``class`` "flex items-stretch"
            children
        }

    /// A pill-shaped RadzenBadge.
    let badgePill (style: BadgeStyle) (textValue: string) =
        comp<RadzenBadge> {
            "Text" => textValue
            "BadgeStyle" => style
            "IsPill" => true
        }

    /// A pulsing RadzenSkeleton loading placeholder, styled to mirror a bit of the
    /// target content. `style` sets width/height (e.g. "width: 100%; height: 1rem").
    /// The block color comes from the theme tokens in index.css
    /// (`--rz-skeleton-*`), tuned to the brutalist palette.
    let skeleton (style: string) =
        comp<RadzenSkeleton> {
            "Animation" => skeletonPulse
            "Style" => style
        }

    /// A vertical list of pulsing skeleton lines — a generic structural
    /// placeholder. Variants (`text`/`circular`/`rectangular`) and widths
    /// mirror common content shapes.
    let skeletonLines (lines: string list) =
        vStackGap "0.75rem" (concat {
            for w in lines do
                skeleton ("width: " + w + "; height: 1rem;")
        })

    /// A responsive row (RadzenRow) of `n` skeleton card placeholders, each
    /// laid out with the SAME column breakpoints as the page's real content
    /// grid. Sharing the grid shape means the skeleton mirrors the loaded
    /// layout exactly, so a breakpoint change here auto-updates both. The
    /// `cardBody` function renders one placeholder card's interior.
    let skeletonGrid (n: int) (sm: int) (md: int) (lg: int) (cardBody: unit -> Node) =
        rowGap "var(--gap-grid)" (concat {
            for _ in 1..n do
                columnResponsive sm md lg (cardOutlined (vStackGap "0.5rem" (cardBody ())))
        })

    /// A single game/tournament-style skeleton card body: image block + title
    /// line + chip line + two body lines. Mirrors `gameCard`/tournament cards.
    let skeletonCardBody () =
        concat {
            skeleton "width: 100%; height: 9rem;"
            skeleton "width: 55%; height: 1.25rem;"
            skeleton "width: 30%; height: 0.9rem;"
            skeleton "width: 90%; height: 0.9rem;"
        }

    /// A single team-card skeleton body: name line + badge + roster lines.
    let skeletonTeamBody () =
        concat {
            skeleton "width: 50%; height: 1.25rem;"
            skeleton "width: 35%; height: 1rem;"
            skeletonLines [ "80%"; "65%"; "72%" ]
        }

    /// A data-table skeleton: a heading line plus a row of column blocks,
    /// mirroring a RadzenDataGrid. `cols` is a list of column widths.
    let skeletonTable (cols: string list) =
        vStackGap "0.75rem" (concat {
            skeleton "width: 40%; height: 1.5rem;"
            for _ in 1..5 do
                rowGap "0.75rem" (concat {
                    for w in cols do
                        skeleton ("width: " + w + "; height: 1rem;")
                })
        })

    /// Wrap content in a fade-in so a skeleton→content swap animates instead
    /// of popping instantly. Each section's real content is wrapped in this
    /// so it eases in as its data arrives. Uses Tailwind's `animate-fade-in`
    /// utility (defined as an `--animate-fade-in` theme token in
    /// Community.Web.Server/Index.fs). `motion-reduce:animate-none` disables
    /// the entrance for users who prefer reduced motion.
    let fadeIn (children: Node) =
        div { attr.``class`` "animate-fade-in motion-reduce:animate-none"; children }

    /// A longer, softer entrance reserved for page heroes/headings (Tailwind
    /// `animate-rise`). Use for the first element on a page so it doesn't
    /// compete with the quicker card fades below. Respects reduced motion.
    let rise (children: Node) =
        div { attr.``class`` "animate-rise motion-reduce:animate-none"; children }

    /// A quick scale+opacity pop (Tailwind `animate-pop`) — for dialogs,
    /// badges, or any element that should "snap" in. Respects reduced motion.
    let pop (children: Node) =
        div { attr.``class`` "animate-pop motion-reduce:animate-none"; children }

    /// An HTML divider using the Radzen border token, for separating card
    /// sections without hand-rolled CSS. A thin horizontal rule tinted with
    /// the theme's border color (no hardcoded hex).
    let divider () =
        div { attr.``class`` "h-px w-full bg-[var(--rz-border-color)]" }

    /// The standard page error message for a `Failed` `RemoteData` slice.
    /// `what` is the plain name of the failed resource (e.g. "games").
    let failedView (what: string) =
        text body1 ("Couldn't load " + what + ".")



    /// The standard page header: a big display title, a short brand accent
    /// bar beneath it (Radzen `--rz-primary` token), and an optional subtitle.
    /// Every page opens with this one component so headings share the same
    /// rhythm and get a brand accent instead of a bare text line.
    /// (The old RadzenBreadCrumb "Home > Page" trail was removed — user
    /// request 2026-08-30: the navbar already communicates the location.)
    let pageHeadingCrumb (title: string) (subtitle: string option) (trail: (string * string option) list) =
        // `trail` is kept for signature compatibility with all call sites but
        // is intentionally ignored: no breadcrumb trail is rendered.
        ignore trail
        rise (vStackGap "0.375rem" (concat {
            text display3 title
            div { attr.``class`` "h-1 w-14 bg-[var(--rz-primary)]" }
            match subtitle with
            | Some s -> text subtitle1 s
            | None -> empty ()
        }))

    /// The status badge for a `GameServer.status` string. Centralizes the
    /// `online`/`maintenance`/`offline` -> badge mapping so Home and Servers
    /// don't each hand-write the same match. Unknown values fall back to
    /// "offline".
    let statusBadge (status: string) =
        match status with
        | "online" -> badgePill successBadge "online"
        | "maintenance" -> badgePill warningBadge "maintenance"
        | _ -> badgePill darkBadge "offline"

    // ---------------------------------------------------------------- feedback

    /// A RadzenAlert with a semantic style. Non-dismissible by default (the
    /// shared error clears on the next successful load / navigation).
    let alert (style: AlertStyle) (textValue: string) =
        comp<RadzenAlert> {
            "AlertStyle" => style
            "Text" => textValue
            "AllowClose" => false
        }

    /// Dismissible alert (42-switches #12): `AllowClose=true` renders the ×
    /// and `Close` fires when dismissed (verified RadzenAlert.razor.cs:53,256)
    /// — lets the caller clear the underlying message.
    ///
    /// NOTE: `RadzenAlert.Close` is a NON-generic `EventCallback` (not
    /// `EventCallback<'T>`), so Bolero's `attr.callback<'T>` (which builds
    /// `EventCallback<'T>`) throws InvalidCastException at runtime — the exact
    /// trap recorded in the repo notes ("keep AllowClose=false"). The fix is a
    /// hand-built non-generic EventCallback attribute.
    let alertDismissible (style: AlertStyle) (textValue: string) (onClose: unit -> unit) =
        Node(fun receiver builder i ->
            builder.OpenComponent<RadzenAlert>(i)
            let n = i + 1
            builder.AddAttribute(n, "AlertStyle", style)
            builder.AddAttribute(n + 1, "Text", textValue)
            builder.AddAttribute(n + 2, "AllowClose", true)
            // Non-generic EventCallback: RadzenAlert.Close is EventCallback
            // (NOT EventCallback<'T>), so attr.callback<'T> would throw.
            let cb =
                EventCallback.Factory.Create(receiver, Action(fun () -> onClose ()))
            builder.AddAttribute(n + 3, "Close", cb)
            builder.CloseComponent()
            n + 4)

    // ---------------------------------------------------------------- navigation

    /// A RadzenPanelMenu container for panel menu items.
    let panelMenu (children: Node) =
        // Single-expand groups (42-switches #31): opening one drawer group
        // collapses the other — standard drawer behavior.
        comp<RadzenPanelMenu> {
            "Multiple" => false
            children
        }

    /// A RadzenPanelMenuItem nav link. `matchAll` selects All-match (exact
    /// route); otherwise Prefix-match. `icon` (Radzen icon name, optional)
    /// renders the audit's icon+label drawer row (#10).
    let panelMenuItem (textValue: string) (path: string) (matchAll: bool) (icon: string option) =
        let iconAttr =
            match icon with
            | Some i -> "Icon" => i
            | None -> "Icon" => null
        comp<RadzenPanelMenuItem> {
            "Text" => textValue
            "Path" => path
            "Match" => (if matchAll then navMatchAll else navMatchPrefix)
            iconAttr
        }

    /// A RadzenPanelMenuItem with an expandable submenu (for a parent group
    /// like "Community"). `children` are nested `panelMenuItem` leaves.
    let panelMenuItemExpandable (textValue: string) (icon: string option) (children: Node) =
        let iconAttr =
            match icon with
            | Some i -> "Icon" => i
            | None -> "Icon" => null
        comp<RadzenPanelMenuItem> {
            "Text" => textValue
            iconAttr
            children
        }

    /// A horizontal RadzenMenu — a top navigation bar. `clickToOpen` toggles
    /// submenu interaction: `true` opens on click, `false` opens on hover
    /// (desktop). `responsive` collapses to a hamburger on small screens.
    /// Items are `menuItem`s (RadzenMenuItem), which navigate via their `Path`
    /// and render nested items as hover/click flyout submenus.
    let menu (clickToOpen: bool) (children: Node) =
        comp<RadzenMenu> {
            "ClickToOpen" => clickToOpen
            "Responsive" => true
            children
        }

    /// A leaf RadzenMenuItem — one entry in a horizontal `menu`. A leaf item
    /// with a `path` renders as a NavLink and navigates on click. Built with a
    /// raw node (NOT `comp`) so no `ChildContent` is emitted — otherwise
    /// `RadzenMenuItem.ChildContent != null` renders the submenu arrow on every
    /// item even when there is no dropdown.
    let menuItem (textValue: string) (path: string option) (matchAll: bool) =
        Node(fun c b i ->
            b.OpenComponent<RadzenMenuItem>(i)
            let n = i + 1
            b.AddAttribute(n, "Text", textValue)
            b.AddAttribute(n + 1, "Path", defaultArg path null)
            b.AddAttribute(n + 2, "Match", (if matchAll then navMatchAll else navMatchPrefix))
            b.CloseComponent()
            n + 3)

    /// A RadzenMenuItem with a flyout submenu. `children` are the nested
    /// `menuItem` leaves; opened per the parent `menu`'s `ClickToOpen` mode.
    let menuSubmenu (textValue: string) (children: Node) =
        comp<RadzenMenuItem> {
            "Text" => textValue
            children
        }

    // ---------------------------------------------------------------- buttons

    /// A Radzen button. `style` controls the semantic color, `onClickMsg` is
    /// the Elmish message to dispatch on click.
    let button (textValue: string) (style: ButtonStyle) (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "ButtonStyle" => style
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    /// RadzenButton with the built-in busy indicator (`IsBusy` + `BusyText`,
    /// verified: RadzenButton.razor.cs — IsBusy disables the button and swaps
    /// the content for a spinner). Use for buttons that trigger async work.
    let buttonBusy
        (textValue: string) (busyText: string) (busy: bool)
        (style: ButtonStyle) (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "IsBusy" => busy
            "BusyText" => busyText
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    /// Icon-only quiet button (42-audit #21 refresh pattern): a Text-variant
    /// button that renders just the Material icon — for panel-header actions.
    let buttonIcon (icon: string) (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Icon" => icon
            "Variant" => textVariant
            "ButtonStyle" => lightButton
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    /// Button with a plain callback (no dispatch threading needed — for
    /// page-local actions like "clear filters").
    let buttonAction (textValue: string) (style: ButtonStyle) (onClick: unit -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "ButtonStyle" => style
            attr.callback "Click" (fun (_: MouseEventArgs) -> onClick ())
        }

    /// Failed view with a retry button (review #1): danger alert + retry
    /// dispatching Shared.Load again. Callback-style so pages without a
    /// Shared dispatch in scope can wire it from the caller.
    let failedViewRetry (what: string) (onRetry: unit -> unit) =
        cardOutlined (vStackGap "0.75rem" (concat {
            alert dangerAlert ("Couldn't load " + what + ". The server may be unreachable.")
            hStackGap "0.5rem" (concat {
                buttonAction "Retry" secondaryButton onRetry
                text caption ("Click retry to fetch " + what + " again.")
            })
        }))

    /// Icon-only quiet button taking a plain callback (for pages without a
    /// dispatch in scope, e.g. refresh buttons wired by the caller).
    let iconButton (icon: string) (onClick: unit -> unit) =
        comp<RadzenButton> {
            "Icon" => icon
            "Variant" => textVariant
            "ButtonStyle" => lightButton
            attr.callback "Click" (fun (_: MouseEventArgs) -> onClick ())
        }

    /// A quiet, text-only Radzen button (Variant.Text) for inline row actions
    /// like the inbox's "Mark read" / "Mark all read" — visually a link,
    /// semantically a button (keeps keyboard focus + a11y).
    let textButton (textValue: string) (onClick: unit -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "Variant" => textVariant
            attr.callback "Click" (fun (_: MouseEventArgs) -> onClick ())
        }

    /// A larger icon-only button (user request 2026-08-30: the header bell
    /// was a small 36px text-variant button hugging the screen edge). Adds an
    /// aria-label (icon-only buttons need one for a11y) and a fixed hit area
    /// via Tailwind classes; callers place it with margin utilities.
    let iconButtonLg (icon: string) (label: string) (onClick: unit -> unit) =
        comp<RadzenButton> {
            "Icon" => icon
            "Variant" => textVariant
            "ButtonStyle" => lightButton
            // aria-label renders via CaptureUnmatchedValues (no AriaLabel
            // param exists on RadzenButton).
            attr.aria "label" label
            attr.``class`` "app-bell-button"
            attr.callback "Click" (fun (_: MouseEventArgs) -> onClick ())
        }

    /// A full-width Radzen button — the mobile-first CTA pattern: the button
    /// stretches to its container so the touch target spans the whole card
    /// (used for card actions like Games' Favourite).
    let buttonWide (textValue: string) (style: ButtonStyle) (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "Style" => "width: 100%;"
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    /// Full-width button with an explicit fill variant. The audit's G3 fix:
    /// INACTIVE/toggleable actions render Outlined (quiet), active/primary
    /// actions render Filled. Same touch target, weight follows meaning.
    let buttonWideVariant
        (textValue: string) (style: ButtonStyle) (variant: Variant)
        (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "Variant" => variant
            "Style" => "width: 100%;"
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    // ---------------------------------------------------------------- forms

    /// RadzenFormField — label + input + helper as ONE component
    /// (42-switches #3; verified RadzenFormField.razor.cs Text/Component/
    /// Variant). `component` is the wrapped input's Name (HtmlFor).
    /// Replaces the hand-rolled caption-text + input pairs.
    let formField (label: string) (component: string) (children: Node) =
        comp<RadzenFormField> {
            "Text" => label
            "Component" => component
            "Variant" => filled
            children
        }

    /// RadzenSwitch — bool toggle (42-switches #17; RadzenSwitch :
    /// FormComponent<bool>, verified). Generic so other bools reuse it.
    let switch (value: bool) (onChanged: bool -> unit) (name: string) =
        comp<RadzenSwitch> {
            "Value" => value
            "Name" => name
            attr.callback "ValueChanged" (fun (v: bool) -> onChanged v)
        }

    /// RadzenToggleButton — owns its on/off appearance (42-switches #5;
    /// verified RadzenToggleButton.razor.cs Value/ToggleButtonStyle/
    /// ToggleShade/ValueChanged). Replaces manual filled/outlined swaps.
    let toggleButton (text: string) (textOn: string) (value: bool) (onChanged: bool -> unit) =
        comp<RadzenToggleButton> {
            "Text" => (if value then textOn else text)
            "Value" => value
            "ButtonStyle" => ButtonStyle.Primary
            "ToggleButtonStyle" => ButtonStyle.Primary
            "Variant" => (if value then filled else outlined)
            attr.callback "ValueChanged" (fun (v: bool) -> onChanged v)
        }

    /// Full-width ToggleButton (Games favourite CTA): ToggleButton +
    /// width:100% — the toggle owns the on/off appearance.
    let toggleButtonWide (text: string) (textOn: string) (value: bool) (onChanged: bool -> unit) =
        comp<RadzenToggleButton> {
            "Text" => (if value then textOn else text)
            "Value" => value
            "ButtonStyle" => ButtonStyle.Primary
            "ToggleButtonStyle" => ButtonStyle.Primary
            "Variant" => (if value then filled else outlined)
            "Style" => "width: 100%;"
            attr.callback "ValueChanged" (fun (v: bool) -> onChanged v)
        }

    /// RadzenFieldset — titled, collapsible group with legend
    /// (42-switches #19; verified AllowCollapse/Text).
    let fieldset (title: string) (collapsible: bool) (children: Node) =
        comp<RadzenFieldset> {
            "Text" => title
            "AllowCollapse" => collapsible
            children
        }

    /// RadzenMarkdown — renders markdown text (42-switches #21;
    /// verified RadzenMarkdown.razor.cs Text property).
    let markdown (text: string) =
        comp<RadzenMarkdown> {
            "Text" => text
        }

    /// RadzenRating read-only display (42-switches #35; verified Stars/
    /// ReadOnly). For display-only ratings (prize tier, member score).
    // NOTE: RadzenRating : FormComponent<int> — Value is an INT (verified
    // RadzenRating.razor.cs:31). Passing a float throws
    // Arg_InvalidCastException at render (caught live on the details dialog).
    let rating (value: int) (stars: int) =
        comp<RadzenRating> {
            "Value" => value
            "Stars" => stars
            "ReadOnly" => true
        }

    /// RadzenLink — themed internal/external link (42-switches #39;
    /// verified RadzenLink.razor.cs Path/Text/Icon/ChildContent).
    let link (path: string) (children: Node) =
        comp<RadzenLink> {
            "Path" => path
            children
        }

    /// RadzenCardGroup — responsive equal-height card row that wraps below
    /// 576px (42-switches #37; verified RadzenCardGroup.razor.cs Responsive).
    /// Replaces hand-rolled flex-wrap + divider-hack composites.
    let cardGroup (children: Node) =
        comp<RadzenCardGroup> {
            "Responsive" => true
            children
        }

    /// A RadzenTextBox bound to a value via `ValueChanged`.
    let textBox (value: string) (onChange: string -> unit) =
        comp<RadzenTextBox> {
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    /// Live search box (42-audit #13; verified RadzenTextBox Placeholder/
    /// Immediate — Immediate fires Change per keystroke, no debounce needed).
    let searchBox (value: string) (onChanged: string -> unit) =
        comp<RadzenTextBox> {
            "Value" => value
            "Placeholder" => "Search…"
            "Immediate" => true
            attr.callback "ValueChanged" (fun (v: string) -> onChanged v)
        }

    /// Named textBox (Name is required by RadzenFormField's `Component`
    /// binding — 42-switches #3).
    let namedTextBox (name: string) (value: string) (onChange: string -> unit) =
        comp<RadzenTextBox> {
            "Name" => name
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    /// A RadzenTextArea bound to a multiline value.
    let textArea (value: string) (onChange: string -> unit) =
        comp<RadzenTextArea> {
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    /// Named textArea for formField `Component` binding (#3).
    let namedTextArea (name: string) (value: string) (onChange: string -> unit) =
        comp<RadzenTextArea> {
            "Name" => name
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    /// A RadzenAutoComplete — a search box that suggests/filters as you type.
    /// `data` is the item collection; `textProperty` is the record field shown
    /// in the suggestions (e.g. "username"). `minLength` gates when the
    /// suggestions appear. `onSelectedItem` fires with the picked object when
    /// the user chooses a suggestion.
    let autoComplete<'data> (data: seq<'data>) (textProperty: string) (value: string) (onValueChanged: string -> unit) =
        comp<RadzenAutoComplete> {
            "Data" => data
            "TextProperty" => textProperty
            "Value" => value
            "MinLength" => 0
            "OpenOnFocus" => true
            // Debounce the filter (library default is 500ms; 150ms feels
            // instant while still batching keystrokes — audit finding #37).
            "FilterDelay" => 150
            attr.callback "ValueChanged" (fun (v: string) -> onValueChanged v)
            // RadzenAutoComplete's ValueChanged binds to the `onchange` event
            // (fires only on Enter/blur/select). For LIVE filtering as you type,
            // capture the bubbling `input` event on the wrapper element.
            attr.callback "oninput" (fun (e: Microsoft.AspNetCore.Components.ChangeEventArgs) ->
                let v = if isNull e.Value then "" else string e.Value
                onValueChanged v)
        }

    /// A RadzenLogin — a ready-made sign-in form (username/password fields with
    /// built-in required validation). `AllowRegister`, `AllowResetPassword` and
    /// `AllowRememberMe` are enabled so the standard sign-in extras render:
    /// a "Remember me" switch, a "Forgot password" link, and a "Sign up"
    /// call-to-action below the form. `onLogin` receives the submitted
    /// `(username, password)`; `onRegister`/`onResetPassword` are invoked when
    /// the user clicks those extras (the caller decides what to do — there is
    /// no register/reset flow in the mock backend).
    let login (onLogin: string * string * bool -> unit) (onRegister: unit -> unit) (onResetPassword: string -> unit) =
        comp<RadzenLogin> {
            "AllowRegister" => true
            "AllowResetPassword" => true
            "AllowRememberMe" => true
            // Filled fields match the #141414 input aesthetic (#29).
            "FormFieldVariant" => Variant.Filled
            attr.callback "Login" (fun (args: LoginArgs) ->
                let user = if isNull args.Username then "" else args.Username
                let pass = if isNull args.Password then "" else args.Password
                // Remember-me flag (42-audit #28/#36) finally delivered.
                onLogin (user, pass, args.RememberMe))
            // RadzenLogin.Register is a NON-generic `EventCallback` (not
            // `EventCallback<EventArgs>`), which Bolero's `attr.callback`
            // can't produce — so attach it with a raw attribute that boxes a
            // plain `EventCallback`.
            Attr(fun receiver builder seq ->
                builder.AddAttribute(
                    seq,
                    "Register",
                    EventCallback.Factory.Create(receiver, Action(fun () -> onRegister ())))
                seq + 1)
            attr.callback "ResetPassword" (fun (v: string) -> onResetPassword (if isNull v then "" else v))
        }

    /// A `RadzenTemplateForm<TItem>` — the model/validation container for form
    /// components. Radzen's docs require inputs like `RadzenLogin` to live
    /// inside one. Built as a raw `Node` (not `comp`) because Bolero's
    /// `comp { children }` always emits `ChildContent` as a plain
    /// `RenderFragment`, but `RadzenTemplateForm<T>.ChildContent` is the
    /// *typed* `RenderFragment<EditContext>` — a plain fragment can't be cast to
    /// it, which throws at render time. So we open the component directly and
    /// pass `ChildContent` as the correctly-typed fragment (the `EditContext`
    /// is the form's model wrapper, ignored here since login handles its own
    /// validation).
    let templateForm<'T> (data: 'T) (children: Node) =
        Node(fun c b i ->
            b.OpenComponent<RadzenTemplateForm<'T>>(i)
            let n = i + 1
            b.AddAttribute(n, "Data", data)
            b.AddAttribute(
                n + 1,
                "ChildContent",
                RenderFragment<EditContext>(fun _ ->
                    RenderFragment(fun b2 -> children.Invoke(c, b2, n + 2) |> ignore)))
            b.CloseComponent()
            n + 2)

    /// A `RadzenLogin` wrapped in the official Radzen demo structure: a
    /// centred, width-constrained `RadzenCard` containing a
    /// `RadzenTemplateForm` that wraps the `RadzenLogin`. Radzen's docs state
    /// the login MUST live inside a `RadzenTemplateForm` (that's how its
    /// built-in required validation is wired up), and the card keeps it from
    /// stretching edge-to-edge — which would push the short labels far from
    /// their inputs. Mirrors `RadzenBlazorDemos/Pages/LoginSimple.razor`.
    let loginCard
        (onLogin: string * string * bool -> unit)
        (onRegister: unit -> unit)
        (onResetPassword: string -> unit) =
        comp<RadzenCard> {
            attr.``class`` "rz-my-12 rz-mx-auto p-[var(--pad-card)] md:p-[var(--pad-page)]"
            // `width: 100%` makes the card actually use its max-width cap —
            // otherwise RadzenCard shrink-wraps to its content, leaving too
            // little room for the label + input columns to sit side by side.
            "Style" => "width: 100%; max-width: 600px;"
            templateForm "SimpleLogin" (login onLogin onRegister onResetPassword)
        }

    // ---------------------------------------------------------------- fragment

    /// Bind a sequence of child nodes to a named `RenderFragment` component
    /// parameter. Bolero's `comp { children }` always fills `ChildContent`, but
    /// some Radzen containers (Tabs, Carousel, Timeline) read their items from
    /// a dedicated `RenderFragment` parameter (`Tabs`/`Items`) instead — so we
    /// must pass them an explicit fragment. Build the node list via `concat { }`.
    let fragmentParam (paramName: string) (children: Node) =
        Attr(fun receiver builder sequence ->
            builder.AddAttribute(sequence, paramName,
                RenderFragment(fun builder ->
                    children.Invoke(receiver, builder, 0) |> ignore))
            sequence + 1)

    /// RadzenAccordion — expand/collapse Q&A panels (42-audit #37;
    /// verified accordion.md). Items are `accordionItem`s.
    let accordion (children: Node) =
        comp<RadzenAccordion> {
            fragmentParam "Items" children
        }

    /// Multi-expand variant (verified RadzenAccordion.razor.cs line 62:
    /// `Multiple`) — several game sections can stay open at once.
    let accordionMultiple (children: Node) =
        comp<RadzenAccordion> {
            "Multiple" => true
            fragmentParam "Items" children
        }

    /// One RadzenAccordionItem — header text + collapsible body.
    let accordionItem (title: string) (children: Node) =
        comp<RadzenAccordionItem> {
            "Text" => title
            children
        }

    /// An accordion item with a header icon and pre-selected state (verified
    /// RadzenAccordionItem.cs: Icon line 24, Selected line 38 + SelectedChanged).
    /// `onSelected` mirrors the user's expand/collapse back into MVU state.
    let accordionItemFull
        (title: string) (icon: string) (selected: bool)
        (onSelected: bool -> unit) (children: Node) =
        comp<RadzenAccordionItem> {
            "Text" => title
            "Icon" => icon
            "Selected" => selected
            attr.callback "SelectedChanged" (fun (sel: bool) -> onSelected sel)
            children
        }


    // ---------------------------------------------------------------- popup

    /// A RadzenPopup — floating content panel (verified RadzenPopup API:
    /// Visible, Lazy, CloseOnClickOutside, AutoFocusFirstElement, Open/Close
    /// events). Controlled form: `visible` drives it; toggling is the
    /// CALLER's job (e.g. a bell button). Tailwind classes land on the root
    /// via `css`; with `Lazy=true` the content renders only when first
    /// opened. `onClosed` mirrors the Close event back into MVU state.
    let popup (visible: bool) (css: string) (onClosed: unit -> unit) (children: Node) =
        // RadzenPopup.Close is a NON-generic EventCallback (verified
        // RadzenPopup API ref + same gotcha as RadzenAlert.Close): Bolero's
        // attr.callback only produces EventCallback<'T>, which Blazor refuses
        // to cast. So the component is built manually and Close is bound with
        // the non-generic EventCallback.Factory.Create overload.
        Node(fun receiver builder sequence ->
            builder.OpenComponent<RadzenPopup>(sequence)
            let mutable i = sequence + 1
            builder.AddAttribute(i, "Visible", visible); i <- i + 1
            builder.AddAttribute(i, "Lazy", true); i <- i + 1
            builder.AddAttribute(i, "CloseOnClickOutside", true); i <- i + 1
            builder.AddAttribute(i, "AutoFocusFirstElement", false); i <- i + 1
            builder.AddAttribute(i, "class", css); i <- i + 1
            builder.AddAttribute(i, "Close",
                EventCallback.Factory.Create(receiver, System.Action(onClosed)))
            i <- i + 1
            builder.AddAttribute(i, "ChildContent",
                RenderFragment(fun b -> children.Invoke(receiver, b, 0) |> ignore))
            builder.CloseComponent()
            sequence + 2)

    // ---------------------------------------------------------------- profile menu

    /// A RadzenProfileMenu — a collapsed nav item that expands into a dropdown
    /// of `profileMenuItem`s. NOTE: items have no per-item click — item clicks
    /// bubble to the parent's `Click` with the item carrying its `Value`, so
    /// route on `item.Value`. `template` is the always-visible trigger content
    /// (e.g. the signed-in username).
    let profileMenu (template: Node) (onClick: string -> unit) (children: Node) =
        comp<RadzenProfileMenu> {
            // The avatar is the trigger affordance — hide the caret (#32).
            "ShowIcon" => false
            fragmentParam "Template" template
            attr.callback "Click" (fun (item: RadzenProfileMenuItem) ->
                if not (isNull (box item)) && not (isNull item.Value) then
                    onClick item.Value)
            children
        }

    /// A RadzenProfileMenuItem — one entry in a `profileMenu` dropdown. The
    /// `value` is what the parent's `onClick` receives (items have no
    /// independent click handler).
    let profileMenuItem (textValue: string) (value: string) =
        comp<RadzenProfileMenuItem> {
            "Text" => textValue
            "Value" => value
        }

    // ---------------------------------------------------------------- tabs

    /// A RadzenTabs container. Reads its items from the `Tabs` render fragment
    /// (`fragmentParam`). `RenderMode` Server by default; uncontrolled — with
    /// `SelectedIndex` left at its default -1 the first tab is auto-selected,
    /// so no page state is needed — ideal for a view-only wrapper. Items are
    /// `RadzenTabsItem` via `tabItem`.
    let tabs (children: Node) =
        comp<RadzenTabs> {
            fragmentParam "Tabs" children
        }

    /// A RadzenTabsItem — one tab header (text) + its panel content.
    let tabItem (textValue: string) (children: Node) =
        comp<RadzenTabsItem> {
            "Text" => textValue
            children
        }

    /// A RadzenSelectBar — the segmented control (audit #18/#19): a single-
    /// row, equal-width option strip with a built-in selected state (uses the
    /// `--rz-selectbar-*` theme vars). `'T` is the value type (pass explicitly,
    /// e.g. `selectBar<string>`). `allowToggles` enables multi-select.
    let selectBar<'T> (value: 'T option) (onChange: 'T -> unit) (multiple: bool) (children: Node) =
        comp<RadzenSelectBar<'T>> {
            "Value" => (match value with Some v -> v | None -> Unchecked.defaultof<'T>)
            attr.callback "ValueChanged" (fun (v: 'T) ->
                if not (isNull (box v)) then onChange v)
            "Multiple" => multiple
            attr.``class`` "w-full"
            children
        }

    /// One segment of a `selectBar`: label + optional icon + the value it
    /// selects.
    let selectBarItem (textValue: string) (value: obj) (icon: string option) =
        let iconAttr =
            match icon with
            | Some i -> "Icon" => i
            | None -> "Icon" => null
        comp<RadzenSelectBarItem> {
            "Text" => textValue
            "Value" => value
            iconAttr
        }

    // ---------------------------------------------------------------- progress

    /// A determinate RadzenProgressBarCircular — a compact ring showing
    /// `value`/`max` with the value inside the circle. `size` is one of the
    /// `circular*` enums; `showValue` displays the percentage in the center.
    let progressBarCircular
        (value: float) (max: float) (size: ProgressBarCircularSize)
        (showValue: bool) (style: ProgressBarStyle) =
        comp<RadzenProgressBarCircular> {
            "Value" => value
            "Max" => max
            "Size" => size
            "ShowValue" => showValue
            "ProgressBarStyle" => style
        }

    /// Circular gauge with custom inner content (`Template`, verified:
    /// RadzenProgressBarCircular.razor.cs) — e.g. a "full"/lock icon when the
    /// server is at capacity instead of the "100%" text.
    let progressBarCircularContent
        (value: float) (max: float) (size: ProgressBarCircularSize)
        (style: ProgressBarStyle) (content: Node) =
        comp<RadzenProgressBarCircular> {
            "Value" => value
            "Max" => max
            "Size" => size
            "ShowValue" => false
            "ProgressBarStyle" => style
            fragmentParam "Template" content
        }

    /// A determinate linear RadzenProgressBar (42-switches #7; verified
    /// RadzenProgressBar.razor.cs Value/Max/ShowValue). For capacity bars.
    let progressBar (value: float) (max: float) (showValue: bool) (style: ProgressBarStyle) =
        comp<RadzenProgressBar> {
            "Value" => value
            "Max" => max
            "ShowValue" => showValue
            "ProgressBarStyle" => style
        }

    /// Indeterminate hairline bar (42-audit #6): global loading indicator.
    let loadingBar =
        comp<RadzenProgressBar> {
            "Mode" => ProgressBarMode.Indeterminate
            "ShowValue" => false
            "Style" => "height: 2px; width: 100%;"
        }

    // ---------------------------------------------------------------- timeline

    /// A vertical RadzenTimeline — a sequence of `timelineItem` nodes (great
    /// for a news/announcements history or a tournament roadmap). Items are
    /// passed via the `Items` render fragment (see `fragmentParam`).
    let timeline (children: Node) =
        comp<RadzenTimeline> {
            fragmentParam "Items" children
        }

    /// A RadzenTimelineItem — one node: a label (usually a date) on the left,
    /// rich child content (headline + body) on the right, and a colored point.
    let timelineItem (label: string) (point: PointStyle) (children: Node) =
        comp<RadzenTimelineItem> {
            "Label" => label
            "PointStyle" => point
            children
        }

    /// Timeline item with an icon INSIDE the point (`PointContent`, verified:
    /// RadzenTimelineItem.razor.cs) — icon-driven news feed markers.
    let timelineItemIcon (label: string) (point: PointStyle) (icon: string) (children: Node) =
        // PointContent is a RenderFragment param — wire it with fragmentParam.
        let pointIcon = comp<RadzenIcon> { "Icon" => icon }
        comp<RadzenTimelineItem> {
            "Label" => label
            "PointStyle" => point
            "PointSize" => pointSizeMedium
            fragmentParam "PointContent" pointIcon
            children
        }

    // ---------------------------------------------------------------- carousel

    /// A RadzenCarousel cycling through `carouselItem` children. On desktop
    /// (`lg`/`xl` breakpoints) `itemsPerPage` slides are visible at once; on
    /// phones the carousel always pages ONE slide per view. Radzen's
    /// `RadzenCarouselItem` hardcodes `flex: 0 0 calc(100% / n)` via an inline
    /// style when `ItemsPerPage > 1` (no responsive variant exists — see its
    /// `ItemStyle` in the vendored source), so the mobile 1-up is enforced in
    /// CSS: the `app-carousel` hook class drives an
    /// `@media (max-width: 767px)` override in index.css that forces every
    /// slide to full width. `PagerPosition`/`PagerOverlay` follow the official
    /// demo defaults for a static bottom dot pager (no text overlap).
    let carousel (itemsPerPage: int) (children: Node) =
        comp<RadzenCarousel> {
            "ItemsPerPage" => itemsPerPage
            "PagerPosition" => pagerBottom
            "PagerOverlay" => false
            // Autoplay ON with a fast cycle (user request 2026-08-30:
            // "make auto transition faster"). Library default is Auto=true
            // at 4000ms; we cycle every 2.5s. Desktop swipe was removed —
            // autoplay + the dot pager are the navigation.
            "Auto" => true
            "Interval" => 2500.0
            // No prev/next arrow buttons (user request 2026-08-30): the dot
            // pager + swipe (AllowScroll stays true) are the navigation.
            // Verified: RadzenCarousel.razor renders the arrows only inside
            // `@if (AllowNavigation)`; API ref "Set to true by default".
            "AllowNavigation" => false
            attr.``class`` "app-carousel"
            fragmentParam "Items" children
        }

    /// A RadzenCarouselItem — a single slide in a `carousel`.
    let carouselItem (children: Node) =
        comp<RadzenCarouselItem> {
            children
        }

    // ---------------------------------------------------------------- media

    /// A RadzenImage — renders an `<img>` from a URL, base64 data, or app asset.
    /// `alt` is shown by screen readers and when the image fails to load.
    /// The box is UNIFORM across all games: full container width, a fixed
    /// height, and `object-fit: cover` so any source aspect ratio (460×215,
    /// 460×259, …) fills the same 9rem box by cropping rather than stretching
    /// or leaving uneven gaps — every card in a row stays the same height. The
    /// height matches the game-card skeleton's image block, so the skeleton and
    /// the loaded content swap without a layout jump.
    /// A Material icon glyph (RadzenIcon). The app uses Material Symbols
    /// via the Radzen icon font — `name` is the icon id (e.g. "lock").
    let icon (name: string) =
        comp<RadzenIcon> {
            "Icon" => name
        }

    let image (src: string) (alt: string) =
        comp<RadzenImage> {
            "Path" => src
            "AlternateText" => alt
            // w-full + h-full: always fill the parent box (the 16/9 media
            // box) — the image ENLARGES past its intrinsic size when the box
            // is bigger, and object-fit:cover crops (never distorts) any
            // source ratio to fill. display:block kills the inline-baseline
            // gap under the img.
            "Style" => "width: 100%; height: 100%; object-fit: cover; display: block;"
        }

    /// A tidy media card: a uniform banner-image box + a padded content
    /// (meta) section, as ONE component so every image-led card (featured-game
    /// slide, game grid card, …) shares the exact same structure and spacing.
    /// The image box uses a fixed aspect-ratio (16/9) with `object-fit: cover`,
    /// so any source banner fills it edge-to-edge without distortion and every
    /// card in a row/grid is the same height. The content section owns the
    /// padding (the image is full-bleed to the card edges), and children flow
    /// in a vertical stack — callers pass title/chip/text/button nodes.
    let mediaCard (imageSrc: string) (imageAlt: string) (children: Node) =
        // Card is a full-height flex column so the meta section flex-fills and
        // a trailing CTA pins to the card bottom (audit #2: ragged bottoms
        // when descriptions vary; parent column is items-stretch).
        cardOutlinedClass "flex flex-col h-full" (concat {
            // Full-bleed image box with a LOCKED 16/9 aspect (audit #2:
            // intrinsic source ratios made card bottoms ragged — 16/9 crops
            // cleanly instead). Overflow hidden clips any source ratio.
            // min-w-0: inside a flex/grid column the box must be allowed to
            // GROW to the column width (a fixed intrinsic image width must
            // never cap the box) — pairs with the w-full img below.
            div {
                attr.``class`` "overflow-hidden w-full min-w-0 aspect-[16/9]"
                image imageSrc imageAlt
            }
            // Padded meta section; flex-1 so the caller's trailing mt-auto
            // node (CTA) reaches the card bottom.
            div {
                attr.``class`` "flex flex-col flex-1 gap-2 px-[var(--pad-card)] py-[var(--pad-card)]"
                children
            }
        })

    /// A RadzenChip label with a badge-style color and an optional fill variant.
    let chip (textValue: string) (style: BadgeStyle) =
        comp<RadzenChip> {
            "Text" => textValue
            "ChipStyle" => style
        }

    /// Interactive chip (audit #27): selectable filter tag with a selected
    /// fill and click handler (verified: RadzenChip.razor.cs
    /// Selected/Click/Close params).
    let chipSelectable (textValue: string) (style: BadgeStyle) (selected: bool) (onClick: unit -> unit) =
        comp<RadzenChip> {
            "Text" => textValue
            "ChipStyle" => style
            "Selected" => selected
            attr.callback "Click" (fun (_: Microsoft.AspNetCore.Components.Web.MouseEventArgs) -> onClick ())
        }

    // ---------------------------------------------------------------- buttons (dropdown)

    /// A RadzenSplitButton — a primary button plus a dropdown menu of
    /// `splitButtonItem`s. The main `Text`/`ButtonStyle` is on the left, the
    /// arrow toggles the menu.
    ///
    /// NOTE: `RadzenSplitButtonItem` has NO per-item click handler. Item clicks
    /// always bubble up to the parent's `Click` with the item as the argument
    /// (the main button passes `null`). So `onClick` receives the chosen
    /// action's `value`: `None` = the main button, `Some value` = that item.
    /// Route on that value — never pass a per-item callback.
    let splitButton (textValue: string) (style: ButtonStyle) (onClick: string option -> unit) (children: Node) =
        comp<RadzenSplitButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "DropDownIcon" => "expand_more"
            attr.callback "Click" (fun (item: RadzenSplitButtonItem) ->
                onClick (if isNull (box item) then None else Some item.Value))
            children
        }

    /// SplitButton with an explicit fill variant (audit G3: lifecycle actions
    /// like tournament close/reopen are Outlined — state-changing, not
    /// navigation — while the button keeps its semantic colour).
    let splitButtonVariant
        (textValue: string) (style: ButtonStyle) (variant: Variant)
        (onClick: string option -> unit) (children: Node) =
        comp<RadzenSplitButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "Variant" => variant
            "DropDownIcon" => "expand_more"
            attr.callback "Click" (fun (item: RadzenSplitButtonItem) ->
                onClick (if isNull (box item) then None else Some item.Value))
            children
        }

    /// SplitButton with the built-in busy indicator (verified:
    /// RadzenSplitButton.razor.cs IsBusy/BusyText). Disabled+spinner while a
    /// tournament mutation is in flight.
    let splitButtonBusy
        (textValue: string) (busyText: string) (busy: bool)
        (style: ButtonStyle) (variant: Variant)
        (onClick: string option -> unit) (children: Node) =
        comp<RadzenSplitButton> {
            "Text" => textValue
            "ButtonStyle" => style
            "Variant" => variant
            "IsBusy" => busy
            "BusyText" => busyText
            attr.callback "Click" (fun (item: RadzenSplitButtonItem) ->
                onClick (if isNull (box item) then None else Some item.Value))
            children
        }

    /// A RadzenSplitButtonItem — one menu entry in a `splitButton`. `value` is
    /// what the parent's `onClick` receives to distinguish this item (it has
    /// no independent click handler — item clicks bubble to the splitButton).
    let splitButtonItem (textValue: string) (value: string) =
        comp<RadzenSplitButtonItem> {
            "Text" => textValue
            "Value" => value
        }

    // ---------------------------------------------------------------- members

    /// A RadzenGravatar — renders a member's avatar from their email (or a
    /// fallback when the email is missing).
    let gravatar (email: string option) (size: int) =
        let safe = defaultArg email ""
        comp<RadzenGravatar> {
            "Email" => safe
            "Size" => size
        }

    /// Initials avatar (audit #5/#6): a circular chip with the member's
    /// first letters. Deliberately NOT RadzenGravatar — its AlternateText
    /// getter hits Radzen's Localize()/ResourceManager reflection path that
    /// AOT/trim strips (see Members.avatarCell history). Pure div + theme
    /// tokens: no reflection, no network, works offline.
    let initialsAvatar (name: string) =
        let initials =
            name.Split(' ')
            |> Array.filter (fun w -> w.Length > 0)
            |> Array.truncate 2
            |> Array.map (fun w -> w[0].ToString().ToUpperInvariant())
            |> String.concat ""
        div {
            attr.``class``
                ("shrink-0 w-9 h-9 rounded-full grid place-items-center select-none "
                 + "bg-[var(--rz-primary-lighter)] text-[var(--rz-primary)] "
                 + "text-[0.875rem] font-medium")
            attr.title name
            text body2 (if initials = "" then "?" else initials)
        }

    // ---------------------------------------------------------------- data grid

    /// A RadzenDataGrid rendering rows of a record/class type. `data` is the
    /// item sequence and `columns` are `dataGridColumn<'T>` nodes. `'T` must be
    /// given EXPLICITLY (e.g. `dataGrid<GameServer> list ...`) so the grid and
    /// its columns share the same row type — otherwise F# infers `obj` and
    /// Radzen's Property bindings can't read the record fields.
    let dataGrid<'T when 'T : not null> (data: seq<'T>) (columns: Node) =
        comp<RadzenDataGrid<'T>> {
            "Data" => data
            "AllowSorting" => true
            "AllowFiltering" => true
            "AllowPaging" => true
            "ShowCellDataAsTooltip" => true
            // Reflow to a vertical card list on narrow screens (< 768px).
            "Responsive" => true
            fragmentParam "Columns" columns
        }

    /// DataGrid with the audit's grid upgrades (findings #3–#6, #9):
    /// `EmptyText` custom empty message, `Density=Compact` rows, and a
    /// `RowRender` hook for value-driven row styling (e.g. tint a server row
    /// near capacity). `rowClass` maps a row to an extra CSS class (None =
    /// no change). `AllowVirtualization` on for scroll-heavy lists.
    let dataGridAdvanced<'T when 'T : not null>
        (data: seq<'T>)
        (emptyText: string option)
        (compact: bool)
        (virtualize: bool)
        (rowClass: ('T -> string option) option)
        (rowSelect: ('T -> unit) option)
        (columns: Node) =
        // Build attrs OUTSIDE the CE body (the `comp` builder forbids
        // bare match/if expressions inside — known Bolero gotcha).
        let emptyAttr =
            match emptyText with
            | Some t -> "EmptyText" => t
            | None -> "EmptyText" => null
        let densityAttr =
            if compact then "Density" => Radzen.Density.Compact
            else "Density" => Radzen.Density.Default
        let rowRenderAttr =
            match rowClass with
            | Some f ->
                Attr(fun _ builder seq ->
                    builder.AddAttribute(seq, "RowRender",
                        Action<Radzen.RowRenderEventArgs<'T>>(fun args ->
                            match f args.Data with
                            | Some cls -> args.Attributes.Add("class", cls)
                            | None -> ()))
                    seq + 1)
            | None -> Attr(fun _ _ seq -> seq)
        // Row-click → detail (#6): AllowRowSelectOnRowClick + RowSelect
        // (verified RadzenDataGrid.razor.cs:2404).
        let rowSelectClickAttr =
            match rowSelect with
            | Some _ -> "AllowRowSelectOnRowClick" => true
            | None -> "AllowRowSelectOnRowClick" => false
        let rowSelectCbAttr =
            match rowSelect with
            | Some f -> attr.callback "RowSelect" (fun (item: 'T) -> f item)
            | None -> attr.callback "RowSelect" (fun (_: 'T) -> ())
        comp<RadzenDataGrid<'T>> {
            "Data" => data
            "AllowSorting" => true
            "AllowFiltering" => true
            "AllowPaging" => true
            "ShowCellDataAsTooltip" => true
            "Responsive" => true
            emptyAttr
            densityAttr
            "AllowVirtualization" => virtualize
            rowRenderAttr
            rowSelectClickAttr
            rowSelectCbAttr
            fragmentParam "Columns" columns
        }

    /// DataGrid template column with an optional footer (42-switches #10):
    /// `FooterTemplate` aggregates recompute on sort/filter/page (verified
    /// datagrid-footer-totals.md). `footer` renders under the column.
    let dataGridTemplateColumnFooter<'T when 'T : not null> (title: string) (footer: Node option) (cell: 'T -> Node) =
        Node(fun c b i ->
            b.OpenComponent<RadzenDataGridColumn<'T>>(i)
            let n0 = i + 1
            b.AddAttribute(n0, "Title", title)
            let mutable n = n0 + 1
            match footer with
            | Some f ->
                b.AddAttribute(n, "FooterTemplate",
                    RenderFragment(fun rt -> f.Invoke(c, rt, 0) |> ignore))
                n <- n + 1
            | None -> ()
            b.AddAttribute(n, "Template",
                RenderFragment<'T>(fun ctx ->
                    RenderFragment(fun rt -> (cell ctx).Invoke(c, rt, 0) |> ignore)))
            b.CloseComponent()
            n + 1)

    /// A RadzenDataGridColumn with a custom cell template instead of a raw
    /// `Property` binding — for rendering a value that isn't a plain string
    /// (e.g. an F# `option<string>`). `title` is the header; `cell` maps each
    /// row to the Node shown in that cell. Built with `attr.fragmentWith`
    /// because the column `Template` is a `RenderFragment<'T>`.
    let dataGridTemplateColumn<'T when 'T : not null> (title: string) (cell: 'T -> Node) =
        Node(fun c b i ->
            b.OpenComponent<RadzenDataGridColumn<'T>>(i)
            let n = i + 1
            b.AddAttribute(n, "Title", title)
            b.AddAttribute(n + 1, "Sortable", true)
            b.AddAttribute(n + 2, "Filterable", false)
            let template =
                RenderFragment<'T>(fun ctx ->
                    RenderFragment(fun rt ->
                        (cell ctx).Invoke(c, rt, 0) |> ignore))
            b.AddAttribute(n + 3, "Template", template)
            b.CloseComponent()
            n + 4)

    // ---------------------------------------------------------------- data list

    /// A RadzenDataList rendering items with a custom card template instead of
    /// table rows. `data` is the item sequence; `renderItem` maps each `'T` to
    /// the Node shown for that item. The `Template` is a `RenderFragment<'T>`,
    /// built with the same manual-Node + `RenderFragment<'T>` pattern as
    /// Template column with an initial sort (42-audit #19; verified
    /// RadzenDataGridColumn.razor.cs:310 `SortOrder`). Gives the grid a
    /// declared default sort without code-behind.
    let dataGridTemplateColumnSorted<'T when 'T : not null>
        (title: string) (initial: Radzen.SortOrder) (cell: 'T -> Node) =
        Node(fun c b i ->
            b.OpenComponent<RadzenDataGridColumn<'T>>(i)
            let n = i + 1
            b.AddAttribute(n, "Title", title)
            b.AddAttribute(n + 1, "Sortable", true)
            b.AddAttribute(n + 2, "Filterable", false)
            b.AddAttribute(n + 3, "SortOrder", initial)
            let template =
                RenderFragment<'T>(fun ctx ->
                    RenderFragment(fun rt ->
                        (cell ctx).Invoke(c, rt, 0) |> ignore))
            b.AddAttribute(n + 4, "Template", template)
            b.CloseComponent()
            n + 5)

    /// `dataGridTemplateColumn` (NOT `comp { children }`, which would bind
    /// ChildContent and throw at runtime).
    let dataList<'T when 'T : not null> (data: seq<'T>) (wrapItems: bool) (renderItem: 'T -> Node) =
        Node(fun c b i ->
            b.OpenComponent<RadzenDataList<'T>>(i)
            let n = i + 1
            b.AddAttribute(n, "Data", data)
            b.AddAttribute(n + 1, "WrapItems", wrapItems)
            let template =
                RenderFragment<'T>(fun ctx ->
                    RenderFragment(fun rt ->
                        (renderItem ctx).Invoke(c, rt, 0) |> ignore))
            b.AddAttribute(n + 2, "Template", template)
            b.CloseComponent()
            n + 3)

    // ---------------------------------------------------------------- dialog

    /// A simple labeled field row for dialog detail bodies: a muted label
    /// followed by a value, stacked vertically.
    let detailField (label: string) (value: string) =
        // Definition-list row (audit #21.2/#22): muted overline label above a
        // strong value, separated from the next row by a hairline divider.
        // Padding-block uses the fluid card token so dialog rows match the
        // card rhythm; the divider uses the theme border token.
        concat {
            vStackGap "0.15rem" (concat {
                text overline label
                text subtitle1 value
            })
            divider ()
        }