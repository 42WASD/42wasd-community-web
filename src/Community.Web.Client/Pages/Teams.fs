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
        RadzenUI.vStackGap "0.75rem" (concat {
            RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                RadzenUI.text RadzenUI.subtitle1 team.name
                RadzenUI.badgePill RadzenUI.lightBadge (string team.players.Length)
            })
            RadzenUI.divider ()
            for player in team.players do
                RadzenUI.hStackGap "0.5rem" (concat {
                    RadzenUI.gravatar player.discord 24
                    RadzenUI.text RadzenUI.caption player.username
                })
        })

    /// The Teams page view. A responsive RadzenRow of stretch columns:
    /// 12-of-12 on phones, 4-of-12 on desktop — the same 12/6/4 grid shape the
    /// Games page uses, so all card grids in the app share one breakpoint
    /// behavior and every card is equal height within its row.
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the loaded card grid (heading + 12/6/4
            // columns of team-card placeholders).
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 18%; height: 2rem;"
                RadzenUI.skeletonGrid 3 12 6 4 RadzenUI.skeletonTeamBody
            })
        | Failed _ ->
            RadzenUI.failedView "teams"
        | Loaded m ->
            let teams = SharedModel.values m
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.pageHeading "Teams" (Some "Community squads and their rosters.")
                RadzenUI.rowGap "1rem" (forEach teams (fun team ->
                    RadzenUI.columnStretch 12 6 4 (RadzenUI.cardOutlined (teamCard team))))
            }))