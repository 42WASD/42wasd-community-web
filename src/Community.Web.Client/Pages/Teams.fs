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

    /// Render one team as a DataList card: name + its player roster.
    let teamCard (team: Team) =
        RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.text RadzenUI.heading6 team.name
            for player in team.players do
                let discord = defaultArg player.discord ""
                RadzenUI.text RadzenUI.caption (player.username + " · " + discord)
        }))

    /// The Teams page view. A `RadzenDataList` lays the team cards out as a
    /// responsive card grid (not table rows), while still reading the
    /// canonical teams cache.
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load teams."
        | Loaded m ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Teams"
                RadzenUI.dataList<Team>
                    (Map.toArray m |> Array.map snd)
                    true
                    teamCard
            })