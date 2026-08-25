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
place the app touches Radzen directly; pages reuse the wrappers):

```
let button text style onClick dispatch = comp<RadzenButton> {
    "Text" => text; "ButtonStyle" => style
    attr.callback "Click" (fun (_: MouseEventArgs) -> dispatch (onClick ())) }
let card variant children = comp<RadzenCard> { "Variant" => variant; children }
let serverCard (server: GameServer) = ...  // RadzenCard Outlined + status dot
```

Notes verified in Radzen source:
- `RadzenButton.Click` is `EventCallback<MouseEventArgs>` → needs
  `open Microsoft.AspNetCore.Components.Web`.
- `RadzenCard` takes a `Variant` param and wraps children as `ChildContent`.
- The `comp<T>` builder wraps child nodes as `ChildContent`.

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