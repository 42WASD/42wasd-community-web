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
    /// that the root re-interprets as a shared (cross-feature) effect;
    /// ViewDetails opens the tournament's detail dialog (an imperative UI
    /// effect, also interpreted by the root — see Main.fs).
    type Msg =
        | ToggleRegistration of string
        | ViewDetails of string

    /// Map a Radzen split-button action value to the message it should emit.
    /// The split button's `Click` fires with the chosen item's `Value` — `None`
    /// for the main button, `Some "toggle"` for the toggle item, `Some
    /// "details"` for "View details". Pure and unit-testable so "View details"
    /// can never accidentally toggle registration.
    let actionMsg (tournamentId: string) (action: string option) =
        match action with
        | None | Some "toggle" -> ToggleRegistration tournamentId
        | Some "details" -> ViewDetails tournamentId
        | Some _ -> ToggleRegistration tournamentId

    /// Render one tournament as a card in the RadzenDataList. The card body
    /// shows name / prize / start date; a split button offers toggling
    /// registration and viewing details.
    let card (tournament: Tournament) (dispatch: Msg -> unit) =
        RadzenUI.cardOutlined (concat {
            RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.heading6 tournament.name
                RadzenUI.text RadzenUI.overline tournament.prize
                RadzenUI.text RadzenUI.caption (tournament.startsAt.ToString("yyyy-MM-dd"))
            })
            // A single split-button definition whose label/style depend on
            // registration state — avoids duplicating the two blocks.
            let label, style =
                if tournament.registrationOpen then "Close registration", RadzenUI.dangerButton
                else "Reopen registration", RadzenUI.successButton
            RadzenUI.splitButton label style (actionMsg tournament.id >> dispatch) (concat {
                RadzenUI.splitButtonItem label "toggle"
                RadzenUI.splitButtonItem "View details" "details"
            })
        })

    /// The Tournaments page view. Selects the canonical cache; renders the
    /// tournaments as a RadzenDataList of cards (wrap-items flow = responsive
    /// multi-column, like the previous 12/6/4 grid).
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.tournaments <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the tournament grid (12/6/4 cards).
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 30%; height: 2rem;"
                RadzenUI.skeletonGrid 6 12 6 4 RadzenUI.skeletonCardBody
            })
        | Failed _ ->
            RadzenUI.failedView "tournaments"
        | Loaded m ->
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Tournaments"
                RadzenUI.dataList<Tournament> (SharedModel.values m) true (fun t -> card t dispatch)
            }))