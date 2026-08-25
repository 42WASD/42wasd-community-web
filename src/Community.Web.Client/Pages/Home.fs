namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Home page — dashboard. Owns no state of its own; it only *selects* the
/// canonical shared data it renders (per the state-ownership model: reuse, do
/// not duplicate). Built entirely on Radzen primitives (Phase 17b): the
/// 12-col responsive grid, RadzenCard surfaces, RadixText typography.
module Home =

    /// Compute community stats from shared state (0 when not loaded yet).
    let stats (shared: SharedModel) =
        let gameCount =
            match shared.games with
            | Loaded m -> m.Count
            | _ -> 0
        let onlineNow =
            match shared.servers with
            | Loaded m -> m.Values |> Seq.sumBy (fun s -> s.onlinePlayers)
            | _ -> 0
        let openTournaments =
            match shared.tournaments with
            | Loaded m -> m.Values |> Seq.filter (fun t -> t.registrationOpen) |> Seq.length
            | _ -> 0
        let memberCount =
            match shared.players with
            | Loaded m -> m.Count
            | _ -> 0
        let favoriteCount = shared.favoriteGames.Count
        gameCount, onlineNow, openTournaments, memberCount, favoriteCount

    /// A responsive stat panel: a column wrapping an outlined card with a
    /// caption label and a value.
    let statPanel (sm: int) (label: string) (value: string) =
        RadzenUI.columnResponsive sm 6 4 (concat {
            RadzenUI.cardOutlined (RadzenUI.vStackGap "0.25rem" (concat {
                RadzenUI.text RadzenUI.caption label
                RadzenUI.text RadzenUI.heading4 value
            }))
        })

    /// Render the dashboard from the selected shared slices.
    let view (shared: SharedModel) =
        let gamesCount, onlineNow, openTournaments, memberCount, favoriteCount = stats shared
        RadzenUI.vStackGap "1.5rem" (concat {
            RadzenUI.text RadzenUI.display3 "Welcome to the gaming community!"
            RadzenUI.text RadzenUI.subtitle1
                "Games we play, active servers, upcoming tournaments, and latest news."

            RadzenUI.rowGap "1rem" (concat {
                statPanel 6 "Games" (gamesCount.ToString())
                statPanel 6 "Players online" (onlineNow.ToString())
                statPanel 6 "Open tournaments" (openTournaments.ToString())
                statPanel 6 "Members" (memberCount.ToString())
                statPanel 6 "Favourite games" (favoriteCount.ToString())
            })
        })