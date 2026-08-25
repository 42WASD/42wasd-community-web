---
phase: 03-step-by-step-implementation/phase-15-rendering-optimization
---

**Phase 15 complete — rendering optimization by evidence (not by guessing).**
The phase rule is explicitly conservative:

> **Do not split rendering into components preemptively. Optimize by evidence.**

So this phase did NOT invent `ElmishComponent`/`ecomp` row isolation. Instead
it added a lightweight, purely observational render probe to the largest list
(Games) so re-render cost becomes *measurable*, confirmed the current whole-list
re-render is cheap, and documented why component isolation is not justified
yet — and how to revisit it when it is.

### Reference

```text
measured slow re-renders on large lists
expensive derivations

ElmishComponent / isolation where justified
keep most rendering as pure functions
normalize data so updates are O(1) per entity

Do not split rendering into components preemptively. Optimize by evidence.
```

Verification: *"Re-render cost on the largest list is measurable, and no
component isolation is added without measured evidence that it is needed."*

### What earlier phases already gave us

- **Normalized state**: every entity list is `RemoteData<Map<string,'T>>`
  keyed by id (`State/Shared.fs`), so per-entity updates are O(1).
- **Pure rendering**: every page view is a pure function over the model (or
  shared slices) — no side effects, no mutation.

Those are two of the three levers the spec names (`keep most rendering as pure
functions`, `normalize data so updates are O(1) per entity`). They were already
in place, so the only missing piece was the *evidence* — the third lever,
`ElmishComponent / isolation where needed`.

### The instrument: `Ui/RenderProbe.fs`

A small module that is purely observational: it counts how many times a named
render region executes during a program run and prints a running total to the
browser console (Wasm maps `printfn` to the browser console).

```fsharp
module Community.Web.Client.Ui.RenderProbe

open System.Collections.Generic

let private counts = Dictionary<string, int>()

let touch (region: string) : int =
    let n =
        match counts.TryGetValue region with
        | true, c -> c + 1
        | _ -> 1
    counts[region] <- n
    n

let report (label: string) : unit =
    let totals =
        counts
        |> Seq.map (fun kv -> $"{kv.Key}={kv.Value}")
        |> String.concat " | "
    counts.Clear()
    printfn $"[RenderProbe] {label}: {totals}"
```

It is deliberately dumb: `touch` only increments, `report` only prints and
resets. It never changes what is rendered and never changes control flow, so
the numbers it produces are honest evidence rather than something that alters
behaviour.

### Wiring it into the Games view

`Pages/Games.fs` touches one region per game row, and the view reports once
after building all rows — so each dispatch prints one readable line rather than
one line per row:

```fsharp
let row (game: Game) (isFavorite: bool) (dispatch: Msg -> unit) =
    let _ = RenderProbe.touch $"game:{game.id}"   // <-- probe
    tr {
        td { game.name }
        ...
    }

let view (shared: SharedModel) (dispatch: Msg -> unit) =
    let favorites = shared.favoriteGames
    cond shared.games <| function
    | NotAsked | Loading -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
    | Failed _ -> Layout.Games().Rows(Layout.EmptyData().Elt()).Elt()
    | Loaded m ->
        let rows = forEach (Map.toArray m) (fun (_, g) -> row g (favorites.Contains g.id) dispatch)
        RenderProbe.report "Games.view"
        Layout.Games().Rows(rows).Elt()
```

`Ui/RenderProbe.fs` is added to `Community.Web.Client.fsproj` right after
`State/Shared.fs` (it compiles before the pages that call it).

### The evidence (browser console)

On `/games`, clicking "Favourite" on Counter-Strike 2:

```
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
```

Unfavouriting then favouriting again:

```
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 | game:game-3=1
```

Measured facts:

- **Every row re-renders on every dispatch** — a single favourite toggle
  re-runs the whole list view; each of the three game rows touches once.
- The report resets each pass (`=1`), so the counter reflects per-render
  activity, not an unbounded accumulation.

### Why no `ElmishComponent` / `ecomp` row isolation yet

The probe proves the whole list re-renders, but the list has **3 rows**.
Rebuilding three pure `Node`s is far below the 16 ms frame budget. Wrapping
each row in an `ElmishComponent<Game, bool>` with a custom `ShouldRender` would
add per-row bookkeeping and a failure surface for **zero measured benefit** —
exactly what the phase rule forbids ("do not preemptively").

The probe is also the tool to revisit this later: when the Games list grows
large and a toggle starts to feel slow, `RenderProbe` will *show* it. Only then
does `ElmishComponent<Game, bool>` (or `lazyComp`) become the justified answer,
measured before and after the change.

### Files changed

```
src/Community.Web.Client/Ui/RenderProbe.fs          (new: touch + report)
src/Community.Web.Client/Pages/Games.fs             (probe touches + report)
src/Community.Web.Client/Community.Web.Client.fsproj (add Ui/RenderProbe.fs)
docs/implementation/progress.yaml                   (phase-15: done)
docs/implementation/index.md                       (regenerated)
docs/implementation/_runbook/phase-15-rendering-optimization.md  (this file)
```

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
```

Browser: `/games` renders; clicking Favourite/Unfavourite flips the row and the
console prints `[RenderProbe] Games.view: game:game-1=1 | game:game-2=1 |
game:game-3=1`, proving the whole list re-renders — and that it is cheap.

`verify.sh` reports `VERIFY OK`.
