namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Teams page — feature-owned view. Selects the canonical Teams cache from
/// Shared and renders it as a RadzenDataList — a card list rather than a
/// table, demonstrating a different presentation shape. Follows the same
/// loading/loaded/failed pattern as the other pages, with no page-local state.
module Teams =

    /// Render one team as a card: the team name header, a member count badge,
    /// and the player roster each shown with their gravatar avatar + Discord
    /// handle. The card auto-sizes to its roster.
    let teamCard (team: Team) =
        RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.subtitle1 team.name
                RadzenUI.badgePill RadzenUI.lightBadge (string team.players.Length)
            })
            for player in team.players do
                RadzenUI.hStackGap "0.5rem" (concat {
                    RadzenUI.gravatar player.discord 24
                    RadzenUI.text RadzenUI.caption player.username
                })
        })

    /// The Teams page view. A RadzenSplitter divides the width between the
    /// team cards; each card is a pane that auto-sizes to its roster, so even
    /// when teams have different member counts the panes balance out into a
    /// dynamic, resizable dashboard (drag the dividers to rebalance).
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the teams layout: heading + a splitter
            // (side-by-side team cards). Two panes with team-card skeletons.
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 18%; height: 2rem;"
                RadzenUI.splitter "height: 420px;" (concat {
                    RadzenUI.splitterPane None (RadzenUI.cardOutlined (RadzenUI.skeletonTeamBody ()))
                    RadzenUI.splitterPane None (RadzenUI.cardOutlined (RadzenUI.skeletonTeamBody ()))
                })
            })
        | Failed _ ->
            RadzenUI.failedView "teams"
        | Loaded m ->
            let teams = SharedModel.values m
            let paneSize = (string (100.0 / float teams.Length)) + "%"
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Teams"
                RadzenUI.splitter "height: 420px;" (concat {
                    for team in teams do
                        RadzenUI.splitterPane (Some paneSize) (concat {
                            RadzenUI.cardOutlined (teamCard team)
                        })
                })
            }))