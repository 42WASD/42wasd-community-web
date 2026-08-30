namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Servers page — feature-owned view (REDESIGN 2026-08-30, user request:
/// "10x better and more organized for all games"). Same MVU architecture:
/// no local state beyond the selected game (root Model), callbacks up.
///
/// Layout: ONE RadzenAccordion (multi-expand) with a section PER GAME —
/// the header carries the game's Material icon, its name, and live summary
/// badges (online count · full count); the body is a responsive grid of
/// compact server cards. Each card = name + status badge + monospace address
/// (tooltip) + a small circular capacity gauge + player counts. An "Other"
/// section catches unassigned servers. This replaces the old SelectBar +
/// DataGrid wall of table rows that read like a raw dump.
module Servers =

    /// Material icon per game — DATA-DRIVEN: a keyword table maps substrings
    /// of the game's display name to an icon, so ANY game added to games.json
    /// gets an icon with zero code change; unknown names fall back to a
    /// generic gaming glyph. (Same visual mapping as before, now declarative.)
    let private iconKeywords: (string * string) list =
        [ "counter", "sports_shooting"
          "cs2", "sports_shooting"
          "dota", "castle"
          "minecraft", "landscape"
          "valorant", "gps_fixed"
          "league", "bolt"
          "lol", "bolt"
          "rocket", "sports_soccer" ]

    let private gameIcon (gname: string) =
        let n = gname.ToLowerInvariant()
        iconKeywords
        |> List.tryFind (fun (kw, _) -> n.Contains kw)
        |> Option.map snd
        |> Option.defaultValue "videogame_asset"

    /// Capacity style thresholds, shared by gauges and badges:
    /// full → danger, ≥80% → warning, otherwise success. maxPlayers is
    /// clamped to ≥1 so a malformed 0 in the data can't divide by zero.
    let private capacityStyle (s: GameServer) =
        let maxP = max s.maxPlayers 1
        if s.onlinePlayers >= maxP then RadzenUI.progressBarDanger
        elif float s.onlinePlayers / float maxP >= 0.8 then
            RadzenUI.progressBarWarning
        else RadzenUI.progressBarSuccess

    /// One compact server card: identity line (name + status badge), the
    /// monospace address with a native tooltip, and a footer row with the
    /// circular capacity gauge + player counts. The whole card is clickable
    /// (opens the detail dialog) — the click lives on a wrapping div because
    /// bare attrs can't yield inside a `concat` CE.
    let private serverCard (s: GameServer) (onSelect: string -> unit) =
        // Clamp denominator: malformed maxPlayers = 0 must not NaN the gauge.
        let ratio =
            float s.onlinePlayers / float (max s.maxPlayers 1)

        div {
            attr.``class`` "w-full cursor-pointer"
            on.click (fun (_: Microsoft.AspNetCore.Components.Web.MouseEventArgs) ->
                onSelect s.id)
            RadzenUI.cardOutlinedClass
                "p-[var(--pad-card)] flex flex-col gap-[0.5rem] h-full"
                (concat {
                    // Identity row: name + status badge pushed to the edge.
                    RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter
                        RadzenUI.justifyBetween
                        (concat {
                            RadzenUI.text RadzenUI.subtitle1 s.name
                            RadzenUI.withClass "self-start"
                                (RadzenUI.statusBadge s.status)
                        })
                    // Address: monospace, truncated, tooltip shows the full
                    // address on hover.
                    div {
                        attr.title s.address
                        attr.``class``
                            "font-mono text-[0.75rem] text-[var(--rz-text-secondary-color)] truncate"
                        s.address
                    }
                    // Footer: gauge (% or a lock when full) + counts column.
                    RadzenUI.hStackGapAlign "0.75rem" RadzenUI.alignCenter
                        RadzenUI.justifyBetween
                        (concat {
                            if s.onlinePlayers >= s.maxPlayers then
                                RadzenUI.progressBarCircularContent
                                    (float s.onlinePlayers) (float s.maxPlayers)
                                    RadzenUI.circularSmall
                                    RadzenUI.progressBarDanger
                                    (RadzenUI.icon "lock")
                            else
                                RadzenUI.progressBarCircular
                                    (float s.onlinePlayers) (float s.maxPlayers)
                                    RadzenUI.circularSmall true
                                    (capacityStyle s)
                            RadzenUI.withClass "text-right" (concat {
                                RadzenUI.text RadzenUI.caption "players"
                                RadzenUI.text RadzenUI.body2
                                    (sprintf "%d / %d" s.onlinePlayers s.maxPlayers)
                                RadzenUI.text RadzenUI.caption
                                    (sprintf "%.0f%% full" (ratio * 100.0))
                            })
                        })
                })
        }

    /// One game section: accordion header (icon + name) and a responsive
    /// card-grid body with summary badges. `expanded`/`onToggle` come from
    /// the root MVU state (SelectServerGame carries the game name).
    let private gameSection
        (icon: string)
        (gname: string)
        (list: GameServer[])
        (expanded: bool)
        (onToggle: bool -> unit)
        (onSelectServer: string -> unit) =

        let online =
            list |> Array.filter (fun s -> s.status = "online") |> Array.length
        let full =
            list
            |> Array.filter (fun s -> s.onlinePlayers >= s.maxPlayers)
            |> Array.length

        RadzenUI.accordionItemFull gname icon expanded onToggle
            (concat {
                if Array.isEmpty list then
                    RadzenUI.text RadzenUI.body2
                        "No servers for this game yet."
                else
                    // Summary badges: at-a-glance health for the section.
                    RadzenUI.hStackGap "0.5rem" (concat {
                        RadzenUI.badgePill RadzenUI.successBadge
                            (sprintf "%d online" online)
                        if full > 0 then
                            RadzenUI.badgePill RadzenUI.dangerBadge
                                (sprintf "%d full" full)
                        RadzenUI.badgePill RadzenUI.infoBadge
                            (sprintf "%d total" list.Length)
                    })
                    // Card grid: 1 col on phones, 2 from sm, 3 from lg.
                    RadzenUI.withClass
                        "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-[var(--gap-grid)] items-stretch"
                        (concat {
                            for s in list do
                                serverCard s onSelectServer
                        })
            })

    /// Grouping: servers by game in manifest order + an "Other" catch-all.
    let private segmentsOf
        (servers: Map<string, GameServer>)
        (games: Map<string, Game>) =

        let grouped, unassignedGroups =
            servers.Values
            |> Seq.toArray
            |> Array.groupBy (fun s -> s.gameId)
            |> Array.partition (fun (gid, _) -> games.ContainsKey gid)

        // Manifest order = each game's position in the games map.
        let rank =
            games
            |> Map.toArray
            |> Array.mapi (fun i (gid, _) -> gid, i)
            |> Map.ofArray

        let byGame =
            grouped
            |> Array.map (fun (gid, list) -> gid, games[gid].name, list)
            |> Array.sortBy (fun (gid, _, _) -> rank[gid])

        let unassigned = unassignedGroups |> Array.collect snd

        [ for (_, gname, list) in byGame -> (gname, list)
          if unassigned.Length > 0 then ("Other", unassigned) ]

    /// The redesigned body: a multi-expand accordion of game sections.
    /// `selected` (root MVU state via SelectServerGame) names the section
    /// expanded on first render; toggling any section updates it.
    let private serverSections
        (selected: string option)
        (onChange: string option -> unit)
        (onSelectServer: string -> unit)
        (servers: Map<string, GameServer>)
        (games: Map<string, Game>) =

        let segments = segmentsOf servers games

        if List.isEmpty segments then
            RadzenUI.text RadzenUI.body2
                "No servers right now — check back soon."
        else
            // Default: the user's selection, else the first section.
            // NOTE: `selected` carries the game NAME now (the accordion
            // header's onToggle reports its own gname), so no id->name
            // lookup — comparing it directly keeps expand state stable.
            let selectedName =
                selected
                |> Option.filter (fun name ->
                    segments |> List.exists (fun (g, _) -> g = name))
                |> Option.orElse (segments |> List.tryHead |> Option.map fst)

            RadzenUI.accordionMultiple (concat {
                for (gname, list) in segments do
                    let icon =
                        if gname = "Other" then "help_outline"
                        else gameIcon gname
                    let isExpanded =
                        match selectedName with
                        | Some name -> name = gname
                        | None -> false
                    gameSection icon gname list isExpanded
                        // Toggle semantics: re-clicking the open section
                        // collapses it; clicking another expands it.
                        (fun sel ->
                            onChange (if sel then Some gname
                                      else if isExpanded then None
                                      else Some gname))
                        onSelectServer
            })

    /// Entry point — signature unchanged (same MVU wiring in App/Layout).
    let serverTabs
        (selected: string option)
        (onChange: string option -> unit)
        (onRefresh: unit -> unit)
        (onSelectServer: string -> unit)
        (servers: Map<string, GameServer>)
        (games: Map<string, Game>) =

        RadzenUI.vStackGap "var(--gap-grid)" (concat {
            // Toolbar: live totals + refresh, right-aligned icon button.
            RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter
                RadzenUI.justifyBetween
                (concat {
                    let online =
                        servers.Values
                        |> Seq.filter (fun s -> s.status = "online")
                        |> Seq.length
                    RadzenUI.hStackGap "0.5rem" (concat {
                        RadzenUI.badgePill RadzenUI.successBadge
                            (sprintf "%d online" online)
                        RadzenUI.badgePill RadzenUI.infoBadge
                            (sprintf "%d servers" servers.Count)
                    })
                    RadzenUI.iconButton "refresh" onRefresh
                })
            serverSections selected onChange onSelectServer servers games
        })

    /// Page view — same RemoteData handling (skeleton / error / loaded);
    /// only the loaded body is redesigned.
    let view
        (selected: string option)
        (onChange: string option -> unit)
        (onRefresh: unit -> unit)
        (onSelectServer: string -> unit)
        (shared: SharedModel) =

        cond shared.servers
        <| function
            | NotAsked | Loading ->
                // Skeleton mirrors the redesign: toolbar + accordion rows.
                RadzenUI.vStackGap "var(--gap-section)" (concat {
                    RadzenUI.skeleton "width: 20%; height: 2rem;"
                    RadzenUI.cardOutlined (concat {
                        RadzenUI.vStackGap "0.75rem" (concat {
                            RadzenUI.skeleton "width: 30%; height: 1.25rem;"
                            RadzenUI.withClass
                                "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-[var(--gap-grid)]"
                                (concat {
                                    for _ in 1..3 do
                                        RadzenUI.skeletonCardBody ()
                                })
                        })
                    })
                })
            | Failed _ -> RadzenUI.failedViewRetry "servers" onRefresh
            | Loaded servers ->
                RadzenUI.fadeIn
                    (RadzenUI.vStackGap "var(--gap-section)" (concat {
                        RadzenUI.pageHeadingCrumb "Servers"
                            (Some "Live game servers and their capacity.")
                            [ ("Home", Some "/"); ("Servers", None) ]
                        match shared.games with
                        | Loaded games ->
                            serverTabs selected onChange onRefresh onSelectServer
                                servers games
                        | _ ->
                            serverTabs selected onChange onRefresh onSelectServer
                                servers Map.empty<string, Game>
                    }))
