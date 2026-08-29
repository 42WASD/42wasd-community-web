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

    /// Render one tournament as a vertical card in the RadzenDataList. Each
    /// item follows the official Radzen DataList demo: a `rz-p-0` outlined
    /// card whose inner columns own the gutter padding (so the card interior
    /// is evenly separated), a responsive RadzenRow of RadzenColumns for the
    /// layout, and the status + prize + date clearly placed. The action
    /// (split button) sits in its own column, right-aligned, and never looks
    /// "out of place" because it has stable gutter spacing.
    let card (tournament: Tournament) (dispatch: Msg -> unit) =
        let label, style =
            if tournament.registrationOpen then "Close registration", RadzenUI.dangerButton
            else "Reopen registration", RadzenUI.successButton
        // A single split-button whose label/style depend on registration state
        // (avoids duplicating the two blocks).
        let action =
            RadzenUI.splitButton label style (actionMsg tournament.id >> dispatch) (concat {
                RadzenUI.splitButtonItem label "toggle"
                RadzenUI.splitButtonItem "View details" "details"
            })
        // The status badge — always present so the state is never ambiguous;
        // color derives from registration via Radzen's semantic styles.
        let status =
            if tournament.registrationOpen then
                RadzenUI.badgePill RadzenUI.successBadge "Open"
            else
                RadzenUI.badgePill RadzenUI.dangerBadge "Closed"
        RadzenUI.cardOutlinedClass "rz-p-0" (RadzenUI.rowGapAlign "0" RadzenUI.alignStretch RadzenUI.justifyStart (concat {
            // Title + status badge block: on mobile full width, then 6/12ths.
            RadzenUI.columnResponsiveClass 12 7 6 "rz-p-4 rz-display-flex rz-align-items-center" (concat {
                RadzenUI.vStackGap "0.25rem" (concat {
                    RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                        RadzenUI.text RadzenUI.heading6 tournament.name
                        status
                    })
                    // Start date as a caption under the title — the secondary
                    // text reads naturally beneath the primary name.
                    RadzenUI.text RadzenUI.caption (tournament.startsAt.ToString("yyyy-MM-dd HH:mm"))
                })
            })
            // Prize — a clearly-labelled value column with equal height.
            RadzenUI.columnResponsiveClass 12 5 3 "rz-p-4 rz-display-flex rz-align-items-center" (concat {
                RadzenUI.vStackGap "0.25rem" (concat {
                    RadzenUI.text RadzenUI.overline "Prize"
                    RadzenUI.text RadzenUI.subtitle1 tournament.prize
                })
            })
            // Action column: full-width split button so the mobile touch
            // target spans the card (right-aligned only on wide screens).
            RadzenUI.columnResponsiveClass 12 12 3 "rz-p-4 rz-display-flex rz-align-items-center rz-justify-content-end" action
        }))

    /// The Tournaments page view. Selects the canonical cache; renders the
    /// tournaments as a vertical RadzenDataList (`WrapItems=false` = the
    /// `rz-datalist-data` vertical list of full-width cards).
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.tournaments <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the tournament vertical list (tall cards).
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 30%; height: 2rem;"
                RadzenUI.skeletonGrid 4 12 12 12 RadzenUI.skeletonCardBody
            })
        | Failed _ ->
            RadzenUI.failedView "tournaments"
        | Loaded m ->
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.pageHeading "Tournaments" (Some "Upcoming competitions — open or closed for registration.")
                RadzenUI.dataList<Tournament> (SharedModel.values m) false (fun t -> card t dispatch)
            }))