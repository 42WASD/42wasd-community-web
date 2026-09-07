# Architecture Thinking — The Architect's Eye

> Source: *Fundamentals of Software Architecture* (Richards & Ford) —
> ch 1 (Introduction), ch 2 (Architectural Thinking), ch 27 (Laws of
> Software Architecture, Revisited).

Architecture is a role, not a promotion. An architect analyzes trade-offs,
guides (never specifies) technology choices, and owns the *why* behind
decisions. Most of the job is judgment under uncertainty.

```mermaid
flowchart LR
    n1(["Architecture thinking"])
    n2["Three laws"]
    n3["1. Everything is a trade-off"]
    n4["not a trade-off? you missed one"]
    n5["analysis is never done once"]
    n6["2. Why is more important than how"]
    n7["decisions carry context"]
    n8["3. Decisions live on a spectrum"]
    n9["rarely binary"]
    n10["architectural decision = every option has significant trade-offs"]
    n11["Architecture vs design"]
    n12["structure vs appearance"]
    n13["strategic vs tactical"]
    n14["effort to change"]
    n15["significance of trade-offs"]
    n16["Expectations of an architect"]
    n17["define decisions &amp; principles"]
    n18["guide, don't specify"]
    n19["continually analyze"]
    n20["know the business domain"]
    n21["stay hands-on"]
    n22["Breadth beats depth"]
    n23["know many solutions"]
    n24["frozen caveman antipattern"]
    n1 --> n2
    n1 --> n11
    n1 --> n16
    n1 --> n22
    n2 --> n3
    n3 --> n4
    n3 --> n5
    n2 --> n6
    n6 --> n7
    n2 --> n8
    n8 --> n9
    n8 --> n10
    n11 --> n12
    n11 --> n13
    n11 --> n14
    n11 --> n15
    n16 --> n17
    n16 --> n18
    n16 --> n19
    n16 --> n20
    n16 --> n21
    n22 --> n23
    n22 --> n24
```

## The three laws

- **Everything in software architecture is a trade-off.** Corollary 1: if
  you found something that isn't a trade-off, you haven't identified the
  trade-off *yet*. Corollary 2: you can't do trade-off analysis once and be
  done.
- ***Why* is more important than *how*.** Anyone can read *how* a system
  works; the decisions and their context are the hard-won knowledge.
- **Most architecture decisions aren't binary — they live on a spectrum.**
  Practical test: *a software architecture decision is one where each of
  the options has significant trade-offs.*

❌ "Should we use microservices or a modular monolith?" (binary framing)
✅ "Which characteristics matter most here, and what does each option cost
us on those?" (trade-off framing)

## Architecture vs design

- Architecture = **structure** (the house: floors, roof). Design =
  **appearance/details** (carpet, wall color).
- Most decisions sit on a spectrum between the two. Three questions locate
  a decision: strategic vs tactical? effort to change? significance of the
  trade-offs?
- Example: choosing microservices = architecture (structure). Choosing the
  UI framework = design (mostly).

## What an architect is expected to do

- **Define decisions and design principles** that *guide* technology
  choices. ❌ "Use React." ✅ "Use a reactive framework — React, Vue, Elm,
  Angular are all fine."
- **Continually analyze** the architecture; it's never finished.
- **Keep current with trends** — without chasing each one.
- **Ensure compliance** with decisions.
- **Know the business domain** — domain knowledge beats framework trivia.
- **Stay hands-on**: balance coding with leading (the book suggests ~50%).
- **Interpersonal skills**: negotiation, documentation, team guidance —
  ironically the hardest part of the job.

## Breadth beats depth

An architect needs a **wide breadth** of knowledge to see solutions others
miss. Beware the **frozen caveman antipattern**: an architect who
reflexively applies an outdated, hard-earned lesson from the past to every
situation.

## Laws in practice: a worked trade-off

**Shared library vs shared service** (ch 27):

| | Shared library | Shared service |
| --- | --- | --- |
| Coupling | build-time (compile-in) | runtime (network calls) |
| Consistency | each service can drift versions | single source of truth |
| Availability | no extra hop | adds a failure point |
| Change | redeploy every consumer | redeploy one service |

There is no "right" answer — the decision depends on change frequency,
team topology, and availability needs. That's the point: name the
trade-offs, pick the least-worst fit, write down the *why*.

## Cross-links

- The First Law in action when picking styles: [architecture-styles](../architecture-styles/index.md).
- Trade-offs inside a codebase (abstraction vs efficiency): [functions-and-composition](../functions-and-composition/index.md).
- Architecture decisions get recorded as ADRs: [architecture-decisions-and-soft-skills](../architecture-decisions-and-soft-skills/index.md).
- Domain knowledge also drives the model: [domain-driven-design](../domain-driven-design/index.md).
