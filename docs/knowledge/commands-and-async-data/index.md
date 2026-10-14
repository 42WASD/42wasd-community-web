# Commands and Async Data — Side Effects in the Loop

> Source: *The Elmish Book* (Zaid Ajaj) — part 3 (Commands in Elmish):
> commands, definition, async-updates, async-to-cmd, async-state,
> deferred-module-utilities, http, xhr-elmish, working-with-json,
> using-json, elmish-hackernews.

`update` is synchronous — so how does an app call an HTTP endpoint? With
**commands**: values that can dispatch messages later. The pattern is
always start-event → side effect → finish-event.

## Loading, forever

The mistake the whole part exists to kill — ad-hoc booleans for async
state. This repo's own type doc names it:

```fsharp
// ❌ from State/RemoteData.fs's doc comment — the shape to avoid:
//    ad-hoc `bool IsLoading` / `bool HasError` fields
{ data = None; isLoading = true; hasError = false }
// loading AND error? loaded but not shown? ambiguous — pick your bug
```

Two booleans give four combinations and answer none of the real questions:
*has it started? is it running? what came back?* The book's answer is one
type whose cases ARE the states. This page is that move, plus the command
machinery that fills those states honestly.

## 1. Why commands exist

`update : Msg -> State -> State * Cmd<Msg>` — the **state transition stays
synchronous** (easy to reason about and test); only the *dispatching* of
follow-up events happens asynchronously. A command, in the book's
definition, is *essentially a function that takes a `dispatch` function
and returns `unit`* — it decides **when** (now, later, never) to fire a
message:

```fsharp
// ✅ from The Elmish Book — the essence of a command
//           dispatch function
//                 ↓
val command : (Msg -> unit) -> unit

let none : Cmd<Msg> = Cmd.ofSub (fun (dispatch: Msg -> unit) -> ())
let ofMsg (msg: Msg) : Cmd<Msg> =
    Cmd.ofSub (fun dispatch -> dispatch msg)
```

Using commands extends the program triplet — `init : unit -> State *
Cmd<Msg>`, and every update arm returns `(nextState, cmd)`.

## 2. From `Async<'T>` to `Cmd<Msg>`

The book implements the adapter from scratch so the machinery is visible:

```fsharp
// ✅ from The Elmish Book (async-to-cmd.md)
let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
    let delayedCmd (dispatch: 'msg -> unit) : unit =
        let delayedDispatch = async {
            let! msg = operation
            dispatch msg
        }
        Async.StartImmediate delayedDispatch
    Cmd.ofSub delayedCmd
```

Write the effect once as `Async<'T>`; the command dispatches the finished
event with the result. Failures dispatch the error branch — no hidden
try/except.

## 3. Modeling asynchronous state

Every async operation becomes a **start/finish event pair**. Instead of
per-operation message pairs (which multiply), wrap the status:

```fsharp
// ✅ from The Elmish Book — start/finish pairs as one generic type
type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't
```

And the state that tracks it — the book's own `Deferred<'T>`, the exact
move this repo's `RemoteData<'T>` makes:

```fsharp
// ✅ from The Elmish Book — "data that will become available later"
type Deferred<'t> =
    | HasNotStartedYet
    | InProgress
    | Resolved of 't
```

`Resolved of Result<'t, 'error>` covers failure too — the UI matches on
`Deferred` to render exactly placeholder / spinner / data / error, with
"no need for extra `Loading` flags because the `InProgress` case covers
for that".

## 4. HTTP and JSON

- The browser primitive is `XMLHttpRequest` (despite the name, it carries
  JSON fine); bindings expose it to F#. Higher-level: `fetch` bindings.
- **Thoth.Json** decoders turn JSON into your types — parse, don't
  validate: JSON that doesn't fit becomes an `Error` you can render, not a
  runtime crash.

## 5. Practice: HackerNews clone

The public HackerNews API returns **ids first**: the `topstories` endpoint
yields 500 story ids, and each item needs its own request. The chapter
wires up: load ids → fan out per-story requests → async-recursive updates
→ render. The full command + Deferred + decoder toolkit in one realistic
app.

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Commands &amp; async data"])
    n2["Why: update is synchronous"]
    n3["state changes stay pure"]
    n4["dispatching happens async"]
    n5["Cmd: dispatch → unit lifted"]
    n6["Cmd.none = do nothing"]
    n7["Cmd.ofMsg = dispatch now"]
    n8["extended program signature"]
    n9["init/update return State * Cmd"]
    n10["Async&lt;'T&gt; → Cmd&lt;Msg&gt;"]
    n11["Async.StartImmediate + dispatch"]
    n12["Cmd.fromAsync helper"]
    n13["Model async state"]
    n14["start/finish event pairs"]
    n15["AsyncOperationStatus: Started | Finished"]
    n16["Deferred&lt;'T&gt;: InProgress | Resolved"]
    n17["Resolved wraps Result"]
    n18["HTTP from the browser"]
    n19["XMLHttpRequest is the primitive"]
    n20["fetch-style bindings"]
    n21["JSON with Thoth"]
    n22["decoders: JSON → types"]
    n23["Decode succeeds or fails"]
    n24["Practice: HackerNews"]
    n25["ids first, items per id"]
    n26["fan-out requests"]
    n27["async recursion over lists"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n1 --> n5
    n5 --> n6
    n5 --> n7
    n5 --> n8
    n8 --> n9
    n1 --> n10
    n10 --> n11
    n10 --> n12
    n1 --> n13
    n13 --> n14
    n13 --> n15
    n13 --> n16
    n16 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n1 --> n21
    n21 --> n22
    n21 --> n23
    n1 --> n24
    n24 --> n25
    n24 --> n26
    n24 --> n27
```

## Cross-links

- `Result`/`bind` — the same two-track shape: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Where the data comes from (DDD workflows server-side): [domain-driven-design](../domain-driven-design/index.md).
- Passing loaded data between pages: [scaling-and-routing](../scaling-and-routing/index.md).
- Bundling/webpack serving the app: [dev-workflow](../dev-workflow/index.md).
- Data at the edges as an architecture concern: [architecture-styles](../architecture-styles/index.md).
