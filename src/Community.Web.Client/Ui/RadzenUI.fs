namespace Community.Web.Client.Ui

open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components.Web
open Radzen
open Radzen.Blazor
open Community.Web.Client.State
open Community.Web.Shared.Domain

/// Thin F# wrappers around Radzen Blazor components, per design.md. Keeping
/// the component interop in one module keeps page views declarative and
/// oblivious to Blazor's object-oriented mechanics: each wrapper takes an
/// Elmish `Msg` (or a value -> Msg) and a dispatch, and maps the Blazor
/// EventCallback to `dispatch`.
///
/// The wrappers are intentionally small and stay on top of the architecture —
/// they never touch state ownership. Radzen services (Dialog/Notification)
/// are registered on the host (Startup.fs) and, when a wrapper needs a side
/// effect, the caller emits an Elmish Msg that an update loop turns into an
/// async Cmd (per design.md section 4). Views themselves remain pure.
module RadzenUI =

    /// A Radzen button. `style` controls the semantic color (Primary/Success/
    /// Danger/...), `onClickMsg` is the Elmish message to dispatch on click.
    let button (text: string) (style: ButtonStyle) (onClickMsg: unit -> 'Msg) (dispatch: 'Msg -> unit) =
        comp<RadzenButton> {
            "Text" => text
            "ButtonStyle" => style
            attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClickMsg ()))
        }

    /// A Radzen card. Wraps child `Node`s into the component's ChildContent.
    /// Used for the gaming-community surfaces (server / tournament cards).
    let card (variant: Variant) (children: Node) =
        comp<RadzenCard> {
            "Variant" => variant
            children
        }

    /// A server-status card (gaming-community direction). A Radzen card with
    /// the brutalist `.server-card` surface and a status dot.
    /// `status` is the raw domain value ("online" | "maintenance" | "offline").
    let serverCard (server: GameServer) =
        let statusDot =
            match server.status with
            | "online" -> "status-dot status-online"
            | "maintenance" -> "status-dot status-maintenance"
            | _ -> "status-dot status-offline"
        card Variant.Outlined (concat {
            div {
                attr.``class`` "server-card"
                div { attr.``class`` "server-name"; server.name }
                div {
                    attr.``class`` "server-address"
                    server.address
                    text "  ·  "
                    text $"{server.onlinePlayers}/{server.maxPlayers} online"
                }
                div {
                    span { attr.``class`` statusDot }
                    text server.status
                }
            }
        })

    /// A tournament card (gaming-community direction): a Radzen card with the
    /// name, prize in neon-cyan mono, and the start date.
    let tournamentCard (tournament: Tournament) =
        card Variant.Outlined (concat {
            div {
                attr.``class`` "tournament-card"
                h3 { attr.``class`` "title is-5"; tournament.name }
                div { attr.``class`` "tournament-prize"; tournament.prize }
                div { attr.``class`` "tournament-date"; tournament.startsAt.ToString("yyyy-MM-dd") }
            }
        })