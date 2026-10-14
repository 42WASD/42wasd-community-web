# Workflows and Error Handling — The Two-Track Model

> Source: *Domain Modeling Made Functional* (Scott Wlaschin) — ch 7
> (Modeling Workflows as Pipelines), ch 10 (Implementation: Working with
> Errors). *Stylish F# 6* ch 11 (Railway Oriented Programming) is the same
> idea told as a widget factory; *The Elmish Book* part 3 applies it to
> async UI updates.

Every business workflow is a **pipeline of functions**: validate →
calculate → store → notify. Error handling is not an afterthought — the
pipeline has two tracks, and every function must handle both.

## The ladder of returns

Real code from this repo (`App/App.fs`) — the shape every async fetch is
forced into when errors are returned "manually":

```fsharp
// ❌ actual repo shape: each step must check + unwrap + re-branch
let result = fetchTournaments ()
match result with
| Ok tournaments ->
    let result2 = validate tournaments
    match result2 with
    | Ok valid ->
        let result3 = persist valid
        match result3 with
        | Ok _ -> "done"
        | Error e -> show e
    | Error e -> show e
| Error e -> show e
```

Every step adds a nesting level, and two thirds of the code is error
plumbing — DMMF's exact complaint about `validateOrder`. The happy path
(the steps you actually care about) drowns. This page is the fix.

## Workflows as pipelines

- A workflow = a function per step, each with **clear input and output
  types** (chosen in [domain-modeling-with-types](../domain-modeling-with-types/index.md)).
- Dependencies (repositories, SMTP, clock) are **parameters, injected
  first** via partial application — I/O stays at the edges.
- Effects are **documented in the type**: `Result<_,_>` for domain errors,
  `Async<_>` for I/O, `AsyncResult<_,_>` for both, plain values for pure
  logic. You can read a signature and know what can go wrong.
- The book's running example: the **place-order workflow** —
  `validateOrder → priceOrder → acknowledgeOrder`. Small helpers compose
  into steps; steps compose into the workflow (`checkAddressExists`,
  `checkProductCodeExists` feed `validatedOrder`; pricing turns
  `ValidatedOrder` into `PricedOrder`). Naming each step after the domain
  event it produces keeps code and ubiquitous language aligned.

## 1. The two-track model: stop nesting, start composing

A **`Result`** is a two-case type: `Ok of 'Success | Error of 'Failure` —
success and failure are both *values on a track*, not a nest of ifs. A
normal function is one piece of track; a function returning `Result` is a
**switch** — one input, two outputs. DMMF's railway picture: connect the
switches so the failure track **bypasses** every later step.

The one adapter that makes switches compose (DMMF ch 10, verbatim) —
it's called **`bind`**:

```fsharp
// ✅ from DMMF ch 10 — if Ok, run the next switch; if Error, bypass it
let bind switchFn twoTrackInput =
    match twoTrackInput with
    | Ok success   -> switchFn success
    | Error failure -> Error failure
```

And **`map`** — lift a normal (one-track) function onto the success track:

```fsharp
// ✅ from DMMF ch 10
let map f aResult =
    match aResult with
    | Ok success   -> Ok (f success)
    | Error failure -> Error failure
```

The nesting-ladder story collapses to a straight pipe:

```fsharp
// ✅ same workflow as the opening ladder — the happy path reads like a spec
input
|> fetchTournaments      // one-track in, two-track out: a switch
|> Result.bind validate
|> Result.bind persist
```

## 2. The types do the wiring

On the success track the type may change at every step — as long as each
output feeds the next input, `bind` composes them (DMMF's demo):

```fsharp
// ✅ from DMMF ch 10 — output of A feeds input of B, and so on
type FunctionA = Apple   -> Result<Bananas, ...>
type FunctionB = Bananas -> Result<Cherries, ...>
type FunctionC = Cherries-> Result<Lemon, ...>

let functionABC input =
    input
    |> functionA
    |> Result.bind functionB
    |> Result.bind functionC
```

On the failure track, the error type must be **the same all the way
along**. Different steps raise different errors, so unify them with a
choice type — and **`mapError`**, the failure-track twin of `map`:

```fsharp
// ✅ from DMMF ch 10
let mapError f aResult =
    match aResult with
    | Ok success   -> Ok success
    | Error failure -> Error (f failure)
```

## 3. Error types are domain types

Errors are part of the domain, so give them types like anything else: one
**choice type per workflow**, each case carrying what the caller needs —
never strings:

```fsharp
// ✅ DMMF's place-order workflow errors — matchable, self-describing
type PlaceOrderError =
    | Validation    of ValidationError list
    | Pricing       of PricingError
    | RemoteService of ServiceError
```

Domain errors are *expected and handled*; infrastructure failures
(network down) and bugs are separate concerns with separate mechanisms —
don't blur them.

## 4. Adapters for the awkward parts

An **adapter** is a small function that converts between shapes the
pipeline can't connect directly. Real code throws exceptions — wrap any
throwing API so it joins the tracks:

```fsharp
// ✅ DMMF's exception adapter: a throwing function becomes a switch
let tryGet (f: unit -> 'a) : Result<'a, exn> =
    try f () |> Ok
    with ex -> Error ex
```

**`tee`** performs a side effect (log, audit) *on the way through* and
passes the value on unchanged — dead-ends become links.

## 5. Computation expressions: hide the plumbing

A **computation expression** (CE) is F# syntax (`result { ... }`) that
lets `bind`/`map` chains be written as straight-line code. Inside the CE,
**`let!`** means "unwrap the Result; if Error, stop here" — the failure
track short-circuits without you writing it:

```fsharp
// ✅ DMMF ch 10 — the ladder again, this time flat
result {
    let! validated = validate unvalidatedOrder
    let! priced    = priceOrder validated
    return acknowledge priced
}
```

The same pattern scales: `asyncResult` layers I/O on top, and
`Result.traverse` turns a *list* of Results into a Result of list
(validate many order lines at once — all errors collected, not just the
first).

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Workflows &amp; errors"])
    n2["Pipeline model"]
    n3["steps with clear input/output"]
    n4["dependencies first (partial application)"]
    n5["effects documented in the type"]
    n6["Two-track model"]
    n7["success track + failure track"]
    n8["switch: one-track → two-track"]
    n9["bind connects switches"]
    n10["map: single-track only"]
    n11["mapError: unify error types"]
    n12["Error types are choice types"]
    n13["one DU per workflow"]
    n14["each case carries context"]
    n15["no stringly-typed errors"]
    n16["Adapters"]
    n17["exception adapter (try/with)"]
    n18["tee: side effect, pass through"]
    n19["Computation expressions"]
    n20["result { let! } hides the plumbing"]
    n21["asyncResult for I/O steps"]
    n22["traverse lists of Results"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n1 --> n6
    n6 --> n7
    n6 --> n8
    n6 --> n9
    n6 --> n10
    n6 --> n11
    n1 --> n12
    n12 --> n13
    n12 --> n14
    n12 --> n15
    n1 --> n16
    n16 --> n17
    n16 --> n18
    n1 --> n19
    n19 --> n20
    n19 --> n21
    n19 --> n22
```

## Cross-links

- The types flowing through the pipeline: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- ROP factory analogy with the same adapters: *SF6* ch 11 (source).
- The same start/finish Result pattern in UI updates: [commands-and-async-data](../commands-and-async-data/index.md).
- Total functions / composition operators: [functions-and-composition](../functions-and-composition/index.md).
- Why pipelines beat layers: [domain-driven-design](../domain-driven-design/index.md), [architecture-styles](../architecture-styles/index.md) (pipeline style).
