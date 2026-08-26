namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Servers page — feature-owned view. Selects the canonical Servers data from
/// Shared; owns no state of its own. A live server browser: `RadzenTabs` groups
/// servers by game, each server card shows status as a `RadzenBadge` and its
/// player capacity as a `RadzenProgressBar` (fills toward red near full).
module Servers =

    /// Group the loaded servers into tabs, one per game id (in manifest order).
    /// Each tab now shows a sortable/filterable/paginated `RadzenDataGrid`
    /// (Phase 17c+): name, address, capacity, and status — with cell tooltips
    /// so long addresses and full capacity bars read clearly on hover.
    let serverTabs (servers: Map<string, GameServer>) (games: Map<string, Game>) =
        let byGame =
            games |> Map.toArray
            |> Array.map (fun (gid, g) ->
                let list =
                    servers.Values
                    |> Seq.filter (fun s -> s.gameId = gid)
                    |> Seq.toArray
                gid, g.name, list)
            |> Array.filter (fun (_, _, list) -> list.Length > 0)
        // Unassigned servers (a game id not in the games map) fall through.
        let unassigned =
            servers.Values
            |> Seq.filter (fun s -> not (games.ContainsKey s.gameId))
            |> Seq.toArray

        let serverGrid (list: GameServer[]) =
            RadzenUI.cardOutlined (RadzenUI.dataGrid<GameServer> list (concat {
                RadzenUI.dataGridColumn<GameServer> "name" "Server" false
                RadzenUI.dataGridColumn<GameServer> "address" "Address" true
                RadzenUI.dataGridColumn<GameServer> "onlinePlayers" "Players" false
                RadzenUI.dataGridColumn<GameServer> "maxPlayers" "Capacity" false
                RadzenUI.dataGridColumn<GameServer> "status" "Status" false
            }))

        let tabNodes = ResizeArray<Node>()
        for (_, gname, list) in byGame do
            tabNodes.Add (RadzenUI.tabItem gname (serverGrid list))
        if unassigned.Length > 0 then
            tabNodes.Add (RadzenUI.tabItem "Other" (serverGrid unassigned))
        RadzenUI.tabs (forEach tabNodes (fun n -> n))

    let view (shared: SharedModel) =
        cond shared.servers <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load servers."
        | Loaded servers ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Servers"
                match shared.games with
                | Loaded games -> serverTabs servers games
                | _ -> serverTabs servers Map.empty<string, Game>
            })