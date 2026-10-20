# Architecture Crash Course

A distilled guide to five sources on building correct software, organized by
**knowledge topic** (not by book), in reading order from first principles to
production practice. The sources live in the repository under `knowledge/md/`,
`knowledge/src/the-elmish-book/`, and `knowledge/src/fsharp-docs/`. Every page
cross-references the other sources where the same concept appears — the course
is meant to be read as one connected body of knowledge.

**New to F#?** Read
[fsharp-for-imperative-programmers](fsharp-for-imperative-programmers/index.md)
first. It translates the syntax from Python, C, and C++, and names the four
ideas that are genuinely new.

```mermaid
flowchart LR
    n1(["Build correct software"])
    n2["Read F# first"]
    n3["let is a binding"]
    n4["currying and pipes"]
    n5["records and choice types"]
    n6["option and result"]
    n7["Think like an architect"]
    n8["Laws: everything is a trade-off"]
    n9["Why beats how"]
    n10["Decisions on a spectrum"]
    n11["Architecture vs design"]
    n12["Name what must be true"]
    n13["Characteristics: the -ilities"]
    n14["Fitness functions"]
    n15["Architecture quantum"]
    n16["Draw the lines"]
    n17["Cohesion and coupling"]
    n18["Conway&apos;s law"]
    n19["Understand the domain"]
    n20["Ubiquitous language"]
    n21["Business events"]
    n22["Bounded contexts"]
    n23["Discover it alone"]
    n24["Harvest the artifacts"]
    n25["Compiler interrogates"]
    n26["Model with types"]
    n27["AND types: records"]
    n28["OR types: choice types"]
    n29["Illegal states unrepresentable"]
    n30["Rules that always hold"]
    n31["Private constructors"]
    n32["Aggregates"]
    n33["Write idiomatic F#"]
    n34["Sense of style"]
    n35["Honest signatures"]
    n36["Missing data: option"]
    n37["Collections and their costs"]
    n38["Immutability by default"]
    n39["Exhaustive matching"]
    n40["Records and unions"]
    n41["Units of measure"]
    n42["Model workflows"]
    n43["Pipelines of pure steps"]
    n44["I/O sandwich"]
    n45["Compose functions"]
    n46["Failure as a value"]
    n47["Collect all errors"]
    n48["Trust boundary"]
    n49["DTOs and repositories"]
    n50["Evolve the model"]
    n51["Change the model first"]
    n52["Build the UI"]
    n53["Fable compiles F# to JS"]
    n54["Model, Msg, update, view"]
    n55["Commands for effects"]
    n56["Async as a state type"]
    n57["HTTP and JSON"]
    n58["Decode text into types"]
    n59["Compose pages"]
    n60["Routing is a type"]
    n61["Trace first, fix second"]
    n62["Govern and evolve"]
    n63["Choose a style"]
    n64["Patterns, not best practices"]
    n65["Record the why"]
    n66["Risk and negotiation"]
    n67["Test the rules"]
    n1 --> n2
    n1 --> n7
    n1 --> n12
    n1 --> n16
    n1 --> n19
    n1 --> n26
    n1 --> n30
    n1 --> n33
    n1 --> n42
    n1 --> n45
    n1 --> n46
    n1 --> n48
    n1 --> n50
    n1 --> n52
    n1 --> n61
    n1 --> n62
    n1 --> n67
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n2 --> n6
    n7 --> n8
    n7 --> n9
    n7 --> n10
    n7 --> n11
    n12 --> n13
    n12 --> n14
    n12 --> n15
    n16 --> n17
    n16 --> n18
    n19 --> n20
    n19 --> n21
    n19 --> n22
    n19 --> n23
    n23 --> n24
    n23 --> n25
    n26 --> n27
    n26 --> n28
    n28 --> n29
    n30 --> n31
    n30 --> n32
    n33 --> n34
    n33 --> n35
    n33 --> n36
    n33 --> n37
    n33 --> n38
    n33 --> n39
    n33 --> n40
    n33 --> n41
    n42 --> n43
    n43 --> n44
    n45 --> n46
    n46 --> n47
    n48 --> n49
    n50 --> n51
    n52 --> n53
    n52 --> n54
    n54 --> n55
    n55 --> n56
    n55 --> n57
    n57 --> n58
    n54 --> n59
    n59 --> n60
    n62 --> n63
    n62 --> n64
    n62 --> n65
    n62 --> n66
```

## Reading order (beginner → end)

### Part 0 — Read F# first

1. **[fsharp-for-imperative-programmers](fsharp-for-imperative-programmers/index.md)** —
   the syntax and the four genuinely new ideas, translated from Python, C,
   and C++: `let` as a binding, `unit`, currying, `fun`/`|>`/`>>`, records,
   choice types, `Option`, `Result`, linked lists, modules, `'T`, and units
   of measure. **Read this first** if you are new to F# — every later page
   assumes it. *(F# Documentation: tour, functions, records,
   discriminated-unions, options, units-of-measure.)*

### Part I — Think like an architect

2. **[architectural-thinking](architectural-thinking/index.md)** — what an
   architect is, the three laws as tests, architecture vs design, breadth
   over depth, and a worked trade-off. *(FSA ch 1–2, 27.)*
3. **[architectural-characteristics](architectural-characteristics/index.md)** —
   naming the "-ilities", translating business concerns, measuring with
   metrics, governing with fitness functions, and the architecture quantum.
   *(FSA ch 4–7.)*
4. **[modularity-and-components](modularity-and-components/index.md)** —
   cohesion and coupling, technical vs domain partitioning, Conway's Law,
   components as the architect's unit, and the coupling metrics.
   *(FSA ch 3, 8.)*

### Part II — Understand the problem first

5. **[domain-driven-design](domain-driven-design/index.md)** — shared
   models, business events, subdomains, bounded contexts, ubiquitous
   language, and the functional architecture. *(DMMF ch 1–3.)*
6. **[discovering-a-domain-alone](discovering-a-domain-alone/index.md)** —
   the solo-developer companion: harvest the field's artifacts as your
   domain expert, event-storm alone, and let the compiler interrogate the
   model. *(Synthesis: DMMF ch 1–2 + Brandolini, solo-adapted.)*
7. **[modeling-with-types](modeling-with-types/index.md)** — AND types, OR
   types, the refined-type process, wrapping primitives, smart constructors,
   value objects vs entities. *(DMMF ch 4–5, 9; SF6 ch 2.)*
8. **[integrity-and-invariants](integrity-and-invariants/index.md)** — the
   verified-email bug, internal vs external integrity, aggregates as
   consistency boundaries, and why consistency is a business term.
   *(DMMF ch 6.)*

### Part III — Write idiomatic F#

9. **[sense-of-style](sense-of-style/index.md)** — naming, layout, types as
   documentation, and comments that explain why rather than what.
   *(SF6 ch 1–2.)*
10. **[designing-functions](designing-functions/index.md)** — honest
   signatures, `Option` vs `Result` for failure, currying and argument
   order, total functions, and when to be generic. *(SF6 ch 2, 7–8.)*
11. **[missing-data](missing-data/index.md)** — the null problem, `Option`
    as a choice type, `map` vs `bind`, when `option` is the wrong tool, and
    interop at the edge. *(SF6 ch 6–7.)*
12. **[collections](collections/index.md)** — lists, arrays, and sequences;
    the cost of each operation; the four workhorses; laziness surprises.
    *(SF6 ch 4–5.)*
13. **[immutability-and-state](immutability-and-state/index.md)** —
    copy-and-update, structural sharing, state as a value, where mutation is
    still right, and concurrency for free. *(SF6 ch 5, 11.)*
14. **[pattern-matching](pattern-matching/index.md)** — exhaustiveness as a
    requirements checklist, the pattern vocabulary, active patterns, and
    when a partial match is legitimate. *(SF6 ch 6, 12.)*
15. **[records-and-unions](records-and-unions/index.md)** — the two building
    blocks, single-case wrappers, when a class earns its place, and modules
    for functions. *(SF6 ch 3, 8.)*
16. **[units-and-measures](units-and-measures/index.md)** — dimensional
    safety at compile time, explicit conversions, where units pay off most,
    and their limits. *(SF6 ch 2, 10.)*

### Part IV — Build the application

17. **[workflows-as-pipelines](workflows-as-pipelines/index.md)** —
    splitting a workflow into typed steps, the I/O sandwich, injecting
    dependencies as functions, and short-circuiting on error.
    *(DMMF ch 7–9.)*
18. **[functions-and-composition](functions-and-composition/index.md)** —
    pipe vs compose, partial application, composition with effects, and
    where composition stops paying. *(SF6 ch 9; DMMF ch 8.)*
19. **[errors-and-result](errors-and-result/index.md)** — failure as a
    value, `Option` vs `Result`, `bind`, collecting all errors, and the two
    levels of error. *(DMMF ch 8, 10; SF6 ch 11.)*
20. **[serialization-and-persistence](serialization-and-persistence/index.md)** —
    DTOs as a separate shape, two total mapping functions, versioning,
    repository boundaries, and event sourcing's honest trade.
    *(DMMF ch 11–13.)*
21. **[evolving-a-design](evolving-a-design/index.md)** — changing the model
    before the code, naming a category with an active pattern, and letting
    the compiler carry the change. *(DMMF ch 13.)*

### Part V — Build the UI

22. **[fable-and-elm](fable-and-elm/index.md)** — the four blocks, a
    complete counter program, why the loop fixes UI inconsistency, and how
    Fable runs. *(Elmish Book parts 1–2.)*
23. **[commands-and-async](commands-and-async/index.md)** — commands as
    values, `Cmd.OfAsync.either`, loading state as a type, `Deferred`, and
    composing commands. *(Elmish Book part 3.)*
24. **[http-and-json](http-and-json/index.md)** — modelling the payload as
    records, parsing in `update`, two failure sources, and the decode
    boundary. *(Elmish Book part 3, §HTTP and §Using Thoth.Json.)*
25. **[scaling-and-routing](scaling-and-routing/index.md)** — composing
    pages, the two composition forms, the Intent pattern, and the URL as a
    type. *(Elmish Book part 4.)*
26. **[dev-workflow](dev-workflow/index.md)** — trace first, fix second;
    turning the trace on; structuring for fast feedback; debugging in order.
    *(Elmish Book part 5.)*

### Part VI — Govern and evolve

27. **[architecture-styles](architecture-styles/index.md)** — layered,
    modular monolith, pipeline, microkernel, event-driven, service-based,
    microservices, and how to choose by elimination. *(FSA ch 9–19.)*
28. **[architecture-patterns](architecture-patterns/index.md)** — reuse,
    orchestration vs choreography, CQRS, and broker topologies; why a
    pattern is not a best practice. *(FSA ch 20.)*
29. **[decisions-and-risk](decisions-and-risk/index.md)** — the ADR, when a
    decision deserves one, immutability of records, risk matrices, and
    fitness functions that make decisions stick. *(FSA ch 21–23.)*
30. **[teams-and-negotiation](teams-and-negotiation/index.md)** — trading
    instead of refusing, knowing your audience, consensus without
    committee, and the 4 Cs. *(FSA ch 24–26.)*
31. **[testing](testing/index.md)** — why pure functions test easily, what
    the compiler cannot check, property-based tests, what not to test, and
    fitness functions as tests. *(DMMF ch 6, 9; FSA ch 24.)*

## Source map (claim → book/chapter)

Rule B5 of the distillation ruleset requires every claim to be traceable.
This table is the verifiable index — check any page against it.

| Page | DMMF | FSA | SF6 | Elmish Book | F# Docs |
| --- | --- | --- | --- | --- | --- |
| fsharp-for-imperative-programmers | — | — | 1, 2 | — | tour, functions, records, DUs, options, UoM |
| architectural-thinking | — | 1, 2, 27 | — | — | — |
| architectural-characteristics | — | 4, 5, 6, 7 | — | — | — |
| modularity-and-components | — | 3, 8 | — | — | — |
| domain-driven-design | 1, 2, 3 | — | — | — | — |
| discovering-a-domain-alone | 1, 2 | — | — | — | — |
| modeling-with-types | 4, 5, 6, 9 | — | 2 | — | records, DUs |
| integrity-and-invariants | 6 | — | — | — | access-control |
| sense-of-style | — | — | 1, 13 | — | style-guide |
| designing-functions | 8 | — | 2, 9 | — | functions, parameters |
| missing-data | — | — | 3, 7 | — | options |
| collections | — | — | 4, 5, 12 | — | lists, arrays, sequences |
| immutability-and-state | — | — | 5, 10 | — | copy-and-update, async |
| pattern-matching | — | — | 6, 13 | — | pattern-matching, active-patterns |
| records-and-unions | 4, 9 | — | 7, 8 | — | records, discriminated-unions |
| units-and-measures | 6 | — | 2 | — | units-of-measure |
| workflows-as-pipelines | 7, 8, 9 | — | — | — | — |
| functions-and-composition | 8 | — | 9 | — | functions, pipeline |
| errors-and-result | 8, 10 | — | 11 | — | results |
| serialization-and-persistence | 11, 12, 13 | — | — | — | — |
| evolving-a-design | 13 | — | — | — | active-patterns |
| fable-and-elm | — | — | — | `elm/*`, `fable/*` | — |
| commands-and-async | — | — | — | `commands/*` | computation-expressions, async |
| http-and-json | — | — | — | `commands/http.md`, `using-json.md` | — |
| scaling-and-routing | — | — | — | `scaling/*` | — |
| dev-workflow | — | — | — | `elm/project-structure.md` | — |
| architecture-styles | — | 9–19 | — | — | — |
| architecture-patterns | — | 20 | — | — | — |
| decisions-and-risk | — | 20, 21, 22, 23 | — | — | — |
| teams-and-negotiation | — | 23, 24, 25, 26 | — | — | — |
| testing | 6, 9 | 24 | — | — | — |

### Known gaps

Chapters read but **not** distilled into a page, with the reason:

| Source | Chapter | Why not covered |
| --- | --- | --- |
| FSA | 17 «Orchestration-Driven SOA» | Historical; the book itself treats it as superseded |
| FSA | 26 «Architectural Intersections» | Touches engineering practices beyond this course's scope |
| FSA | 27 (partly) | Its laws are on `architectural-thinking`; the rest is a worked case study |
| SF6 | 12 «Performance» | Only the collection-cost rule is used, on `collections` |
| SF6 | 14 «Summary» | Recap of chapters already covered |
| Elmish | `dev-flow/*` | Describes a webpack toolchain this repo does not use |
| Elmish | `elm/todo-app*.md` | Worked exercise series; rule A8 forbids exercises |
| F# Docs | `language-specification` | Written for compiler implementers, not readers |
| F# Docs | `query-expressions`, `type-providers`, `quotations`, `byrefs`, `structs` | Real F# features, unused in this repo and in the other sources |

## Concept cross-reference (where topics recur across books)

| Concept | FSA | DMMF | SF6 | Elmish Book |
| --- | --- | --- | --- | --- |
| Everything is a trade-off | ch 1, 2, 27 | ch 3 (architecture) | ch 12 (abstraction vs efficiency) | ch 2 (design choices) |
| Bounded contexts as modules | ch 7 (quanta), 18 (microservices) | ch 1–3 | — | part 4 (pages as programs) |
| Model with types, not flags | — | ch 4–6 | ch 2, 3, 7 | part 2 (State/Msg types) |
| Explicit errors over exceptions | ch 22 (risk) | ch 6, 10 | ch 8 | part 3 (Result in async) |
| Composition of small parts | ch 8 (components) | ch 7, 9 (pipelines) | ch 9 (composition), 4 | part 4 (nested programs) |
| Communicate intent | ch 21 (ADRs) | ch 2 (ubiquitous language) | ch 1 (semantic focus) | part 4 (Intent pattern) |
| Immutability / pure cores | — | ch 5, 9 | ch 5 | part 2 (state copies in update) |
| Measuring instead of guessing | ch 3 (metrics), 6 (fitness) | — | ch 10 (BenchmarkDotNet) | part 5 (bundle analysis) |
| Partitioning: technical vs domain | ch 9, 18 | ch 3 (vertical slices) | — | part 4 (page modules) |
| Data at the edges | ch 26 (data topologies) | ch 11–12 | — | part 3 (HTTP/JSON) |

## About this course

- Distilled from five sources: *Fundamentals of Software Architecture*
  (Richards & Ford), *Domain Modeling Made Functional* (Wlaschin),
  *Stylish F# 6* (Eason), *The Elmish Book* (Ajaj), and the official
  *F# Documentation* (Microsoft / .NET Foundation, CC BY 4.0). The source
  chapter is noted on every page so you can go deeper.
- Each page is a **tutorial**, not a reference: a story, numbered chunks
  with ❌/✅ worked examples, and a Revise flowchart at the end. There are
  no exercises.
- Every page ends with **Cross-links** to related knowledge wherever it
  lives in the course.
- The full-text search index over the source books
  (`knowledge/distill/_index/` in the repository) is repo-local tooling
  and is not published with the site.
