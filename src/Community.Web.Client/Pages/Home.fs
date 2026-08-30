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
                let baseCls = "flex-1 min-w-[7rem] px-[var(--pad-card)] py-[var(--pad-card)]"
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
        // Build the gauge OUTSIDE the CE (bare if/match inside `comp`/`concat`
        // bodies is ambiguous — Bolero gotcha).
        let gauge =
            if s.onlinePlayers >= s.maxPlayers then
                // Full server: the gauge's inner Template (audit #25) shows a
                // lock glyph instead of the redundant "100%".
                RadzenUI.progressBarCircularContent
                    (float s.onlinePlayers) (float s.maxPlayers)
                    RadzenUI.circularMedium RadzenUI.progressBarDanger
                    (RadzenUI.icon "lock")
            else
                RadzenUI.progressBarCircular (float s.onlinePlayers) (float s.maxPlayers)
                    RadzenUI.circularMedium true
                    (if float s.onlinePlayers / float (max s.maxPlayers 1) >= 0.8 then RadzenUI.progressBarWarning
                     else RadzenUI.progressBarSuccess)
        // w-full: the row card is a flex item inside RadzenColumn (display:flex,
        // row) — without it each card shrink-wraps to its content and the
        // three rows get RAGGED right edges (the misalignment the user saw:
        // 386/373/362px wide inside equal 705px columns). w-full = uniform.
        RadzenUI.cardOutlinedClass "w-full" (RadzenUI.hStackGapAlign "0.75rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                // Pulsing status dot for live servers. Tailwind `animate-pulse`
                // (opacity pulse) + `rounded-full` gives the same live-dot
                // effect as the old custom `.animate-pulse-dot`. Sized with a
                // Tailwind utility and colored with the Radzen `--rz-success`
                // token (no hardcoded hex).
                match s.status with
                | "online" ->
                    let pingClass =
                        "animate-pulse rounded-full w-2.5 h-2.5 bg-[var(--rz-success)] "
                        + "motion-reduce:animate-none"
                    div { attr.``class`` pingClass }
                | _ -> empty ()
                RadzenUI.vStackGap "0.125rem" (concat {
                    RadzenUI.text RadzenUI.body1 s.name
                    // self-start wrapper: the badge must NOT stretch to the
                    // column width (flex column align-items:normal = stretch)
                    // — stretched it became a full-width green bar whose
                    // width tracked the server name's length (non-uniform).
                    div {
                        attr.``class`` "self-start"
                        RadzenUI.statusBadge s.status
                    }
                })
            })
            // Fixed-width capacity column, content pushed right: the gauge
            // and the capacity text start at the SAME x on every row (the
            // block used to shrink-wrap its text, so "8 / 12" vs "20 / 50"
            // shifted the gauge column per row — the non-uniformity the
            // user reported). 110px fits "capacity" + "999 / 999 online".
            RadzenUI.withClass "w-[110px] justify-end" (concat {
                gauge
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

    /// A real news timeline item with an icon inside the point (audit #24:
    /// PointContent icon markers turn the feed into a scannable activity
    /// list).
    let newsItem (date: string) (title: string) (body: string) =
        RadzenUI.timelineItemIcon date RadzenUI.pointPrimary "article" (concat {
            RadzenUI.text RadzenUI.body1 title
            RadzenUI.text RadzenUI.caption body
        })

    /// A skeleton news timeline item — same timeline-node shape, skeleton body.
    let newsSkeletonItem () =
        RadzenUI.timelineItem "" RadzenUI.pointPrimary (RadzenUI.skeletonLines [ "100%"; "78%" ])

    /// One carousel slide = a PAGE GROUP of up to TWO game cards (user
    /// request 2026-08-30: 2 per slide on mobile). Cards stack on phones
    /// (grid-cols-1) and sit side-by-side from sm upward (sm:grid-cols-2).
    /// Cards stretch to equal height inside the group.
    let gamePageGroup (group: Node list) =
        RadzenUI.carouselItem (
            div {
                attr.``class`` "grid grid-cols-1 sm:grid-cols-2 gap-[var(--gap-grid)] items-stretch h-full"
                concat {
                    for slide in group do
                        slide
                }
            })

    /// The "featured games" carousel. G4 FIX (audit): RadzenCarousel steps
    /// `selectedIndex += ItemsPerPage` per click, while the visible window was
    /// ONE card on mobile — so "next" skipped 3 games at a time. Instead of
    /// asking the carousel to multi-up, each slide is now a PAGE GROUP and
    /// the carousel runs at ItemsPerPage = 1: one click = one displayed group
    /// at EVERY viewport — n cards advance per turn, matching what is shown.
    /// (User request 2026-08-30) MOBILE shows TWO games per slide, so chunks
    /// are 2; cards inside stack on phones (grid-cols-1) and go 2-up from sm
    /// upward. 6 games -> 3 clean swipes instead of 2 heavy 3-card slides.
    let gamesCarousel (slides: Node list) =
        slides
        |> List.chunkBySize 2
        |> List.map gamePageGroup
        |> fun groups -> RadzenUI.carousel 1 (forEach groups (fun g -> g))

    /// A real featured-game card: banner image, title, genre chip, blurb —
    /// via the shared `mediaCard` wrapper so the card matches the Games grid
    /// exactly (uniform banner box + padded meta section). Rendered INSIDE a
    /// page-group slide by `gamePageGroup`.
    let gameSlide (g: Game) =
        RadzenUI.mediaCard g.imageUrl g.name (concat {
            RadzenUI.text RadzenUI.heading6 g.name
            RadzenUI.chip g.genre RadzenUI.primaryBadge
            RadzenUI.text RadzenUI.body2 g.description
        })

    /// A skeleton game CARD (inside a group slide) — same mediaCard shape.
    let gameSkeletonSlide () =
        RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
            RadzenUI.skeleton "width: 100%; height: 9rem;"
            RadzenUI.skeleton "width: 55%; height: 1.25rem;"
            RadzenUI.skeleton "width: 30%; height: 0.9rem;"
            RadzenUI.skeleton "width: 85%; height: 0.9rem;"
        }))

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
                    RadzenUI.rowGap "var(--gap-grid)" (forEach (SharedModel.values servers) (fun s ->
                        RadzenUI.columnStretch 12 6 4 (serverStatusRow s)))
                })))
            | _ ->
                RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.skeleton "width: 35%; height: 1.25rem;"
                    RadzenUI.rowGap "var(--gap-grid)" (concat {
                        for _ in 1..3 do
                            RadzenUI.columnStretch 12 6 4 (RadzenUI.skeleton "width: 100%; height: 5rem;")
                    })
                }))

        // Hero (42-audit #7): identity moment + primary CTA above the KPIs.
        let heroClass =
            "w-full p-[var(--pad-page)] md:p-[var(--pad-page)] "
            + "bg-[color-mix(in_srgb,var(--rz-primary)_8%,var(--rz-panel-background-color))]"
        let heroBlurb =
            "The **42 Abu Dhabi** gaming community hub — browse the games we "
            + "play, watch live server capacity, and never miss a tournament."
        let hero =
            RadzenUI.cardOutlinedClass heroClass
                (RadzenUI.vStackGap "var(--gap-grid)" (concat {
                    RadzenUI.text RadzenUI.display2 "42WASD"
                    RadzenUI.markdown heroBlurb
                    RadzenUI.hStackGap "0.5rem" (concat {
                        RadzenUI.link "/games"
                            (RadzenUI.buttonAction "Browse games" RadzenUI.primaryButton (fun () -> ()))
                    })
                }))
        // Upcoming tournaments strip (42-audit #8).
        let tournamentsStrip =
            match shared.tournaments with
            | Loaded m ->
                let open_ = SharedModel.values m |> Array.filter (fun t -> t.registrationOpen)
                if Array.isEmpty open_ then empty () else
                RadzenUI.fadeIn (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                        RadzenUI.text RadzenUI.heading6 "Registration open"
                        RadzenUI.link "/tournaments" (RadzenUI.text RadzenUI.caption "View all →")
                    })
                    for t in (open_ |> Array.sortBy (fun t -> t.startsAt) |> Array.truncate 3) do
                        RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                            RadzenUI.text RadzenUI.body1 t.name
                            RadzenUI.text RadzenUI.caption (t.startsAt.ToString("MM-dd HH:mm"))
                        })
                })))
            | _ -> empty ()
        // Leaderboard teaser (42-audit #9).
        let leaderboardStrip =
            match shared.players with
            | Loaded m ->
                RadzenUI.fadeIn (RadzenUI.cardOutlined (RadzenUI.vStackGap "0.5rem" (concat {
                    RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                        RadzenUI.text RadzenUI.heading6 "Members"
                        RadzenUI.link "/members" (RadzenUI.text RadzenUI.caption "View all →")
                    })
                    for pl in (SharedModel.values m |> Array.truncate 5) do
                        RadzenUI.hStackGap "0.75rem" (concat {
                            RadzenUI.initialsAvatar pl.username
                            RadzenUI.text RadzenUI.body1 pl.username
                        })
                })))
            | _ -> empty ()
        RadzenUI.vStackGap "var(--gap-section)" (concat {
            RadzenUI.pageHeadingCrumb
                "Welcome to the gaming community!"
                (Some "Games we play, active servers, upcoming tournaments, and latest news.")
                []

            hero

            RadzenUI.fadeIn statsSection
            newsSection
            gamesSection
            serversSection
            tournamentsStrip
            leaderboardStrip
        })