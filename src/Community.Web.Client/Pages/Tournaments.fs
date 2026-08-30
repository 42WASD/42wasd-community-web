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
        | ShareTournament of string

    /// Map a Radzen split-button action value to the message it should emit.
    /// The split button's `Click` fires with the chosen item's `Value` — `None`
    /// for the main button, `Some "toggle"` for the toggle item, `Some
    /// "details"` for "View details". Pure and unit-testable so "View details"
    /// can never accidentally toggle registration.
    let actionMsg (tournamentId: string) (action: string option) =
        match action with
        | None | Some "toggle" -> ToggleRegistration tournamentId
        | Some "details" -> ViewDetails tournamentId
        | Some "share" -> ShareTournament tournamentId
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
        // (avoids duplicating the two blocks). G3: lifecycle actions are
        // OUTLINED — they change state, they don't navigate — so the card
        // wall doesn't read as a stack of primary CTAs (audit #13–15).
        let action =
            RadzenUI.splitButtonVariant label style RadzenUI.outlined (actionMsg tournament.id >> dispatch) (concat {
                RadzenUI.splitButtonItem label "toggle"
                RadzenUI.splitButtonItem "View details" "details"
                RadzenUI.splitButtonItem "Copy link" "share"
            })
        // The status badge — always present so the state is never ambiguous;
        // color derives from registration via Radzen's semantic styles.
        let status =
            if tournament.registrationOpen then
                RadzenUI.badgePill RadzenUI.successBadge "Open"
            else
                RadzenUI.badgePill RadzenUI.dangerBadge "Closed"
        // Countdown badge (42-audit #22) — hoisted per the CE gotcha.
        let countdown =
            if tournament.registrationOpen then
                let span = tournament.startsAt - System.DateTime.Now
                let label =
                    if span.TotalDays >= 1.0 then sprintf "Starts in %dd %dh" (int span.TotalDays) (span.Hours)
                    elif span.TotalHours >= 1.0 then sprintf "Starts in %dh %dm" (int span.TotalHours) (span.Minutes)
                    elif span.TotalMinutes > 0.0 then sprintf "Starts in %dm" (int span.TotalMinutes)
                    else "Starting now"
                RadzenUI.badgePill RadzenUI.infoBadge label
            else empty ()
        // Audit #4 consolidation: ONE fluid-padded flex column per card —
        // title+badge row (name truncates with min-w-0 so long titles never
        // push the badge off-card), a single meta caption line
        // "starts · prize" beneath, and the action pinned to the bottom via
        // mt-auto so buttons align across cards.
        RadzenUI.cardOutlinedClass "flex flex-col h-full" (concat {
            div {
                attr.``class`` "p-[var(--pad-card)] flex flex-col flex-1 gap-[0.25rem] min-w-0"
                concat {
                    RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                        div { attr.title tournament.name
                              RadzenUI.text RadzenUI.heading6 tournament.name }
                        status
                    })
                    // Meta merge (audit #4.2): date + prize as one caption
                    // line under the title instead of a dedicated column.
                    RadzenUI.text RadzenUI.caption
                        (tournament.startsAt.ToString("yyyy-MM-dd HH:mm") + "  ·  " + tournament.prize)
                    countdown
                    // mt-auto pins the action to the card bottom so split
                    // buttons share a baseline across cards (audit #14.3).
                    div {
                        attr.``class`` "mt-auto pt-[var(--gap-grid)]"
                        action
                    }
                }
            }
        })

    /// The Tournaments page view. Selects the canonical cache; renders the
    /// tournaments as a vertical RadzenDataList (`WrapItems=false` = the
    /// `rz-datalist-data` vertical list of full-width cards).
    let view (onReload: unit -> unit) (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.tournaments <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the tournament vertical list (tall cards).
            RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.skeleton "width: 30%; height: 2rem;"
                RadzenUI.skeletonGrid 4 12 12 12 RadzenUI.skeletonCardBody
            })
        | Failed _ ->
            RadzenUI.failedViewRetry "tournaments" onReload
        | Loaded m ->
            RadzenUI.fadeIn (RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.pageHeadingCrumb "Tournaments"
                    (Some "Upcoming competitions — open or closed for registration.")
                    [ ("Home", Some "/"); ("Tournaments", None) ]
                // Status tabs (42-audit #24): Upcoming (registration open)
                // vs Closed — the RadzenTabs wrapper finally has a home.
                RadzenUI.tabs (concat {
                    RadzenUI.tabItem "Upcoming"
                        (RadzenUI.dataList<Tournament>
                            (SharedModel.values m |> Array.filter (fun t -> t.registrationOpen))
                            false (fun t -> card t dispatch))
                    RadzenUI.tabItem "Closed"
                        (RadzenUI.dataList<Tournament>
                            (SharedModel.values m |> Array.filter (fun t -> not t.registrationOpen))
                            false (fun t -> card t dispatch))
                })
            }))