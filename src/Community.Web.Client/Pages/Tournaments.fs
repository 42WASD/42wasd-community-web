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

    /// Decide whether a split-button action should toggle registration. The
    /// Radzen split button's `Click` fires with the chosen item's `Value` —
    /// `None` for the main button, `Some "toggle"` for the toggle item, `Some
    /// "details"` for "View details". Pure and unit-testable so "View details"
    /// can never accidentally toggle registration.
    let isToggleAction (action: string option) =
        match action with
        | None | Some "toggle" -> true
        | Some _ -> false

    /// Render one tournament card. The Radzen split button's dropdown has two
    /// actions: close/reopen registration (main button + first item) and
    /// "View details" (a no-op placeholder). Because RadzenSplitButtonItem has
    /// no per-item click, the parent's `Click` receives the item's `Value`.
    let card (tournament: Tournament) (dispatch: Msg -> unit) =
        let run action =
            if isToggleAction action then
                ToggleRegistration tournament.id |> dispatch
        RadzenUI.cardHover (concat {
            RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.heading6 tournament.name
                RadzenUI.text RadzenUI.overline tournament.prize
                RadzenUI.text RadzenUI.caption (tournament.startsAt.ToString("yyyy-MM-dd"))
            })
            if tournament.registrationOpen then
                RadzenUI.splitButton
                    "Close registration"
                    RadzenUI.dangerButton
                    run
                    (concat {
                        RadzenUI.splitButtonItem "Close registration" "toggle"
                        RadzenUI.splitButtonItem "View details" "details"
                    })
            else
                RadzenUI.splitButton
                    "Reopen registration"
                    RadzenUI.successButton
                    run
                    (concat {
                        RadzenUI.splitButtonItem "Reopen registration" "toggle"
                        RadzenUI.splitButtonItem "View details" "details"
                    })
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