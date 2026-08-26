module Community.Web.Client.Ui.RenderProbe

/// Lightweight, evidence-based rendering instrumentation for Phase 15.
///
/// The phase rule is: *do not split rendering into components preemptively;
/// optimize by evidence.* This module exists solely to make re-render cost
/// *measurable* in the browser console, so we can decide by evidence whether
/// an isolation mechanism (ElmishComponent / lazyComp) is ever justified.
///
/// It deliberately never changes what is rendered: it only counts how many
/// times a render path (e.g. a game row) is rebuilt during the current
/// program run, and prints a running total. Because our model is already
/// normalized (Map keyed by id → O(1) per-entity updates) and our renderers
/// are pure functions, the probe currently confirms the whole page view is
/// cheap to re-run — which is exactly the evidence that justifies NOT adding
/// component isolation yet.
///
/// The counter is a mutable, out-of-band diagnostic. It is therefore compiled
/// ONLY in DEBUG builds; in a Release build every entry point is a pure no-op
/// so production carries no instrumentation state. This keeps the runtime
/// purely functional while keeping the probe available for browser dev.

#if DEBUG

open System.Collections.Generic

let private counts = Dictionary<string, int>()

/// Record one render of the named region (e.g. a list row). Purely
/// observational: updates an out-of-band counter, never affects the nodes
/// being built. Returns nothing — the running total is logged by `report`.
let touch (region: string) : unit =
    let n =
        match counts.TryGetValue region with
        | true, c -> c + 1
        | _ -> 1
    counts[region] <- n

/// Log the running render totals to the browser console (WASM maps printfn
/// output to console.log), then reset. Call at a stable point — e.g. once per
/// page view after the rows have been built — so each dispatch produces one
/// readable line rather than one line per row.
let report (label: string) : unit =
    let totals =
        counts
        |> Seq.map (fun kv -> $"{kv.Key}={kv.Value}")
        |> String.concat " | "
    counts.Clear()
    printfn $"[RenderProbe] {label}: {totals}"

#else

/// No-op release stub: `touch` exists so DEBUG call sites compile unchanged,
/// but in production it does nothing.
let touch (_region: string) : unit = ()

/// No-op release stub for `report`.
let report (_label: string) : unit = ()

#endif