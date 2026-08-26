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

    /// Capacity bar style: red when full, warning when mostly full, otherwise
    /// primary. So a server about to cap reads as "full" at a glance.
    let capacityStyle (s: GameServer) =
        if s.onlinePlayers >= s.maxPlayers then RadzenUI.progressBarDanger
        elif float s.onlinePlayers / float (max s.maxPlayers 1) >= 0.8 then RadzenUI.progressBarWarning
        else RadzenUI.progressBarSuccess

    /// A server card: status badge, name, address, and a capacity bar.
    let serverCard (s: GameServer) =
        let statusBadge =
            match s.status with
            | "online" -> RadzenUI.badgePill RadzenUI.successBadge "online"
            | "maintenance" -> RadzenUI.badgePill RadzenUI.warningBadge "maintenance"
            | _ -> RadzenUI.badgePill RadzenUI.darkBadge "offline"
        RadzenUI.columnResponsive 12 6 4 (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.heading6 s.name
                statusBadge
            })
            RadzenUI.text RadzenUI.caption s.address
            RadzenUI.progressBarValue (float s.onlinePlayers) (float s.maxPlayers) (capacityStyle s)
        })))

    /// Group the loaded servers into tabs, one per game id (in manifest order).
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
        let tabNodes = ResizeArray<Node>()
        for (_, gname, list) in byGame do
            tabNodes.Add (RadzenUI.tabItem gname (RadzenUI.rowGap "1rem" (forEach list (fun s -> serverCard s))))
        if unassigned.Length > 0 then
            tabNodes.Add (RadzenUI.tabItem "Other" (RadzenUI.rowGap "1rem" (forEach unassigned (fun s -> serverCard s))))
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