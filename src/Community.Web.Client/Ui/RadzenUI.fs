namespace Community.Web.Client.Ui

open System
open Bolero
open Bolero.Html
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

    /// A responsive RadzenSidebar. Inside a RadzenLayout it auto-collapses
    /// below 768px. Uncontrolled form (sidebar manages its own state).
    let sidebar (children: Node) =
        comp<RadzenSidebar> {
            children
        }

    /// A RadzenSidebarToggle — the hamburger that toggles the sidebar.
    let sidebarToggle (onToggle: unit -> unit) =
        comp<RadzenSidebarToggle> {
            attr.callback "Click" (fun (_: EventArgs) -> onToggle ())
        }

    /// A RadzenRow — a responsive flex row in the 12-column grid.
    let row (children: Node) =
        comp<RadzenRow> {
            children
        }

    /// A responsive RadzenRow with a gap.
    let rowGap (gap: string) (children: Node) =
        comp<RadzenRow> {
            "Gap" => gap
            children
        }

    /// A RadzenColumn of a given grid width (1-12). Use inside a `row`.
    let column (size: int) (children: Node) =
        comp<RadzenColumn> {
            "Size" => size
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

    /// A RadzenBadge label.
    let badge (style: BadgeStyle) (textValue: string) =
        comp<RadzenBadge> {
            "Text" => textValue
            "BadgeStyle" => style
        }

    /// A pill-shaped RadzenBadge.
    let badgePill (style: BadgeStyle) (textValue: string) =
        comp<RadzenBadge> {
            "Text" => textValue
            "BadgeStyle" => style
            "IsPill" => true
        }

    /// A RadzenSkeleton loading placeholder.
    let skeleton () =
        comp<RadzenSkeleton> {
            "Animation" => skeletonPulse
        }

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

    /// A RadzenPassword bound to a value via `ValueChanged`.
    let password (value: string) (onChange: string -> unit) =
        comp<RadzenPassword> {
            "Value" => value
            attr.callback "ValueChanged" (fun (v: string) -> onChange v)
        }

    // ---------------------------------------------------------------- cards

    /// A server-status card (gaming-community direction).
    let serverCard (server: GameServer) =
        let statusBadge =
            match server.status with
            | "online" -> badgePill successBadge "online"
            | "maintenance" -> badgePill warningBadge "maintenance"
            | _ -> badgePill darkBadge "offline"
        cardOutlined (vStackGap "0.25rem" (concat {
            text heading6 server.name
            text caption $"{server.address}  ·  {server.onlinePlayers}/{server.maxPlayers} online"
            statusBadge
        }))

    /// A tournament card (gaming-community).
    let tournamentCard (tournament: Tournament) =
        cardOutlined (vStackGap "0.5rem" (concat {
            text heading6 tournament.name
            text overline tournament.prize
            text caption (tournament.startsAt.ToString("yyyy-MM-dd"))
        }))