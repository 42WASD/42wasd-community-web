---
phase: 03-step-by-step-implementation/phase-17-design-system
---

**Phase 17 complete — design system.** This phase applies a consistent visual
design on top of the proven architecture without changing state ownership.
Direction: **gaming-community (dark-first)** with the **42 Abu Dhabi brutalist
palette**, implemented through the **Radzen Blazor** component library behind
thin F# wrappers.

### Reference

```text
- theme tokens (typography, spacing, colors)
- layout components (header, footer, nav)
- reusable surface components
```

Rule: *the visual design must not change state ownership — apply the theme on
top of the architecture.* The layout shell, MVU messages, shared caches, and
page/feature ownership are untouched; only presentation layers change.

### Direction chosen

From the design language section, the **gaming-community (dark-first)**
direction was selected and tuned to the **42 Abu Dhabi brutalist palette**:

| Token | Value | Use |
|---|---|---|
| Terminal Black | `#000000` | page / body background |
| Pure White | `#FFFFFF` | primary text |
| Abu Dhabi Red | `#BF0000` | primary accent, buttons |
| Charcoal | `#1A1A1A` | cards / panels |
| Muted Ash | `#A3A3A3` | secondary text |
| Neon Cyan | `#00E5FF` | optional accent (prize, pings) |

Brutalist treatment: `--rz-border-radius: 0px`, 1px solid borders, monospace
type (`Fira Code` / `JetBrains Mono`).

### The Radzen integration

`Radzen.Blazor 11.2.7` is the vendored component library (fork
`42WASD/radzen-blazor`, branch `jya0-v11.2.7` at tag `v11.2.7`, submodule under
`thirdparty/`). It is pulled in as a NuGet package and themed entirely via CSS
variable overrides in `:root` of the app's own stylesheet — the library CSS is
never hand-edited.

- `Startup.fs` registers services: `builder.Services.AddRadzenComponents()`.
- `Server/Index.fs` loads Radzen's `material-dark-base.css` theme (before the
  app's `index.css` so overrides win) and the Radzen JS bundle.
- `wwwroot/css/index.css` overrides the actual Radzen variables (verified in
  `material-dark-base.css`): `--rz-primary`, `--rz-body-background-color`,
  `--rz-base-background-color`, `--rz-panel-background-color`,
  `--rz-card-background-color`, `--rz-text-color`, `--rz-border-radius`, etc.

### The F# wrappers

`Ui/RadzenUI.fs` is the thin cross-feature wrapper module (this is the *only*
place the app touches Radzen directly; pages reuse the wrappers and never
`open Radzen`). Even the Radzen enums are re-exported so pages stay oblivious
to the component library's object model:

```
let dangerButton = ButtonStyle.Danger     // re-exported enum values
let successButton = ButtonStyle.Success
let outlinedCard  = Variant.Outlined

let button text style onClick dispatch = comp<RadzenButton> {
    "Text" => text; "ButtonStyle" => style
    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClick ())) }
let card variant children = comp<RadzenCard> { "Variant" => variant; children }
let serverCard (server: GameServer) = ...  // RadzenCard Outlined + status dot
```

Pages consume only these names (`RadzenUI.button`, `RadzenUI.card`,
`RadzenUI.dangerButton`, ...) — the `open Radzen` / `Variant.` / `ButtonStyle.`
leaks at call sites are gone.

Notes verified in Radzen source:
- `RadzenButton.Click` is `EventCallback<MouseEventArgs>` → needs
  `open Microsoft.AspNetCore.Components.Web`.
- `RadzenCard` takes a `Variant` param and wraps children as `ChildContent`.
- The `comp<T>` builder wraps child nodes as `ChildContent`.
- Radzen is **view-only**: the wrappers return `Node`s and only ever render;
  `Startup.fs` registers DI, and no `DialogService`/`NotificationService` is
  invoked from `update`/`init`/`Cmd`. Any future Radzen side effect must be
  emitted as an async `Cmd`, never called in `view`.

### Reusable surfaces

Two "key pages" were given Radzen-backed surfaces; the rest keep their existing
Bulma styling (now themed by the palette overrides):

- **Servers** — `RadzenUI.serverCard` replaces the old table rows; each server
  is an outlined `RadzenCard` with a status dot and address.
- **Tournaments** — each tournament is a `RadzenUI.card Variant.Outlined`
  holding the prize (neon cyan) and a Radzen `Button` that dispatches
  `ToggleRegistration`. Clicking it still mutates the shared canonical cache —
  the cross-feature effect is unchanged; only the surface changed.

The shared templates were adapted so card views (divs, not `<tr>`s) have
containers: `main.html`'s Servers/Tournaments templates changed from `<table>`
to `<div class="server-list">` / `<div class="tournament-list">`.

### Files changed

```
thirdparty/radzen-blazor/              (new submodule → 42WASD/radzen-blazor @ v11.2.7)
.gitmodules                            (+ radzen-blazor entry)
src/Community.Web.Client/Community.Web.Client.fsproj  (+ Radzen.Blazor 11.2.7, + Ui/RadzenUI.fs)
src/Community.Web.Client/Startup.fs    (+ AddRadzenComponents)
src/Community.Web.Server/Index.fs      (+ material-dark-base.css, Radzen JS)
src/Community.Web.Client/wwwroot/css/index.css  (rewritten: 42 brutalist palette)
src/Community.Web.Client/Ui/RadzenUI.fs        (new: Radzen F# wrappers)
src/Community.Web.Client/Pages/Servers.fs      (Radzen server cards)
src/Community.Web.Client/Pages/Tournaments.fs  (Radzen card + buttons)
src/Community.Web.Client/wwwroot/main.html     (card-list containers)
docs/implementation/progress.yaml              (phase-17: done)
docs/implementation/index.md                  (regenerated)
docs/implementation/_runbook/phase-17-design-system.md  (this file)
```

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 21 still pass
```

Design confirmed live in the browser: black body background, `--rz-primary`
= `#BF0000`, Radzen CSS loaded, `.rz-card` surfaces present, and the
`ToggleRegistration` button round-trips the cross-feature update.

`verify.sh` reports `VERIFY OK`.

---

### Phase 17b — full Radzen conversion (responsive layout, zero custom CSS)

Follow-up pass (same phase, appended): every page and the app shell are now
built entirely from Radzen primitives behind the `RadzenUI` wrappers, and
`index.css` is reduced to palette tokens only. The Bulma templates and the
HTML template engine are gone.

#### The three design decisions

1. **Layout shell = `RadzenLayout`** (Header + Sidebar + Body + Footer). The
   sidebar auto-collapses below 768px (`ResponsiveMaxWidth`), and the hamburger
   (`RadzenSidebarToggle`) flips a `sidebarExpanded` bool held in the root
   `Model`. Radzen primitives, not custom CSS, provide all responsiveness.
2. **Radzen primitives + zero custom CSS.** `wwwroot/css/index.css` is cut from
   ~200 lines of hardcoded layout to ~60 lines of pure `:root` palette tokens
   plus the typeface rule. No `.sidebar` width, `.box`, `.title`, `.table`,
   `.server-card`, `.status-dot`, `.navbar` — those were Bulma's responsibility
   and are now Radzen's.
3. **All pages, not just the two key pages.** Home, Games, Servers,
   Tournaments, Members, Teams, About, and Account all render through Radzen
   wrappers now.

#### What changed

| Area | Before | After |
|---|---|---|
| App shell | `Layout` HTML template (`main.html`) | `RadzenLayout` shell in `Ui/Layout.fs` |
| Nav | `Layout.MenuItem()` template | `RadzenPanelMenuItem` + `RadzenPanelMenu` |
| Pages | `Layout.Home()/.Games()/...` templates | Radzen `vStack`/`row`/`column`/`card`/`text`/`button`/`skeleton` |
| Loading | `Layout.EmptyData()` | `RadzenSkeleton` |
| Errors | `Layout.ErrorNotification()` | `RadzenAlert` (non-dismissible) |
| Forms | `Layout.SignIn()/.AccountSignedIn()` | `RadzenTextBox`/`RadzenPassword`/`RadzenTextArea`/`RadzenButton` |
| Templates infra | `Ui/Templates.fs` + `wwwroot/main.html` | **deleted** (no HTML templates remain) |
| CSS | hardcoded layout rules | `:root` palette tokens only |

#### The new shell (`Ui/Layout.fs`)

```fsharp
let view (model: Model) (dispatch: Message -> unit) =
    RadzenUI.layout (concat {
        RadzenUI.header (concat {
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.sidebarToggle (fun () -> dispatch ToggleSidebar)
                RadzenUI.text RadzenUI.heading4 "42WASD"
            })
        })
        RadzenUI.sidebarExpanded model.sidebarExpanded (fun _ -> dispatch ToggleSidebar)
            (RadzenUI.panelMenu (concat { /* menuItem per page */ }))
        RadzenUI.body (cond model.page <| function
            | Home -> Home.view model.shared
            | ... )
        RadzenUI.footer (cond model.shared.error <| function
            | Some err -> RadzenUI.alert RadzenUI.dangerAlert err
            | None -> empty())
    })
```

`ToggleSidebar` was added to the root `Message` and `sidebarExpanded` to the
root `Model` — the only state-ownership change, and it's shell-only (sidebar
state is cross-feature UI, so it belongs at the root, not in any page).

#### Radzen API notes (verified in vendored source)

- `RadzenSidebarToggle.Click` is `EventCallback<EventArgs>` (not
  `MouseEventArgs`); `RadzenButton.Click` IS `EventCallback<MouseEventArgs>`.
- `RadzenAlert.Close` is a non-generic `EventCallback` — Bolero's
  `attr.callback` produces `EventCallback<'T>`, so the shared error alert is
  non-dismissible (`AllowClose=false`) to avoid the cast mismatch.
- `RadzenSidebar.Expanded`/`ExpandedChanged` is the two-way binding; the
  wrapper passes both to keep the shell controlled by Elmish state.
- `RadzenColumn` `SizeXS/SM/MD/LG` provide the responsive 12-col grid.
- `RadzenSkeleton` `SkeletonVariant` uses `Text/Circular/Rectangular` (not
  `Circle`/`Rectangle`).

#### Phase 17b files changed

```
src/Community.Web.Client/Ui/RadzenUI.fs    (+ alert, panel menu/menu item,
                                             sidebarExpanded, password wrappers)
src/Community.Web.Client/Ui/Layout.fs      (rewritten: RadzenLayout shell)
src/Community.Web.Client/Ui/Templates.fs   (deleted — no templates remain)
src/Community.Web.Client/wwwroot/main.html (deleted — no templates remain)
src/Community.Web.Client/Community.Web.Client.fsproj  (- Templates.fs)
src/Community.Web.Client/App/App.fs        (+ ToggleSidebar, sidebarExpanded)
src/Community.Web.Client/Pages/*.fs        (all 8 pages → Radzen primitives)
src/Community.Web.Server/Index.fs          (- Bulma navbar; keeps Radzen css/js)
src/Community.Web.Client/wwwroot/css/index.css  (slim to palette tokens)
```

#### Phase 17b verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 21 still pass
```

Confirmed live in the browser: the `RadzenLayout` shell renders with the
sidebar, the hamburger collapses/expands the nav, and all eight pages render
through Radzen cards/grid/buttons — including sign-in, favourite toggle, and
tournament registration (the cross-feature effects still update the shared
canonical caches exactly as before).

`verify.sh` reports `VERIFY OK`.

---

### Phase 17c — richer component surfaces (Tabs / Carousel / Timeline / ProgressBar)

Motivated by the community-landing research (Raider.IO/FACEIT patterns): the
app was a flat grid of cards. This phase adds density and hierarchy using
existing Radzen components that were already in the library but unused, and
surfaces the `News` slice that was loaded into state but never rendered.

#### The core gotcha: named `RenderFragment` parameters

Bolero's `comp<T> { children }` always binds trailing children to the
`ChildContent` parameter. Several Radzen containers read their items from a
**dedicated `RenderFragment` parameter instead of `ChildContent`**:

- `RadzenTabs` reads `RadzenTabsItem`s from its **`Tabs`** parameter.
- `RadzenCarousel` reads `RadzenCarouselItem`s from its **`Items`** parameter.
- `RadzenTimeline` reads `RadzenTimelineItem`s from its **`Items`** parameter.

So a wrapper that passes children via `ChildContent` silently renders an empty
container (the outer `<div>`/nav shows, but zero tabs/slides/entries). The fix
is a helper that binds nodes to a named fragment:

```fsharp
let fragmentParam (paramName: string) (children: Node) =
    Attr(fun receiver builder sequence ->
        builder.AddAttribute(sequence, paramName,
            RenderFragment(fun builder ->
                children.Invoke(receiver, builder, 0) |> ignore))
        sequence + 1)
```

Then `comp<RadzenTabs> { fragmentParam "Tabs" children }`. Because F# has no
forward references, `fragmentParam` must be declared **above** the wrappers
that use it.

**Key architectural constraint:** `comp<T>` body cannot contain
`yield!`/`if`/`match`, and a Radzen component's `Items`/`Tabs` children are
ordinary `Node`s — so build the item nodes *outside* the `comp` and pass them
in. This keeps the wrapper view-only (no page state needed; `RadzenTabs` with
`SelectedIndex` left at -1 auto-selects the first tab, so it works
**uncontrolled** — perfect for our feature-owned view-only pages).

#### New wrappers (Ui/RadzenUI.fs)

```
fragmentParam paramName children   # bind a Node to a named RenderFragment param
tabs items                         # RadzenTabs  (items via "Tabs")
tabItem text children              # RadzenTabsItem (Text + ChildContent)
timeline items                     # RadzenTimeline (items via "Items")
timelineItem label point children  # RadzenTimelineItem (Label + PointStyle)
carousel itemsPerPage items        # RadzenCarousel (items via "Items", PagerPosition bottom)
carouselItem children              # RadzenCarouselItem
progressBar value max style        # RadzenProgressBar (Value/Max are double → pass floats)
progressBarValue value max style   # RadzenProgressBar with ShowValue
```

Also re-exported enum values: `progressBarPrimary/Success/Danger/Warning/Info/
Dark` (ProgressBarStyle) and `pagerBottom` (PagerPosition).

#### Servers page → tabbed browser

`Servers.view` now groups servers by `gameId` and renders them under
`RadzenTabs` (one tab per game, in manifest order; servers with a gameId not
in the games map fall through to an "Other" tab). Each server card uses the
existing `badgePill` for status and a `progressBarValue` for `onlinePlayers /
maxPlayers` capacity, colouring toward red near full (`capacityStyle`).

#### Home page → landing dashboard

- Stat strip retained (Games / Players online / Open tournaments / Members /
  Favourites) with caption + heading.
- **Featured games `RadzenCarousel`** — cycles the games from the shared cache
  (`carouselItem` = a card with name / genre / description).
- **Live servers** strip — one row per server: name + status `badgePill` +
  `progressBarValue` capacity.
- **Latest news `RadzenTimeline`** — surfaces the previously-unused `News`
  slice: each entry shows its `publishedAt` date as the label and the title +
  body as the content. This is the first time `shared.news` renders anywhere.

All new views stay view-only, selecting canonical shared slices (per the
state-ownership model); none of the new components require page-local Elmish
state.

#### Files changed (Phase 17c)

```
src/Community.Web.Client/Ui/RadzenUI.fs  (+ fragmentParam + tabs/timeline/carousel/
                                             progressBar wrappers + progress enums)
src/Community.Web.Client/Pages/Servers.fs (group servers by game into RadzenTabs;
                                           capacity progress bar per card)
src/Community.Web.Client/Pages/Home.fs    (+ featured-games carousel, live-server
                                           strip, news timeline)
```

#### Verification

```bash
dotnet build Community.Web.sln          # 0 warnings, 0 errors
dotnet test tests/Community.Client.Tests/  # all 22 still pass
bash scripts/docs/verify.sh             # VERIFY OK
```

Verified live in the browser: the Servers page shows tabs (Counter-Strike 2 /
Dota 2 / Minecraft) and switching tabs shows each game's servers with capacity
bars; the Home page shows the featured-games carousel, the live-server strip,
and the news timeline. The MVU trace confirms the normal `Get*` → `Got*`
flow with no dropped messages.

---

## Post-phase note (2026-09-01): DialogService IS now invoked — as an Elmish Cmd

The "no `DialogService` … is invoked from `update`/`init`/`Cmd`" statement
above was true when written (only `NotificationService` existed, wired in a
later round). It is **outdated as of the Tournaments dialog work**: `Main.fs`
now resolves `DialogService` alongside `NotificationService` and opens the
tournament details dialog via `Cmd.ofEffect (fun _ -> …OpenAsync…)`.
The invariant that actually matters held then and still holds:

- **Never call Radzen services from `view` or pure `update`** — the view stays
  a pure `Node` function and `App.update` stays service-free/testable.
- Service calls live only in the **service-aware wrapper** in `Main.fs`
  (resolved from `this.Services`), emitted as `Cmd.ofEffect` commands layered
  on top of the pure update — exactly the "async Cmd" carve-out the original
  note anticipated.