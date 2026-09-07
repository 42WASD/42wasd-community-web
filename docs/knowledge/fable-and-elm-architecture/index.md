# Fable and The Elm Architecture — State, Message, Update, View

> Source: *The Elmish Book* (Zaid Ajaj) — part 1 (Understanding Fable) and
> part 2 (The Elm Architecture): the-architecture, counter, render-html,
> conditional-rendering, form-inputs, react-in-elmish, todo-app.

Fable compiles F# to JavaScript. The Elm Architecture (TEA) — via the
Elmish library — structures every UI as one loop: **state** in, **events**
in, **next state** out, **view** computed from state.

```mermaid
flowchart LR
    n1(["Fable + Elmish"])
    n2["Fable: F# → JavaScript"]
    n3["FCS parse → Fable AST → Babel"]
    n4["BCL where it makes sense"]
    n5["bindings for the rest"]
    n6["Feliz DSL for UI"]
    n7["TEA building blocks"]
    n8["State (Model): the data"]
    n9["Msg: events as choice type"]
    n10["update: Msg → State → State"]
    n11["render: State → ReactElement"]
    n12["mkSimple wires the loop"]
    n13["View dispatches"]
    n14["prop.onClick (fun _ -&gt; dispatch Inc)"]
    n15["render = pure function of state"]
    n16["Render mechanics"]
    n17["DSL mirrors HTML"]
    n18["conditional rendering: if/match"]
    n19["React re-renders efficiently"]
    n20["Inputs: events carry payloads"]
    n21["onTextInput → TextChanged of string"]
    n22["Practice: todo app"]
    n23["part 1: render a list"]
    n24["part 2: identify items (ids)"]
    n25["part 3: editing state per item"]
    n26["exercises: compiler-driven change"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n2 --> n6
    n1 --> n7
    n7 --> n8
    n7 --> n9
    n7 --> n10
    n7 --> n11
    n7 --> n12
    n7 --> n13
    n13 --> n14
    n13 --> n15
    n1 --> n16
    n16 --> n17
    n16 --> n18
    n16 --> n19
    n1 --> n20
    n20 --> n21
    n1 --> n22
    n22 --> n23
    n22 --> n24
    n22 --> n25
    n22 --> n26
```

## Fable: F# in the browser

- Pipeline: F# Compiler Services parses/type-checks → **Fable AST** →
  translated into **Babel AST** → JavaScript. Replacements map F#
  constructs to JS counterparts.
- BCL support is pragmatic, not total: `System.Random` works (maps to
  `Math.random()`); `System.Threading` threads and `System.IO` don't —
  JS runtimes are single-threaded and sandboxed. Where BCL can't reach,
  use **bindings** (`Fable.Browser.*`, `Fable.Node`).
- **Feliz** is the UI DSL: `Html.div [ prop.onClick ...; prop.text ... ]`
  — typed, composable, no stringly-typed markup.

## The Elm Architecture: four blocks

| Block | Type | Role |
| --- | --- | --- |
| **State** (Model) | a record | all data the app tracks |
| **Msg** | choice type | every event that can change state |
| **update** | `Msg -> State -> State` | computes the *next* state |
| **render** (View) | `State -> (Msg -> unit) -> ReactElement` | computes the UI from state |

```fsharp
type State = { Count: int }
type Msg = Increment | Decrement

let update msg state =
    match msg with
    | Increment -> { state with Count = state.Count + 1 }
    | Decrement -> { state with Count = state.Count - 1 }

let render (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.button [ prop.onClick (fun _ -> dispatch Increment); prop.text "+" ]
        Html.h1 state.Count
    ]

Program.mkSimple init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

Key properties:

- **update is synchronous and pure** — the same state + msg always gives
  the same next state. Trivially testable (see cross-links).
- **render is the only place that dispatches** — nothing happens unless
  the view (or a command) fires an event.
- The UI is a **snapshot of state**: render(state) is recomputed on every
  change; React diffs it efficiently.

## Rendering and inputs

- Conditional rendering is just F#: `if`/`match` produce different
  elements — show the "even" badge when `state.Count % 2 = 0`.
- Inputs carry payloads: `TextOfMonth of string` events flow through the
  same dispatch loop; the state stays the single source of truth.
- **Contrast with vanilla JS** (the book's first counter): DOM apps grab
  elements and attach handlers imperatively (`getElementsById`,
  `innerText`, callbacks) — state lives in the DOM and drifts. TEA
  inverts it: state lives in one value, and the DOM is recomputed from
  it. No stale-handler bugs, no state scattered across nodes.

## Practice: the todo app

The book builds a todo app in three escalating parts: render a list →
identify items by id (update in place) → edit items (per-item local
state). Then exercises flip the flow: change a type (`int` → `Guid`) and
let **the compiler enumerate every place to fix**.

## Cross-links

- Modeling the state with domain types: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- When update needs side effects (HTTP/timers): [commands-and-async-data](../commands-and-async-data/index.md).
- Splitting one program into many: [scaling-and-routing](../scaling-and-routing/index.md).
- The build pipeline that serves this: [dev-workflow](../dev-workflow/index.md).
- `update` = pure transition functions: [functions-and-composition](../functions-and-composition/index.md).
