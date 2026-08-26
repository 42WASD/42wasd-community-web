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
        | SetSearch s -> { model with search = s }, Cmd.none

    /// The avatar column: a RadzenGravatar + username shown as the first grid
    /// cell, demonstrating a template column inside the DataGrid.
    let avatarCell (player: Player) =
        RadzenUI.hStackGap "0.5rem" (concat {
            RadzenUI.gravatar player.discord 32
            RadzenUI.text RadzenUI.body1 player.username
        })

    let view (model: Model) (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.players <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load members."
        | Loaded m ->
            let query = model.search.Trim().ToLowerInvariant()
            // Filter the canonical roster by username or Discord handle.
            let players =
                Map.toArray m
                |> Array.map snd
                |> Array.filter (fun p ->
                    query = ""
                    || p.username.ToLowerInvariant().Contains query
                    || (defaultArg p.discord "").ToLowerInvariant().Contains query)
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Members"
                // A search box that suggests usernames and drives the filter.
                RadzenUI.autoComplete
                    (Map.toArray m |> Array.map snd)
                    "username" model.search (fun v -> dispatch (SetSearch v))
                // The filtered roster as a sortable/filterable/paged grid.
                RadzenUI.dataGrid<Player> players (concat {
                    RadzenUI.dataGridTemplateColumn<Player> "Member" avatarCell
                    RadzenUI.dataGridColumn<Player> "username" "Username" true
                    RadzenUI.dataGridTemplateColumn<Player> "Discord" (fun p ->
                        RadzenUI.text RadzenUI.body1 (defaultArg p.discord ""))
                })
            })