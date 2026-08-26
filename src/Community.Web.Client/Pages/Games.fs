namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Games page — feature-owned view. Selects the canonical Games cache and the
/// favourite set from Shared; owns no shared data itself. When the user
/// toggles a game's favourite, the page emits a *cross-feature effect*: a
/// local message that the root translates into a `Shared.ToggleFavoriteGame`,
/// which updates the shared favourite set.
///
/// Built on Radzen primitives (Phase 17b): each game renders as a responsive
/// card with its info and a favourite toggle button. No Bulma tables remain.
module Games =

    /// The Games page's local messages.
    type Msg =
        | ToggleFavorite of string

    /// Render one game card with a favourite toggle button. The button
    /// dispatches a local ToggleFavorite (owned by this feature). Wrapped in a
    /// stretch column so cards in a row align to equal height at every
    /// breakpoint; the card itself fills the column so descriptions don't
    /// leave ragged bottoms.
    let gameCard (game: Game) (isFavorite: bool) (dispatch: Msg -> unit) =
        // Phase 15 evidence: probe how often this game is rebuilt.
        RenderProbe.touch $"game:{game.id}"
        RadzenUI.columnStretch 12 6 4 (concat {
            RadzenUI.cardHover (RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.image game.imageUrl game.name
                RadzenUI.text RadzenUI.heading6 game.name
                RadzenUI.chip game.genre RadzenUI.primaryBadge
                RadzenUI.text RadzenUI.body2 game.description
                RadzenUI.button
                    (if isFavorite then "Unfavourite" else "Favourite")
                    (if isFavorite then RadzenUI.lightButton else RadzenUI.primaryButton)
                    (fun () -> ToggleFavorite game.id)
                    dispatch
            }))
        })

    /// The Games page view. Selects the canonical cache and the favourite set.
    let view (shared: SharedModel) (dispatch: Msg -> unit) =
        let favorites = shared.favoriteGames
        cond shared.games <| function
        | NotAsked | Loading ->
            // Dynamic skeleton: mirrors the real layout (a responsive grid of
            // game cards at 12/6/4 breakpoints) so the swap to loaded content
            // changes only the placeholder→live detail, never the structure.
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 25%; height: 2rem;"
                RadzenUI.skeletonGrid 6 12 6 4 RadzenUI.skeletonCardBody
            })
        | Failed _ ->
            RadzenUI.failedView "games"
        | Loaded m ->
            let rows = forEach (SharedModel.values m) (fun g -> gameCard g (favorites.Contains g.id) dispatch)
            // Phase 15 evidence: report once per page render.
            RenderProbe.report "Games.view"
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.rise (RadzenUI.text RadzenUI.display3 "Games")
                RadzenUI.rowGap "1rem" rows
            }))