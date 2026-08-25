namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Games page — feature-owned view. Selects the canonical Games cache and the
/// favourite set from Shared; owns no shared data itself. When the user
/// toggles a game's favourite, the page emits a *cross-feature effect*: a
/// local message that the root translates into a `Shared.ToggleFavoriteGame`,
/// which updates the shared favourite set. Home's "favourite games" stat reads
/// the same set and reflects the change.
module Games =

    /// The Games page's local messages. ToggleFavorite is an intent the root
    /// re-interprets as a shared (cross-feature) effect.
    type Msg =
        | ToggleFavorite of string

    /// Render one game row with a favourite toggle button. The button
    /// dispatches a local ToggleFavorite (owned by this feature); the root
    /// turns it into a shared-cache update.
    let row (game: Game) (isFavorite: bool) (dispatch: Msg -> unit) =
        // Phase 15 evidence: probe how often this game is rebuilt. Because the
        // view is a pure function over the normalized map, a favourite toggle
        // re-runs the whole list view; the probe shows every row re-renders.
        // The list is small, so this is cheap — evidence says no component
        // isolation yet (the phase rule: optimize only by evidence).
        let _ = RenderProbe.touch $"game:{game.id}"
        tr {
            td { game.name }
            td { game.genre }
            td { game.description }
            td {
                let buttonClass = if isFavorite then "button is-small is-warning" else "button is-small"
                let buttonText = if isFavorite then "Unfavourite" else "Favourite"
                button {
                    attr.``class`` buttonClass
                    on.click (fun _ -> dispatch (ToggleFavorite game.id))
                    buttonText
                }
            }
        }

    /// The Games page view. Selects the canonical cache and the favourite set.
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        let favorites = shared.favoriteGames
        cond shared.games <| function
        | NotAsked | Loading -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
        | Failed _ -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
        | Loaded m ->
            let rows = forEach (Map.toArray m) (fun (_, g) -> row g (favorites.Contains g.id) dispatch)
            // Phase 15 evidence: report once per page render (after the rows
            // have been built) so each dispatch produces one readable line in
            // the browser console instead of one line per row. It shows how
            // many times each game row was rebuilt during this render pass —
            // the evidence that a single favourite toggle re-renders the whole
            // list (every row re-touches), and that isolating rows would only
            // pay off when the list grows enough to make that measurably slow.
            RenderProbe.report "Games.view"
            Layout.Games().Rows(rows).Elt()