namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Radzen
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Tournaments page — feature-owned view. Selects the canonical Tournaments
/// cache from Shared. It owns no shared data itself; when the user toggles a
/// tournament's registration, the page emits a *cross-feature effect*: a local
/// message that the root translates into a `Shared.ToggleTournament` message,
/// which mutates the canonical cache. Home's "open tournaments" stat reads
/// that same cache, so it reflects the change — the reference's cross-feature
/// verification (an action on one page updates shared state another page sees).
module Tournaments =

    /// The Tournaments page's local messages. ToggleRegistration is an intent
    /// that the root re-interprets as a shared (cross-feature) effect.
    type Msg =
        | ToggleRegistration of string

    /// Render one tournament card. The Radzen button dispatches a local
    /// ToggleRegistration (owned by this feature); the root turns it into a
    /// shared-cache update.
    let card (tournament: Tournament) (dispatch: Msg -> unit) =
        RadzenUI.card Variant.Outlined (concat {
            div {
                attr.``class`` "tournament-card"
                h3 { attr.``class`` "title is-5"; tournament.name }
                div { attr.``class`` "tournament-prize"; tournament.prize }
                div { attr.``class`` "tournament-date"; tournament.startsAt.ToString("yyyy-MM-dd") }
                if tournament.registrationOpen then
                    RadzenUI.button "Close registration" ButtonStyle.Danger (fun () -> ToggleRegistration tournament.id) dispatch
                else
                    RadzenUI.button "Reopen registration" ButtonStyle.Success (fun () -> ToggleRegistration tournament.id) dispatch
            }
        })

    /// The Tournaments page view. Selects the canonical cache; if it isn't
    /// loaded yet, falls back to the shared loading/empty row.
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.tournaments <| function
        | NotAsked | Loading -> Layout.Tournaments().Rows(Layout.EmptyData().Elt()).Elt()
        | Failed _ -> Layout.Tournaments().Rows(Layout.EmptyData().Elt()).Elt()
        | Loaded m ->
            Layout.Tournaments()
                .Rows(forEach (Map.toArray m) (fun (_, t) -> card t dispatch))
                .Elt()