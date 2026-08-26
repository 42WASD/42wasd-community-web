namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Elmish
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Members page — feature-owned view. Selects the shared Players list (the
/// canonical member roster) and renders it as a searchable RadzenDataGrid.
///
/// Holds a single piece of page-local state — the search term typed into the
/// RadzenAutoComplete — which filters the grid's rows. (Phase 9 nested-page
/// messages: Msg composed into the root Message, carried via the route's
/// PageModel, discarded on navigation.)
module Members =

    /// The Members page's transient, page-local state: the live search term.
    type Model =
        {
            search: string
        }

    /// The Members page's local messages.
    type Msg =
        | SetSearch of string

    let init = { search = "" }

    let update msg model =
        match msg with
        | SetSearch s ->
            // Guard the same null PageModel seen in `view` under trimming (see
            // view's `isNull (box model)`): if the router handed us a null
            // model, start from `init` before projecting.
            let model' = if isNull (box model) then init else model
            { model' with search = s }, Cmd.none

    /// The avatar column: username shown as the first grid cell.
    /// NOTE: intentionally NOT using `RadzenGravatar` — its `AlternateText`
    /// getter calls Radzen's `Localize()` → `RadzenStrings.ResourceManager`
    /// lookup at render time, a reflection path AOT/trim strips → the whole
    /// grid throws NullReferenceException on the AOT build. A plain typed
    /// text cell avoids reflection entirely (mirrors the Servers fix). Also
    /// NOT wrapping in a `RadzenStack` — Servers' working grid cells are bare
    /// `RadzenText`, and the 3 NREs on Members = 3 rows implicate the cell
    /// body. Keep it as a single bare text node like Servers.
    let avatarCell (player: Player) =
        RadzenUI.text RadzenUI.body1 player.username

    let view (model: Model) (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.players <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the Members layout: heading + search
            // box + member data-grid.
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 22%; height: 2rem;"
                RadzenUI.skeleton "width: 100%; height: 2.5rem;"
                RadzenUI.cardOutlined (RadzenUI.skeletonTable [ "50%"; "46%" ])
            })
        | Failed _ ->
            RadzenUI.failedView "members"
        | Loaded m ->
            // Defensive null guard: under trimming, the router's PageModel may
            // be constructed with a null Model (the `Unsafe.AsRef` write in
            // `definePageModel` is a reflection-adjacent path that partial trim
            // can drop). The page must still render, so treat a null model as
            // "empty search". Live filtering still works when the model is
            // non-null (the usual non-published case).
            let search =
                if isNull (box model) then "" else model.search
            let query = search.Trim().ToLowerInvariant()
            // Filter the canonical roster by username or Discord handle.
            let players =
                SharedModel.values m
                |> Array.filter (fun p ->
                    query = ""
                    || p.username.ToLowerInvariant().Contains query
                    || (defaultArg p.discord "").ToLowerInvariant().Contains query)
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Members"
                // A search box that suggests usernames and drives the live
                // filter on the roster below.
                RadzenUI.autoComplete
                    (SharedModel.values m)
                    "username" search (fun v -> dispatch (SetSearch v))
                RadzenUI.dataGrid<Player> players (concat {
                    // template columns (NOT dataGridColumn "property") — Radzen's
                    // string-`Property` binding uses runtime reflection that
                    // AOT/trim strips. Typed F# lambdas avoid reflection.
                    RadzenUI.dataGridTemplateColumn<Player> "Member" avatarCell
                    RadzenUI.dataGridTemplateColumn<Player> "Discord" (fun p ->
                        RadzenUI.text RadzenUI.body1 (defaultArg p.discord ""))
                })
            }))