namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Teams page — feature-owned view. Selects the canonical Teams cache from
/// Shared and renders each team as a responsive card with its roster. Follows
/// the same shape as the other pages: no page-local Model, loading/loaded/
/// failed handled by pattern-matching the canonical cache. Built on Radzen.
module Teams =

    /// Render one team card: name + its player roster.
    let teamCard (team: Team) =
        RadzenUI.columnResponsive 12 6 4 (concat {
            RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.heading6 team.name
                for player in team.players do
                    let discord = defaultArg player.discord ""
                    RadzenUI.text RadzenUI.caption (player.username + " · " + discord)
            }))
        })

    /// The Teams page view. Card layout (not a table) demonstrating a
    /// different presentation shape while still reading the canonical cache.
    let view (shared: SharedModel) =
        cond shared.teams <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load teams."
        | Loaded m ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Teams"
                RadzenUI.rowGap "1rem" (forEach (Map.toArray m) (fun (_, team) -> teamCard team))
            })