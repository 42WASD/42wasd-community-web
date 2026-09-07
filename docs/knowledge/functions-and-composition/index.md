# Functions and Composition — Small Parts, Big Programs

> Source: *Stylish F# 6* (Kit Eason) — ch 4 (Collection Functions), ch 9
> (Programming with Functions), ch 12 (Performance). *DMMF* ch 8
> (Understanding Functions) for total functions; *The Elmish Book* uses
> these idioms throughout.

Functions are things: they can be named, passed, returned, and combined.
Composition — building pipelines from small functions — is how F# programs
grow without collapsing.

```mermaid
flowchart LR
    n1(["Functions &amp; composition"])
    n2["Functions are values"]
    n3["bind to a new name"]
    n4["pass as arguments"]
    n5["return from functions"]
    n6["Function types everywhere"]
    n7["a -&gt; b readable as a contract"]
    n8["type hints at boundaries"]
    n9["Currying &amp; partial application"]
    n10["one argument at a time"]
    n11["supply deps → specialized fn"]
    n12["collections: the vocabulary"]
    n13["map / filter / fold / groupBy"]
    n14["pipe chains read top-to-bottom"]
    n15["pick the right collection type"]
    n16["Total functions"]
    n17["handle every input"]
    n18["no exceptions, no nulls"]
    n19["restrict input, extend output"]
    n20["Composition operators"]
    n21["&gt;&gt; forward composition"]
    n22["|&gt; pipe"]
    n23["Performance discipline"]
    n24["measure first: BenchmarkDotNet"]
    n25["wrong collection type = O(n²)"]
    n26["string building: builder or Seq"]
    n27["abstraction vs efficiency trade-off"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n1 --> n6
    n6 --> n7
    n6 --> n8
    n1 --> n9
    n9 --> n10
    n9 --> n11
    n1 --> n12
    n12 --> n13
    n12 --> n14
    n12 --> n15
    n1 --> n16
    n16 --> n17
    n16 --> n18
    n16 --> n19
    n1 --> n20
    n20 --> n21
    n20 --> n22
    n1 --> n23
    n23 --> n24
    n23 --> n25
    n23 --> n26
    n23 --> n27
```

## Functions are values

```fsharp
let add a b = a + b
let addUp = add            // bind a function to another label
let applyAndPrint f a b = printfn "%i" (f a b)   // pass as argument
```

A function's type — `float -> float -> float` — is a **contract** readable
at a glance: two floats in, one float out. Write signatures first (see
[fsharp-style](../fsharp-style/index.md)); add explicit type hints where
inference would be ambiguous.

## Currying and partial application

- F# functions take arguments **one at a time**; supplying only some
  returns a specialized function.
- The idiomatic dependency-injection: dependencies first, data last.

```fsharp
let calculateShipping (rateTable: RateTable) (getNow: unit -> DateTime)
                      (order: Order) = (* ... *)

let calculate = calculateShipping myRates systemClock
// later, everywhere: calculate order — deps baked in, pure to the caller
```

## Total functions: restrict input, extend output

- **Partial** function: crashes on some inputs (head of empty list).
- **Total** function: defined for *every* input of its type — no
  exceptions, no nulls.
- Make functions total by **restricting input** (types that already
  guarantee validity) and **extending output** (`Option`/`Result` instead
  of throwing).

## The collection vocabulary

- Learn the core chords so they're instinctive: `map`, `filter`, `fold`,
  `groupBy`, `sortBy`, `sumBy`, `collect`.
- Pipes make chains read top-to-bottom:

```fsharp
lines
|> List.filter isCompleted
|> List.countBy dueDateBucket
|> List.sortByDescending snd
```

- Choose the right collection type: `List` = prepended, immutable stacks;
  `Array` = indexed mutation, performance; `Seq` = lazy pipelines;
  `Map`/`Set` = lookup.

## Composition operators

- `|>` pipes a value into a function — the data flows left-to-right.
- `>>` composes two functions into one — pipelines become values (see
  [workflows-and-error-handling](../workflows-and-error-handling/index.md)).

## Performance: measure, then fix

- Design is a **compromise between abstraction and efficiency** — F#'s
  high level can land you on the wrong end. Fix with measurement, not
  folklore: **BenchmarkDotNet**, and keep tests green while tuning.
- Classic traps: `List` where random access is needed (O(n) indexing →
  O(n²) loops), naive string concatenation in loops (quadratic copies),
  short-lived object churn in hot paths.

## Cross-links

- Signatures as design: [fsharp-style](../fsharp-style/index.md); total
  functions in pipelines: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Partial application as DI in workflows: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Composition of UI programs: [scaling-and-routing](../scaling-and-routing/index.md).
- Trade-off thinking (abstraction vs efficiency): [architecture-thinking](../architecture-thinking/index.md).
