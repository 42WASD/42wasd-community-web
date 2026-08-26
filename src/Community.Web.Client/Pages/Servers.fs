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
        // Single pass over the server list: group by game id, then split into
        // servers that belong to a known game vs. unassigned ones. This avoids
        // the previous per-game `Seq.filter` (O(games × servers)).
        let grouped, unassigned =
            servers.Values
            |> Seq.toArray
            |> Array.groupBy (fun s -> s.gameId)
            |> Array.partition (fun (gid, _) -> games.ContainsKey gid)
        // Preserve tab order: groups are ordered by each game's position in the
        // manifest (games map), not by first-server-appearance order.
        let rank = games |> Map.toArray |> Array.mapi (fun i (gid, _) -> gid, i) |> Map.ofArray
        let byGame =
            grouped
            |> Array.map (fun (gid, list) -> gid, games[gid].name, list)
            |> Array.sortBy (fun (gid, _, _) -> rank[gid])
        let unassigned = unassigned |> Array.collect snd

        let serverGrid (list: GameServer[]) =
            RadzenUI.cardOutlined (RadzenUI.dataGrid<GameServer> list (concat {
                // NOTE: template columns (NOT dataGridColumn "property") — Radzen's
                // string-`Property` binding uses runtime reflection that AOT/trim
                // strips → NullReferenceException. Typed F# lambdas avoid reflection.
                RadzenUI.dataGridTemplateColumn<GameServer> "Server" (fun s ->
                    RadzenUI.text RadzenUI.body1 s.name)
                RadzenUI.dataGridTemplateColumn<GameServer> "Address" (fun s ->
                    RadzenUI.text RadzenUI.body1 s.address)
                RadzenUI.dataGridTemplateColumn<GameServer> "Players" (fun s ->
                    RadzenUI.text RadzenUI.body1 (string s.onlinePlayers))
                RadzenUI.dataGridTemplateColumn<GameServer> "Capacity" (fun s ->
                    RadzenUI.text RadzenUI.body1 (string s.maxPlayers))
                RadzenUI.dataGridTemplateColumn<GameServer> "Status" (fun s ->
                    RadzenUI.statusBadge s.status)
            }))

        let tabNodes =
            [ for (_, gname, list) in byGame do
                yield RadzenUI.tabItem gname (serverGrid list)
              if unassigned.Length > 0 then
                yield RadzenUI.tabItem "Other" (serverGrid unassigned) ]
        RadzenUI.tabs (forEach tabNodes (fun n -> n))

    let view (shared: SharedModel) =
        cond shared.servers <| function
        | NotAsked | Loading ->
            // Dynamic skeleton: a heading + a tab strip + a data-grid block,
            // mirroring the loaded ServerTabs layout.
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.skeleton "width: 20%; height: 2rem;"
                RadzenUI.rowGap "0.75rem" (concat {
                    for _ in 1..3 do
                        RadzenUI.skeleton "width: 15%; height: 1.25rem;"
                })
                RadzenUI.cardOutlined (RadzenUI.skeletonTable [ "22%"; "30%"; "12%"; "12%"; "12%" ])
            })
        | Failed _ ->
            RadzenUI.failedView "servers"
        | Loaded servers ->
            RadzenUI.fadeIn (RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Servers"
                match shared.games with
                | Loaded games -> serverTabs servers games
                | _ -> serverTabs servers Map.empty<string, Game>
            }))