namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
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
            Layout.Games()
                .Rows(forEach (Map.toArray m) (fun (_, g) -> row g (favorites.Contains g.id) dispatch))
                .Elt()