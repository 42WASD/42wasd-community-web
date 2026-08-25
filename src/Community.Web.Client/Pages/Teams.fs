namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Teams page — feature-owned view. Selects the canonical Teams cache from
/// Shared and renders each team as a card with its roster. Follows the same
/// shape as the other pages: no page-local Model (list-only, module-level
/// view), loading/loaded/failed handled by `dataRows`.
module Teams =

    /// Render one team card: name + its player roster.
    let teamCard (team: Team) =
        div {
            attr.``class`` "box"
            h3 {
                attr.``class`` "title is-5"
                team.name
            }
            ul {
                for player in team.players do
                    let discord = defaultArg player.discord ""
                    li {
                        player.username
                        text " "
                        text discord
                    }
            }
        }

    /// The Teams page view. Card layout (not a table) to demonstrate a
    /// different presentation shape while still reading the canonical cache.
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading -> Layout.EmptyData().Elt()
        | Failed _ -> Layout.EmptyData().Elt()
        | Loaded m ->
            div {
                h1 { attr.``class`` "title" ; text "Teams" }
                forEach (Map.toArray m) (fun (_, team) -> teamCard team)
            }