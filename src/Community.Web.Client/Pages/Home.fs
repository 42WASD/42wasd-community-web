namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Home page — feature-owned view. Owns no state of its own; it only *selects*
/// the canonical shared data it renders (per the state-ownership model: reuse,
/// do not duplicate). The dashboard aggregates stats + latest rows from Shared.
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

    /// Render the dashboard from the selected shared slices.
    let view (shared: SharedModel) =
        let gamesCount, onlineNow, openTournaments, memberCount, favoriteCount = stats shared
        Layout.Home()
            .GamesCount(gamesCount.ToString())
            .OnlineNow(onlineNow.ToString())
            .OpenTournaments(openTournaments.ToString())
            .MembersCount(memberCount.ToString())
            .Favorites(favoriteCount.ToString())
            .Games(dataRows shared.games <| fun g -> tr { td { g.name }; td { g.genre } })
            .Servers(dataRows shared.servers <| fun s -> tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() } })
            .Tournaments(dataRows shared.tournaments <| fun t -> tr { td { t.name }; td { t.prize } })
            .News(dataRows shared.news <| fun n -> tr { td { n.title }; td { n.publishedAt.ToString("yyyy-MM-dd") } })
            .Elt()