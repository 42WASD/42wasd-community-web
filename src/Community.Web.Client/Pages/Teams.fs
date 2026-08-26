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

    /// The Teams page view. A `RadzenTileLayout` lays the team cards out as a
    /// uniform, icon-led grid of tiles (each with the roster's gravatars),
    /// read from the canonical teams cache. Tiles are laid out statically in
    /// two columns of two rows.
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
                RadzenUI.tileLayout 6 (concat {
                    for (idx, team) in teams |> Array.indexed do
                        // Two per row (each spans 3 of 6 columns); row grows
                        // by one grid row per pair.
                        RadzenUI.tileLayoutItem
                            team.name
                            "groups"
                            ((idx % 2) * 3 + 1)
                            (idx / 2 + 1)
                            3
                            (teamCard team)
                })
            })