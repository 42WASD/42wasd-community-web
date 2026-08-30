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
/// PageModel, discarded on navigation.) The PageModel write-back works in
/// production thanks to the vendored Bolero SetModel fix (thirdparty/Bolero
/// Router.fs: Unsafe.AsRef no-opped under trimmed WASM).
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
        // Audit #5: member row = initials avatar + username (NOT a bare text
        // stack). Same initials-avatar primitive as Teams rosters.
        RadzenUI.hStackGap "0.75rem" (concat {
            RadzenUI.initialsAvatar player.username
            RadzenUI.text RadzenUI.body1 player.username
        })

    let view
        (onReload: unit -> unit) (onMemberDetail: string -> unit)
        (model: Model) (shared: SharedModel) (dispatch: Msg -> unit) =
        cond shared.players <| function
        | NotAsked | Loading ->
            // Dynamic skeleton mirrors the Members layout: heading + search
            // box + member data-grid.
            RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.skeleton "width: 22%; height: 2rem;"
                RadzenUI.skeleton "width: 100%; height: 2.5rem;"
                RadzenUI.cardOutlined (RadzenUI.skeletonTable [ "50%"; "46%" ])
            })
        | Failed _ ->
            RadzenUI.failedViewRetry "members" onReload
        | Loaded m ->
            // Defensive null guard: under SSR/trimming the router may hand us
            // a null Model. `search` lives in the PageModel (page-owned state
            // per the state-ownership table).
            let search =
                if isNull (box model) then "" else model.search
            let query = search.Trim().ToLowerInvariant()
            // Filter the canonical roster by username or Discord handle.
            let players =
                SharedModel.values m
                |> Array.filter (fun p ->
                    query = ""
                    || p.username.ToLowerInvariant().Contains query)
            RadzenUI.fadeIn (RadzenUI.vStackGap "var(--gap-section)" (concat {
                RadzenUI.pageHeadingCrumb "Members" (Some "Search the community roster by name.")
                    [ ("Home", Some "/"); ("Community", None); ("Members", None) ]
                // A search box that suggests usernames and drives the live
                // filter on the roster below.
                RadzenUI.autoComplete
                    (SharedModel.values m)
                    "username" search (fun v -> dispatch (SetSearch v))
                RadzenUI.dataGridAdvanced<Player> players
                    (Some "No members match that search.") false false None
                    (Some (fun p -> onMemberDetail p.id)) (concat {
                    // template columns (NOT dataGridColumn "property") — Radzen's
                    // string-`Property` binding uses runtime reflection that
                    // AOT/trim strips. Typed F# lambdas avoid reflection.
                    RadzenUI.dataGridTemplateColumn<Player> "Member" avatarCell
                    RadzenUI.dataGridTemplateColumn<Player> "Role" (fun p ->
                        RadzenUI.text RadzenUI.caption "Member")
                })
            }))