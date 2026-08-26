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

    /// Render one team as a tile: a header with a team icon, and the player
    /// roster each shown with their gravatar avatar + Discord handle. Placed
    /// into the shared `RadzenTileLayout` grid by `tileRow`/`tileCol`.
    let teamCard (team: Team) =
        RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.subtitle2 (string team.players.Length)
                RadzenUI.text RadzenUI.caption "members"
            })
            for player in team.players do
                RadzenUI.hStackGap "0.5rem" (concat {
                    RadzenUI.gravatar player.discord 24
                    RadzenUI.text RadzenUI.caption player.username
                })
        })

    /// The Teams page view. A responsive Radzen 12-col grid lays the team cards
    /// out so each card auto-sizes to its roster (no fixed tile height, so no
    /// clipped members / inner scroll). Cards flow 2-per-row on small screens
    /// and up to 3-per-row on desktop, with non-uniform heights looking like a
    /// dashboard.
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load teams."
        | Loaded m ->
            let teams = Map.toArray m |> Array.map snd
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Teams"
                RadzenUI.rowGap "1rem" (concat {
                    for team in teams do
                        RadzenUI.columnResponsive 6 6 4 (concat {
                            RadzenUI.cardOutlined (teamCard team)
                        })
                })
            })