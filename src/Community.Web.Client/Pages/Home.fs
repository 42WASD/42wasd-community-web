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

    /// One KPI cell inside the cohesive stats bar: a caption label above a
    /// value. Each cell is an equal-width (`flex-1`) child of the bar, so the
    /// five stats share the row evenly — no orphan, no uneven wrap. The label
    /// and value sit in a tight vertical stack.
    let statCell (label: string) (value: string) =
        RadzenUI.vStackGap "0.25rem" (concat {
            RadzenUI.text RadzenUI.caption label
            RadzenUI.text RadzenUI.heading4 value
        })

    /// The cohesive KPI bar: ONE outlined card containing all five stats in a
    /// single wrapping flex row (`flex-1` cells). Because it's one card (not
    /// five independent cards), there is no 2-2-1 wrap and no orphaned cell —
    /// the five numbers read as one dashboard strip. Cells keep a min-width so
    /// they wrap at even widths (never a squished 5th cell), and every
    /// non-first cell carries a hairline divider on its top AND left so wrapped
    /// rows still read as one aligned grid (`-mt-px/-ml-px` collapses doubled
    /// borders between neighbors).
    let statsBar (items: (string * string) list) =
        let cells =
            items
            |> List.mapi (fun i (label, value) ->
                let baseCls = "flex-1 min-w-[7rem] px-4 py-4"
                let cls =
                    if i > 0 then baseCls + " border-l border-t border-[var(--rz-border-color)] -mt-px -ml-px"
                    else baseCls
                div {
                    attr.``class`` cls
                    statCell label value
                })
        RadzenUI.cardOutlinedClass "rz-p-0 overflow-hidden" (
            div {
                attr.``class`` "flex flex-wrap"
                concat {
                    for cell in cells do
                        cell
                }
            })

    /// A compact live-server row: name + status badge on the left, circular
    /// capacity gauge + counts on the right. `justifyBetween` spreads the two
    /// groups to the card edges (no dead middle gap on narrow cards) and the
    /// inner stacks align so badge/gauge baselines line up.
    let serverStatusRow (s: GameServer) =
        RadzenUI.cardOutlined (RadzenUI.hStackGapAlign "0.75rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                // Pulsing status dot for live servers. Tailwind `animate-pulse`
                // (opacity pulse) + `rounded-full` gives the same live-dot
                // effect as the old custom `.animate-pulse-dot`. Sized with a
                // Tailwind utility and colored with the Radzen `--rz-success`
                // token (no hardcoded hex).
                match s.status with
                | "online" ->
                    div { attr.``class`` "animate-pulse rounded-full w-2.5 h-2.5 bg-[var(--rz-success)] motion-reduce:animate-none" }
                | _ -> empty ()
                RadzenUI.vStackGap "0.125rem" (concat {
                    RadzenUI.text RadzenUI.body1 s.name
                    RadzenUI.statusBadge s.status
                })
            })
            RadzenUI.hStackGap "0.75rem" (concat {
                RadzenUI.progressBarCircular (float s.onlinePlayers) (float s.maxPlayers)
                    RadzenUI.circularMedium true
                    (if s.onlinePlayers >= s.maxPlayers then RadzenUI.progressBarDanger
                     elif float s.onlinePlayers / float (max s.maxPlayers 1) >= 0.8 then RadzenUI.progressBarWarning
                     else RadzenUI.progressBarSuccess)
                RadzenUI.vStackGap "0.125rem" (concat {
                    RadzenUI.text RadzenUI.caption "capacity"
                    RadzenUI.text RadzenUI.body2 (sprintf "%d / %d online" s.onlinePlayers s.maxPlayers)
                })
            })
        }))

    /// The "latest news" card container: a heading + a vertical timeline. This is
    /// the ONE place the news layout is defined. Both the real content and the
    /// loading skeleton feed timeline `items` into this same container, so a
    /// layout change here (e.g. a different heading, card variant, gap) is
    /// reflected in the loading state automatically.
    let newsCard (items: Node list) =
        RadzenUI.cardOutlined (RadzenUI.vStackGap "0.75rem" (concat {
            RadzenUI.text RadzenUI.heading6 "Latest news"
            RadzenUI.timeline (forEach items (fun n -> n))
        }))

    /// A real news timeline item.
    let newsItem (date: string) (title: string) (body: string) =
        RadzenUI.timelineItem date RadzenUI.pointPrimary (concat {
            RadzenUI.text RadzenUI.body1 title
            RadzenUI.text RadzenUI.caption body
        })

    /// A skeleton news timeline item — same timeline-node shape, skeleton body.
    let newsSkeletonItem () =
        RadzenUI.timelineItem "" RadzenUI.pointPrimary (RadzenUI.skeletonLines [ "100%"; "78%" ])

    /// The "featured games" carousel container. Same single-source principle as
    /// `newsCard`: both real slides and skeleton slides flow through here, so
    /// carousel layout changes update the loading state automatically.
    let gamesCarousel (slides: Node list) =
        RadzenUI.carousel (min 3 slides.Length) (forEach slides (fun s -> s))

    /// A real featured-game slide: banner image, title, genre chip, blurb —
    /// via the shared `mediaCard` wrapper so the slide matches the Games grid
    /// cards exactly (uniform banner box + padded meta section).
    let gameSlide (g: Game) =
        RadzenUI.carouselItem (RadzenUI.mediaCard g.imageUrl g.name (concat {
            RadzenUI.text RadzenUI.heading6 g.name
            RadzenUI.chip g.genre RadzenUI.primaryBadge
            RadzenUI.text RadzenUI.body2 g.description
        }))

    /// A skeleton featured-game slide — same carousel-item shape, skeleton body.
    let gameSkeletonSlide () =
        RadzenUI.carouselItem (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.skeleton "width: 100%; height: 9rem;"
            RadzenUI.skeleton "width: 55%; height: 1.25rem;"
            RadzenUI.skeleton "width: 30%; height: 0.9rem;"
            RadzenUI.skeleton "width: 85%; height: 0.9rem;"
        })))

    /// Render the dashboard from the selected shared slices. Order: headline →
    /// stats → featured games → live servers → latest news. Each section
    /// transitions *independently*: it renders the SAME container (defined
    /// above) with skeleton items while that slice's `RemoteData` is not
    /// `Loaded`, then swaps to real items and fades in — no hardcoded look, no
    /// atomic all-or-nothing swap, no pop/jitter.
    let view (shared: SharedModel) =
        let gamesCount, onlineNow, openTournaments, memberCount, favoriteCount = stats shared

        // Stats strip — always present (values are 0 until loaded), but fades
        // in once the primary caches are ready so it doesn't flash "0 0 0".
        let statsSection =
            statsBar [
                "Games", gamesCount.ToString()
                "Players online", onlineNow.ToString()
                "Open tournaments", openTournaments.ToString()
                "Members", memberCount.ToString()
                "Favourite games", favoriteCount.ToString()
            ]

        // Latest news: real timeline items once loaded, else skeleton items —
        // both fed into the SAME newsCard container.
        let newsItems =
            match shared.news with
            | Loaded n when n.Count > 0 ->
                n |> SharedModel.values
                |> Array.sortByDescending (fun x -> x.publishedAt)
                |> Array.toList
                |> List.map (fun x -> newsItem (x.publishedAt.ToString("yyyy-MM-dd")) x.title x.body)
            | _ -> [ for _ in 1..3 -> newsSkeletonItem () ]
        let newsSection = newsCard newsItems

        // Featured games: same gamesCarousel container, real vs skeleton slides.
        let gamesSection =
            cond shared.games <| function
            | Loaded g when g.Count > 0 ->
                RadzenUI.fadeIn (gamesCarousel (SharedModel.values g |> Array.toList |> List.map gameSlide))
            | _ -> RadzenUI.fadeIn (gamesCarousel [ for _ in 1..3 -> gameSkeletonSlide () ])

        // Live servers: SAME card + row container for real rows and skeleton
        // cards, so the layout matches exactly. Equal-height via `columnStretch`.
        // Phone breakpoint is 12 (one card per row) — three ~125px-wide cards
        // squeezed side-by-side was unreadable on 390px screens.
        let serversSection =
            cond shared.servers <| function
            | Loaded servers when servers.Count > 0 ->
                RadzenUI.fadeIn (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.text RadzenUI.heading6 "Live servers"
                    RadzenUI.rowGap "1rem" (forEach (SharedModel.values servers) (fun s ->
                        RadzenUI.columnStretch 12 6 4 (serverStatusRow s)))
                })))
            | _ ->
                RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.skeleton "width: 35%; height: 1.25rem;"
                    RadzenUI.rowGap "1rem" (concat {
                        for _ in 1..3 do
                            RadzenUI.columnStretch 12 6 4 (RadzenUI.skeleton "width: 100%; height: 5rem;")
                    })
                }))

        RadzenUI.vStackGap "1.5rem" (concat {
            RadzenUI.pageHeading
                "Welcome to the gaming community!"
                (Some "Games we play, active servers, upcoming tournaments, and latest news.")

            RadzenUI.fadeIn statsSection
            newsSection
            gamesSection
            serversSection
        })