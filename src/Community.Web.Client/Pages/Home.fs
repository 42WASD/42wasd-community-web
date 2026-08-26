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

    /// A compact live-server row: name, status badge, and a circular capacity
    /// gauge. The online count comes straight from the canonical servers cache.
    let serverStatusRow (s: GameServer) =
        RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                // Pulsing status dot for live servers (see index.css).
                match s.status with
                | "online" ->
                    div { attr.``class`` "animate-pulse-dot"; attr.style "width:10px; height:10px; background-color:#009739;" }
                | _ -> empty ()
                RadzenUI.text RadzenUI.body1 s.name
                match s.status with
                | "online" -> RadzenUI.badgePill RadzenUI.successBadge "online"
                | "maintenance" -> RadzenUI.badgePill RadzenUI.warningBadge "maintenance"
                | _ -> RadzenUI.badgePill RadzenUI.darkBadge "offline"
            })
            RadzenUI.hStackGap "1rem" (concat {
                RadzenUI.progressBarCircular (float s.onlinePlayers) (float s.maxPlayers)
                    RadzenUI.circularMedium true
                    (if s.onlinePlayers >= s.maxPlayers then RadzenUI.progressBarDanger
                     elif float s.onlinePlayers / float (max s.maxPlayers 1) >= 0.8 then RadzenUI.progressBarWarning
                     else RadzenUI.progressBarSuccess)
                RadzenUI.vStackGap "0.25rem" (concat {
                    RadzenUI.text RadzenUI.caption "capacity"
                    RadzenUI.text RadzenUI.body1 (sprintf "%d / %d online" s.onlinePlayers s.maxPlayers)
                })
            })
        }))

    /// The "latest news" section rendered as a vertical RadzenTimeline, so the
    /// (previously invisible) News cache surfaces as announcements. Sorted so
    /// the most recent post is FIRST (top of the timeline).
    let newsTimeline (news: Map<string, News>) =
        let items =
            Map.toArray news
            |> Array.sortByDescending (fun (_, n) -> n.publishedAt)
            |> Array.map (fun (_, n) ->
                RadzenUI.timelineItem (n.publishedAt.ToString("yyyy-MM-dd")) RadzenUI.pointPrimary
                    (concat {
                        RadzenUI.text RadzenUI.body1 n.title
                        RadzenUI.text RadzenUI.caption n.body
                    }))
        RadzenUI.cardOutlined (RadzenUI.vStackGap "0.75rem" (concat {
            RadzenUI.text RadzenUI.heading6 "Latest news"
            RadzenUI.timeline (forEach items (fun n -> n))
        }))

    /// A "featured games" carousel cycling the canonical games cache. Each
    /// slide is a rich card: a banner image, the title, a genre chip, and a
    /// short description — so the hero shows off the community's titles.
    let featuredCarousel (games: Map<string, Game>) =
        let slides =
            Map.toArray games
            |> Array.map (fun (_, g) ->
                RadzenUI.carouselItem (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.image g.imageUrl g.name
                    RadzenUI.text RadzenUI.heading6 g.name
                    RadzenUI.chip g.genre RadzenUI.primaryBadge
                    RadzenUI.text RadzenUI.body2 g.description
                }))))
        RadzenUI.carousel (min 3 slides.Length) (forEach slides (fun s -> s))

    /// Render the dashboard from the selected shared slices. Order: headline →
    /// stats → featured games → live servers → latest news.
    let view (shared: SharedModel) =
        let gamesCount, onlineNow, openTournaments, memberCount, favoriteCount = stats shared
        RadzenUI.vStackGap "1.5rem" (concat {
            RadzenUI.text RadzenUI.display3 "Welcome to the gaming community!"
            RadzenUI.text RadzenUI.subtitle1
                "Games we play, active servers, upcoming tournaments, and latest news."

            // Live community stats strip.
            RadzenUI.rowGap "1rem" (concat {
                statPanel 6 "Games" (gamesCount.ToString())
                statPanel 6 "Players online" (onlineNow.ToString())
                statPanel 6 "Open tournaments" (openTournaments.ToString())
                statPanel 6 "Members" (memberCount.ToString())
                statPanel 6 "Favourite games" (favoriteCount.ToString())
            })

            // Latest news FIRST (surface the News data, most recent on top).
            match shared.news with
            | Loaded n when n.Count > 0 -> newsTimeline n
            | _ -> empty ()

            // Featured games carousel (reads the canonical games cache).
            match shared.games with
            | Loaded g when g.Count > 0 -> featuredCarousel g
            | _ -> empty ()

            // Live server status strip (reads the canonical servers cache).
            cond shared.servers <| function
            | Loaded servers when servers.Count > 0 ->
                RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.text RadzenUI.heading6 "Live servers"
                    RadzenUI.rowGap "1rem" (forEach (Map.toArray servers) (fun (_, s) ->
                        RadzenUI.columnResponsive 12 6 4 (serverStatusRow s)))
                }))
            | _ -> empty ()
        })