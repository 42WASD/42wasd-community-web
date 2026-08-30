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
    /// leave ragged bottoms. The card body uses the shared `mediaCard`
    /// wrapper (uniform banner box + padded meta section) so the Games grid
    /// and the Home featured-games carousel are pixel-identical.
    let gameCard (game: Game) (isFavorite: bool) (dispatch: Msg -> unit) =
        // Phase 15 evidence: probe how often this game is rebuilt.
        RenderProbe.touch $"game:{game.id}"
        RadzenUI.columnStretch 12 6 4 (concat {
            RadzenUI.cardHover (RadzenUI.mediaCard game.imageUrl game.name (concat {
                RadzenUI.text RadzenUI.heading6 game.name
                RadzenUI.chip game.genre RadzenUI.primaryBadge
                RadzenUI.text RadzenUI.body2 game.description
                // mt-auto pushes the CTA to the card bottom so buttons align
                // across the row even when blurbs differ in length (audit #2).
                // ToggleButton (42-switches #5) owns the on/off state
                // appearance; full-width via Style.
                div {
                    attr.``class`` "mt-auto w-full"
                    RadzenUI.toggleButtonWide
                        "Favourite" "Unfavourite" isFavorite
                        (fun _v -> dispatch (ToggleFavorite game.id))
                }
            }))
        })

    /// The Games page view. Selects the canonical cache and the favourite set.
    /// Interactive genre-filter chip row (audit #27): RadzenChip
    /// Selected/Click — an "All" chip plus one per distinct genre. Clicking a
    /// genre filters the grid; clicking the active genre returns to All.
    let genreFilter (games: Game seq) (selected: string option) (dispatch: string option -> unit) =
        let genres =
            games
            |> Seq.map (fun g -> g.genre)
            |> Seq.distinct
            |> Seq.sort
            |> List.ofSeq
        let chipFor (label: string) (value: string option) =
            let isActive = selected = value
            RadzenUI.chipSelectable label RadzenUI.primaryBadge isActive
                (fun () -> dispatch (if isActive then None else value))
        // Wrapping row (42-switches #16): Stack default is NoWrap — many
        // genres overflowed 390px. w-full + flex-wrap via Tailwind utilities
        // (RadzenStack Wrap is available but we already have Tailwind here).
        div {
            attr.``class`` "flex flex-wrap gap-2 w-full"
            concat {
                chipFor "All" None
                for g in genres do
                    chipFor g (Some g)
            }
        }

    let view (genre: string option) (search: string) (sortKey: string)
             (onGenre: string option -> unit) (onSearch: string -> unit) (onSort: string -> unit)
             (onReload: unit -> unit) (shared: SharedModel) (dispatch: Msg -> unit) =
        let favorites = shared.favoriteGames
        cond shared.games <| function
        | NotAsked | Loading ->
            // Dynamic skeleton: mirrors the real layout (a responsive grid of
            // game cards at 12/6/4 breakpoints) so the swap to loaded content
            // changes only the placeholder→live detail, never the structure.
            RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.skeleton "width: 25%; height: 2rem;"
                RadzenUI.skeletonGrid 6 12 6 4 RadzenUI.skeletonCardBody
            })
        | Failed _ ->
            RadzenUI.failedViewRetry "games" onReload
        | Loaded m ->
            let all = SharedModel.values m
            let filtered =
                all
                |> Array.filter (fun x ->
                    (match genre with Some g -> x.genre = g | None -> true)
                    && (search = ""
                        || x.name.ToLowerInvariant().Contains (search.ToLowerInvariant())
                        || x.genre.ToLowerInvariant().Contains (search.ToLowerInvariant())))
                // Sort (42-audit #14): name | players(favourites) — favourites
                // sort uses the shared set via count heuristic (id presence).
                |> fun arr ->
                    match sortKey with
                    | "favourites" -> arr |> Array.sortBy (fun x -> if favorites.Contains x.id then 0 else 1)
                    | _ -> arr |> Array.sortBy (fun x -> x.name)
            let resultCount = Array.length filtered
            let rows =
                if Array.isEmpty filtered then
                    // Empty-filter state (42-audit #16): message + clear button
                    concat {
                        RadzenUI.alert RadzenUI.infoAlert "No games match this filter."
                        RadzenUI.buttonAction "Clear filters" RadzenUI.lightButton
                            (fun () -> onGenre None)
                    }
                else
                    forEach filtered (fun g -> gameCard g (favorites.Contains g.id) dispatch)
            // Phase 15 evidence: report once per page render.
            RenderProbe.report "Games.view"
            RadzenUI.fadeIn (RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.pageHeadingCrumb "Games" None [ ("Home", Some "/"); ("Games", None) ]
                // Toolbar (42-audit #13/#14/#15): live search + sort segments
                // + result count badge.
                RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                    RadzenUI.searchBox search onSearch
                    RadzenUI.selectBar<string>
                        (Some sortKey)
                        (fun v -> onSort v)
                        false
                        (concat {
                            RadzenUI.selectBarItem "Name" "name" (Some "sort_by_alpha")
                            RadzenUI.selectBarItem "Favourites" "favourites" (Some "favorite")
                        })
                    RadzenUI.badgePill RadzenUI.infoBadge (sprintf "%d games" resultCount)
                })
                genreFilter all genre onGenre
                RadzenUI.rowGap "var(--gap-grid)" rows
            }))