# F# Style — Code Is Communication

> Source: *Stylish F# 6* (Kit Eason) — ch 1 (The Sense of Style), ch 3
> (Missing Data), ch 5 (Immutability and Mutation), ch 6 (Pattern
> Matching). *The Elmish Book* parts 1–2 practice the same idioms.

The aim of style is not elegance for its own sake — it is to
**communicate** with peers and with the computer. A style guide *suggests*;
it does not mandate.

```mermaid
flowchart LR
    n1(["Stylish F#"])
    n2["Style = communication"]
    n3["understanding beats obedience"]
    n4["semantic focus"]
    n5["motivational transparency"]
    n6["complexity explosion"]
    n7["Design functions with types"]
    n8["sketch the signature first"]
    n9["make illegal states unrepresentable"]
    n10["iterate: what did the domain teach?"]
    n11["Missing data: Option, not null"]
    n12["null: a billion-dollar mistake"]
    n13["type Option&lt;'T&gt; = Some | None"]
    n14["match on Some/None"]
    n15["map/bind — compose handling"]
    n16["no .Value, no .IsSome checks"]
    n17["design missing data OUT"]
    n18["Immutability as default"]
    n19["copies, not mutation"]
    n20["mutation: local + justified"]
    n21["Pattern matching"]
    n22["match = control + binding"]
    n23["active patterns"]
    n24["match on structure, not flags"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n2 --> n6
    n1 --> n7
    n7 --> n8
    n7 --> n9
    n7 --> n10
    n1 --> n11
    n11 --> n12
    n11 --> n13
    n11 --> n14
    n11 --> n15
    n11 --> n16
    n11 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n1 --> n21
    n21 --> n22
    n21 --> n23
    n21 --> n24
```

## Why style matters: the four traits of unreadable code

Bad code bases share four characteristics:

1. **Hard to work out what's going on** in any one piece — poor
   *semantic focus* (meaning is scattered across the code base).
2. **Hard to be sure a change has only the wanted effects** — side
   effects hide in distant code.
3. **Hard to be certain of the author's intent** — poor *motivational
   transparency*.
4. **Hard to tell whether the code is efficient** without experimenting.

Under deadline pressure, unexplainable code causes **complexity
explosions**: developers stop reasoning, duplicate code, "program by
coincidence", and pile on special cases. Style is the antidote.

## Design functions with types: the loop

Kit Eason's thought process for any function:

1. **Sketch the signature** — naive input/output types + a good name.
2. **Code the body**, naively if needed.
3. Ask: does the signature **cover the use cases and eliminate potential
   errors**? Refine signature, then body.
4. Ask: **what did the domain teach me?** New error cases → push them into
   the signature. Loop until stable.

```fsharp
// v1: accepts nonsense silently
let addInterest (interestType: int, amt: float, rate: float, y: int) = (* ... *)

// v2: the signature itself rules out bad input
type InterestType = Simple | Compound
let addInterest (kind: InterestType) (rate: Rate) (amount: Amount<positive>) (years: Years<positive>) = (* ... *)
```

## Missing data: `Option`, not null

- `null` was invented in 1965 and Hoare later called it his
  "billion-dollar mistake". F# pushes it to the edges of interop.
- `Option<'T>` is just a choice type: `Some of 'T | None`. Nothing magical.

```fsharp
// ❌ antipatterns
if opt.IsSome then opt.Value.ToUpper()   // lurking null-ref again
match opt with | Some x -> x | _ -> ""   // fine, but Option module is cleaner

// ✅ compose handling
opt |> Option.map String.upper |> Option.defaultValue ""
```

- **Best move: design missing data OUT.** `Delivery : string option` is
  unclear; a choice type states the business rules:

```fsharp
type DeliveryMethod =
    | Electronic            // no delivery address needed
    | Physical of Address   // must have one
```

## Immutability as default

- Mutation feels natural but forces you to *reason about history*.
  Immutable style: **create copies, don't change** — records give you
  `{ old with Field = newValue }`.
- Most code should be immutable; mutation only where performance or the
  operation itself demands it (see [functions-and-composition](../functions-and-composition/index.md)),
  and then keep it **local** and justified.

## Pattern matching everywhere

- `match` combines control flow, destructuring, and exhaustiveness
  checking in one construct — the compiler flags unhandled cases.
- Match on **structure and content**, never on boolean flags.
- **Active patterns** let you match on computed classifications:

```fsharp
let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd
match count with Even -> "even" | Odd -> "odd"
```

## When classes earn their place

- Records cover most data; **classes** are for when internal and external
  representations must differ, when state must be **held or mutated over
  time** (streams, connections, caches), or when interoping with OO
  hierarchies (F# classes can implement C# interfaces and vice versa).
- Keep the boundary crisp: immutable functional core, class-based
  **adapters** at the edges.

## Cross-links

- These types in domain modeling: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- Pattern matching the two-track model: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Currying / partial application (the machinery behind signatures): [functions-and-composition](../functions-and-composition/index.md).
- The same state-as-data thinking in the UI: [fable-and-elm-architecture](../fable-and-elm-architecture/index.md).
- Readability as ubiquitous language: [domain-driven-design](../domain-driven-design/index.md).
