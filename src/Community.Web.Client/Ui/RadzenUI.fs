namespace Community.Web.Client.Ui

open System
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
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
        comp<RadzenSidebar> {
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

    /// An outlined RadzenCard with a hover lift + Material ripple. The hover
    /// lift is pure Tailwind (translate + glow), the ripple uses Radzen's own
    /// `rz-ripple` utility. The glow color comes from the Radzen `--rz-primary`
    /// token (via color-mix for alpha), so it tracks the brand accent instead
    /// of a hardcoded cyan.
    let cardHover (children: Node) =
        comp<RadzenCard> {
            "Variant" => outlined
            attr.``class``
                ("rz-ripple cursor-pointer "
                 + "transition-[transform,box-shadow] duration-200 ease-in-out "
                 + "hover:-translate-y-1 "
                 + "hover:shadow-[0_8px_16px_color-mix(in_srgb,var(--rz-primary)_25%,transparent)]")
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
        rowGap "1rem" (concat {
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
    /// Community.Web.Server/Index.fs).
    let fadeIn (children: Node) =
        div { attr.``class`` "animate-fade-in"; children }

    /// The standard page error message for a `Failed` `RemoteData` slice.
    /// `what` is the plain name of the failed resource (e.g. "games").
    let failedView (what: string) =
        text body1 ("Couldn't load " + what + ".")

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

    // ---------------------------------------------------------------- navigation

    /// A RadzenPanelMenu container for panel menu items.
    let panelMenu (children: Node) =
        comp<RadzenPanelMenu> {
            children
        }

    /// A RadzenPanelMenuItem nav link. `matchAll` selects All-match (exact
    /// route); otherwise Prefix-match.
    let panelMenuItem (textValue: string) (path: string) (matchAll: bool) =
        comp<RadzenPanelMenuItem> {
            "Text" => textValue
            "Path" => path
            "Match" => (if matchAll then navMatchAll else navMatchPrefix)
        }

    /// A RadzenPanelMenuItem with an expandable submenu (for a parent group
    /// like "Community"). `children` are nested `panelMenuItem` leaves.
    let panelMenuItemExpandable (textValue: string) (children: Node) =
        comp<RadzenPanelMenuItem> {
            "Text" => textValue
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

    // ---------------------------------------------------------------- forms

    /// A RadzenTextBox bound to a value via `ValueChanged`.
    let textBox (value: string) (onChange: string -> unit) =
        comp<RadzenTextBox> {
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    /// A RadzenTextArea bound to a multiline value.
    let textArea (value: string) (onChange: string -> unit) =
        comp<RadzenTextArea> {
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
            attr.callback "ValueChanged" (fun (v: string) -> onValueChanged v)
            // RadzenAutoComplete's ValueChanged binds to the `onchange` event
            // (fires only on Enter/blur/select). For LIVE filtering as you type,
            // capture the bubbling `input` event on the wrapper element.
            attr.callback "oninput" (fun (e: Microsoft.AspNetCore.Components.ChangeEventArgs) ->
                let v = if isNull e.Value then "" else string e.Value
                onValueChanged v)
        }

    /// A RadzenLogin — a ready-made sign-in form (username/password fields with
    /// built-in required validation). The `onLogin` callback receives the
    /// submitted `(username, password)`.
    let login (onLogin: string * string -> unit) =
        comp<RadzenLogin> {
            "AllowRegister" => false
            "AllowResetPassword" => false
            attr.callback "Login" (fun (args: LoginArgs) ->
                let user = if isNull args.Username then "" else args.Username
                let pass = if isNull args.Password then "" else args.Password
                onLogin (user, pass))
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

    // ---------------------------------------------------------------- profile menu

    /// A RadzenProfileMenu — a collapsed nav item that expands into a dropdown
    /// of `profileMenuItem`s. NOTE: items have no per-item click — item clicks
    /// bubble to the parent's `Click` with the item carrying its `Value`, so
    /// route on `item.Value`. `template` is the always-visible trigger content
    /// (e.g. the signed-in username).
    let profileMenu (template: Node) (onClick: string -> unit) (children: Node) =
        comp<RadzenProfileMenu> {
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

    // ---------------------------------------------------------------- progress

    /// A determinate RadzenProgressBarCircular — a compact ring showing
    /// `value`/`max` with the value inside the circle. `size` is one of the
    /// `circular*` enums; `showValue` displays the percentage in the center.
    let progressBarCircular (value: float) (max: float) (size: ProgressBarCircularSize) (showValue: bool) (style: ProgressBarStyle) =
        comp<RadzenProgressBarCircular> {
            "Value" => value
            "Max" => max
            "Size" => size
            "ShowValue" => showValue
            "ProgressBarStyle" => style
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

    // ---------------------------------------------------------------- carousel

    /// A RadzenCarousel cycling through `carouselItem` children. `itemsPerPage`
    /// controls how many are visible at once on large screens. Items are passed
    /// via the `Items` render-fragment (see `fragmentParam`).
    let carousel (itemsPerPage: int) (children: Node) =
        comp<RadzenCarousel> {
            "ItemsPerPage" => itemsPerPage
            "PagerPosition" => pagerBottom
            "PagerOverlay" => false
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
    let image (src: string) (alt: string) =
        comp<RadzenImage> {
            "Path" => src
            "AlternateText" => alt
            "Style" => "width: 100%; height: 9rem; object-fit: cover; display: block;"
        }

    /// A RadzenChip label with a badge-style color and an optional fill variant.
    let chip (textValue: string) (style: BadgeStyle) =
        comp<RadzenChip> {
            "Text" => textValue
            "ChipStyle" => style
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
        vStackGap "0.15rem" (concat {
            text overline label
            text body1 value
        })