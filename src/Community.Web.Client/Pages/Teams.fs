namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Teams page — feature-owned view. Selects the canonical Teams cache from
/// Shared and renders it as a responsive card grid — one team card per row on
/// phones, three equal-height cards on desktop, via the Radzen 12-col grid.
/// (A RadzenSplitter was tried here: its panes cannot stack on small screens,
/// leaving ~125px-wide columns with sideways drag bars — unusable on phones.)
/// Follows the same loading/loaded/failed pattern as the other pages, with no
/// page-local state.
module Teams =

    /// Render one team as a card: the team name header (with the member
    /// count badge pushed to the card's right edge via justifyBetween) and
    /// the player roster each shown with their gravatar avatar + Discord
    /// handle. The card auto-sizes to its roster.
    let teamCard (team: Team) =
        // MOBILE MISALIGNMENT FIX (user report): RadzenCard shrink-wraps to
        // content, and `columnStretch`'s flex column only stretches HEIGHT —
        // the card sat at its content width (178–217px, ragged). Same fix as
        // Games' cardHover: the card must be w-full so every card in the row
        // fills its column. The roster row also gets min-w-0 + truncate so a
        // long username can't push the card wider than the column.
        RadzenUI.cardOutlinedClass "w-full" (
            RadzenUI.vStackGap "0.75rem" (concat {
                RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                    RadzenUI.text RadzenUI.subtitle1 team.name
                    RadzenUI.badgePill RadzenUI.lightBadge (string team.players.Length)
                })
                RadzenUI.divider ()
                // Avatar stack (42-audit #31): overlapping initials avatars
                // summarize the roster; the full list lives in a collapsible
                // fieldset (42-audit #32) so long rosters don't bloat the card.
                div {
                    attr.``class`` "flex -space-x-2"
                    concat {
                        for player in team.players |> Array.truncate 5 do
                            div { attr.``class`` "ring-2 ring-[var(--rz-card-background-color)] rounded-full"
                                  RadzenUI.initialsAvatar player.username }
                        if team.players.Length > 5 then
                            let moreClass =
                                "h-9 w-9 rounded-full grid place-items-center text-[0.75rem] "
                                + "bg-[var(--rz-base-600,#2A2A2A)] text-[var(--rz-text-color)] "
                                + "ring-2 ring-[var(--rz-card-background-color)]"
                            div {
                                attr.``class`` moreClass
                                RadzenUI.text RadzenUI.caption ("+" + string (team.players.Length - 5))
                            }
                    }
                }
                RadzenUI.fieldset ("Roster (" + string team.players.Length + ")") true
                    (RadzenUI.vStackGap "0.75rem" (concat {
                        for player in team.players do
                            RadzenUI.hStackGap "0.75rem" (concat {
                                RadzenUI.initialsAvatar player.username
                                RadzenUI.text RadzenUI.caption player.username
                            })
                    }))
            }))

    /// The Teams page view. A responsive RadzenRow of stretch columns:
    /// 12-of-12 on phones, 4-of-12 on desktop — the same 12/6/4 grid shape the
    /// Games page uses, so all card grids in the app share one breakpoint
    /// behavior and every card is equal height within its row.
    let view (onReload: unit -> unit) (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the loaded card grid (heading + 12/6/4
            // columns of team-card placeholders).
            RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.skeleton "width: 18%; height: 2rem;"
                RadzenUI.skeletonGrid 3 12 6 4 RadzenUI.skeletonTeamBody
            })
        | Failed _ ->
            RadzenUI.failedViewRetry "teams" onReload
        | Loaded m ->
            let teams = SharedModel.values m
            RadzenUI.fadeIn (RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.pageHeadingCrumb "Teams" (Some "Community squads and their rosters.")
                    [ ("Home", Some "/"); ("Community", None); ("Teams", None) ]
                RadzenUI.rowGap "var(--gap-grid)" (forEach teams (fun team ->
                    RadzenUI.columnStretch 12 6 4 (teamCard team)))
            }))