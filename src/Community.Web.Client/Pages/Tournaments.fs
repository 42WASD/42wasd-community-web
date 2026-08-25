namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
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
        RadzenUI.cardOutlined (concat {
            RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.heading6 tournament.name
                RadzenUI.text RadzenUI.overline tournament.prize
                RadzenUI.text RadzenUI.caption (tournament.startsAt.ToString("yyyy-MM-dd"))
            })
            if tournament.registrationOpen then
                RadzenUI.button "Close registration" RadzenUI.dangerButton (fun () -> ToggleRegistration tournament.id) dispatch
            else
                RadzenUI.button "Reopen registration" RadzenUI.successButton (fun () -> ToggleRegistration tournament.id) dispatch
        })

    /// The Tournaments page view. Selects the canonical cache; renders a
    /// responsive row of tournament cards.
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.tournaments <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load tournaments."
        | Loaded m ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Tournaments"
                RadzenUI.rowGap "1rem" (forEach (Map.toArray m) (fun (_, t) ->
                    RadzenUI.columnResponsive 12 6 4 (card t dispatch)))
            })