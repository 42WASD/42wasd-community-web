# Architecture Characteristics and Governance — Naming What Matters

> Source: *Fundamentals of Software Architecture* (Richards & Ford) —
> ch 3 (Modularity), ch 4–6 (Characteristics: Defined, Identifying,
> Measuring & Governing), ch 7 (Scope & Quanta), ch 8 (Component-Based
> Thinking).

Before choosing a style, name what the system must be good at — the
**-ilities** — then measure and *govern* them. Implicit requirements are
still requirements.

## A fitness function you already run

This repo's `bash scripts/docs/verify.sh` *is* the concept of this page,
running weekly: the golden tests regenerate nav from the manifest and
**fail on drift** — an objective, automated check that the docs'
structure stays consistent, with zero human judgment in the loop.

FSA's definition names exactly that shape: an **architectural fitness
function** is *any mechanism that provides an objective integrity
assessment of some architecture characteristic*. Tests, monitors,
regression suites — "not some new framework to download", a new
perspective on existing tools. This page is: name what matters (the
characteristics), measure it, and wire the check in so it runs itself.

## 1. What counts as an architecture characteristic

FSA's three-criteria test, verbatim — to count, a requirement must be:

> a **nondomain design consideration**, influence some **structural
> aspect** of the design, *and* be **critical or important to the
> application's success**.

- **Implicit** characteristics (availability, stability, security) appear
  in no requirements document yet decide whether the site works. Explicit
  ones (target response time, scalability) are specified.
- They aren't binary checkboxes: each is a spectrum with real trade-offs
  against the others (see [architecture-thinking](../architecture-thinking/index.md)).
- The catalog: **operational** (availability, performance, scalability,
  recoverability, robustness...), **structural** (extensibility,
  maintainability, portability...), plus cloud and cross-cutting lists.

## 2. Identifying: translate, then prioritize

- Stakeholders say "mergers, time-to-market, user satisfaction";
  architects say "interoperability, agility, performance". The book's
  translation table (ch 5, verbatim rows):

| Domain concern | Architectural characteristics |
| --- | --- |
| Mergers and acquisitions | Interoperability, scalability, adaptability, extensibility |
| Time to market | Agility, testability, deployability |
| User satisfaction | Performance, availability, fault tolerance, testability, deployability, agility, security |
| Competitive advantage | Agility, testability, deployability, scalability, availability, fault tolerance |
| Time and budget | Simplicity, feasibility |

- Beware **false equivalences** — *agility* is a **composite**
  characteristic composed of measurable things, not "time to market".
- Ask of each candidate: **architecture or design?** (structural support
  needed → architecture; Template Method suffices → design).
- Prioritize: an architecture supporting everything supports nothing.

## 3. Measuring modularity

| Metric | Meaning |
| --- | --- |
| **Cohesion** | related code stays together (a module does one thing) |
| **Afferent coupling** | how many components depend on *me* |
| **Efferent coupling** | how many components *I* depend on |
| **Abstractness** | abstract artifacts ÷ concrete artifacts |
| **Instability** | efferent ÷ (efferent + afferent) |
| **Distance from main sequence** | `D = |A + I − 1|` — 0 is the ideal abstractness/instability balance |

Metrics steer; they don't decide. Too many abstractions is as real a
failure as none.

## 4. Governance: fitness functions

- **Governance** = steering the development process so the important
  characteristics survive schedule pressure. Modularity is important but
  never urgent — hence it decays without a mechanism.
- The definition (ch 6, verbatim): *any mechanism providing an objective
  integrity assessment of some architecture characteristic* — the
  `verify.sh` story above is one; a dependency test that fails when the
  domain layer imports infrastructure; a performance regression suite; a
  monitoring alert on error rate.

## 5. Component-based thinking

- A **logical component** = a container of related responsibilities
  ("Reference Manager": GetData, SetData, ReloadCache...). Components are
  the level an architect *sees*; classes are the developer's level.
- The architect identifies components and their **roles and
  relationships**; developers own their internal design. (This is the
  "guide, don't specify" rule from [architecture-thinking](../architecture-thinking/index.md).)
- **Component coupling** matters even without direct communication —
  shared assumptions couple too.

## 6. Bounded contexts as quanta

- The **architecture quantum**: the smallest set of components that must
  deploy together with their data, synchronously coupled. A bounded
  context with its own store is usually a quantum.
- Quanta connect this page to DDD and to the style catalog: monoliths
  have one big quantum; microservices aim for many small ones.

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Characteristics &amp; governance"])
    n2["What are characteristics"]
    n3["operational: perf, availability"]
    n4["structural: modularity, coupling"]
    n5["implicit ones still bind you"]
    n6["explicit support vs implicit care"]
    n7["Identify from the domain"]
    n8["translate business talk → -ilities"]
    n9["user satisfaction → performance etc"]
    n10["design or architecture?"]
    n11["prioritize: most important ≤ 3"]
    n12["Measuring"]
    n13["cyclomatic complexity"]
    n14["coupling: afferent/efferent"]
    n15["abstractness &amp; instability"]
    n16["distance from main sequence"]
    n17["metrics have limits"]
    n18["Governance + fitness functions"]
    n19["govern = steer"]
    n20["fitness function: objective integrity check"]
    n21["any mechanism: tests, monitors"]
    n22["Component-based thinking"]
    n23["logical components ≠ classes"]
    n24["architect: roles &amp; relationships"]
    n25["component coupling"]
    n26["Modularity"]
    n27["cohesion: related together"]
    n28["coupling: dependencies out"]
    n29["bounded contexts = quanta"]
    n30["quantum: deploy + data + sync"]
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
    n1 --> n12
    n12 --> n13
    n12 --> n14
    n12 --> n15
    n15 --> n16
    n12 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n18 --> n21
    n1 --> n22
    n22 --> n23
    n22 --> n24
    n22 --> n25
    n1 --> n26
    n26 --> n27
    n26 --> n28
    n1 --> n29
    n29 --> n30
```

## Cross-links

- DDD's bounded contexts (the domain view): [domain-driven-design](../domain-driven-design/index.md).
- Where these constraints bite — style selection: [architecture-styles](../architecture-styles/index.md).
- Fitness functions automate decisions: [architecture-decisions-and-soft-skills](../architecture-decisions-and-soft-skills/index.md).
- Trade-off discipline: [architecture-thinking](../architecture-thinking/index.md).
- Measuring instead of guessing, at code level: [functions-and-composition](../functions-and-composition/index.md).
