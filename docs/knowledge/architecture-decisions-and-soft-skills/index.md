# Architecture Decisions and Soft Skills — The Human Layer

> Source: *Fundamentals of Software Architecture* (Richards & Ford) —
> ch 20–26: Architectural Patterns, Decisions (ADRs), Analyzing Risk,
> Diagramming, Making Teams Effective, Negotiation, Intersections.

Technical brilliance fails without communication. Decisions need records,
risk needs a shared language, and teams need the right kind of architect —
not too tight, not too loose.

```mermaid
flowchart LR
    n1(["Decisions &amp; soft skills"])
    n2["Architectural decisions"]
    n3["structure + tech with characteristics impact"]
    n4["guide teams, not dictate"]
    n5["ADRs: short records"]
    n6["Title/Status/Context/Decision/Consequences"]
    n7["+ Compliance &amp; Notes"]
    n8["Analytics &amp; fitness hooks"]
    n9["Analyzing risk"]
    n10["risk matrix: likelihood × impact"]
    n11["characteristics as criteria"]
    n12["risk storming: individual → consensus → mitigate"]
    n13["structural decay is risk too"]
    n14["Diagramming"]
    n15["C4: context/container/component/class"]
    n16["layers semantically, not decoratively"]
    n17["no irrational artifact attachment"]
    n18["Teams"]
    n19["control-freak: too tight"]
    n20["armchair: too loose"]
    n21["effective: right boundaries"]
    n22["collaboration + constraints"]
    n23["Negotiation"]
    n24["stakeholders, not just tech"]
    n25["data-driven persuasion"]
    n26["Intersections"]
    n27["implementation &amp; infrastructure"]
    n28["data topologies"]
    n29["engineering practices"]
    n30["team topologies &amp; integration"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n5 --> n6
    n5 --> n7
    n5 --> n8
    n1 --> n9
    n9 --> n10
    n9 --> n11
    n9 --> n12
    n9 --> n13
    n1 --> n14
    n14 --> n15
    n14 --> n16
    n14 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n18 --> n21
    n18 --> n22
    n1 --> n23
    n23 --> n24
    n23 --> n25
    n1 --> n26
    n26 --> n27
    n26 --> n28
    n26 --> n29
    n26 --> n30
```

## Making decisions: ADRs

- Architectural decisions shape **structure** or technology choices that
  impact characteristics; good ones **guide** teams to the right
  technical choices rather than dictating every detail.
- **Architecture Decision Records**: one- to two-page text/markdown per
  decision — **Title, Status, Context, Decision, Consequences** (+ the
  authors' additions: **Compliance** — how it's enforced, possibly via
  fitness functions — and **Notes**). ADRs fix the Second Law's problem:
  the *why* survives team turnover.

## Analyzing risk

- Risk = operational (availability, data integrity) **and** structural
  (coupling, decay). Use a **risk matrix** (likelihood × impact) with
  characteristics as assessment criteria.
- **Risk storming**: (1) individuals assess alone — no anchoring;
  (2) consensus on the hot spots; (3) mitigation plans. Repeat when the
  architecture changes.

## Diagramming

- **C4** (Simon Brown): zoom levels **Context → Container → Component →
  Class**. Use layers **semantically** (each layer = real meaning), not
  decoratively (boxes grouped for looks). Don't let one tool become an
  irrational artifact attachment; pick the diagram the audience needs.

## Teams: three architect personalities

| Personality | Boundaries | Result |
| --- | --- | --- |
| **Control freak** | too tight — writes the pseudocode, bans libraries | steals the craft; devs disengage |
| **Armchair** | too loose — absent, diagrams too high-level | team does the architect's work, confusion |
| **Effective** | right-sized constraints | guidance + availability + roadblocks removed |

## Negotiation

- Architecture work is persuasion across audiences: developers, managers,
  other architects. Bring **data and trade-off analysis**, not evangelism
  — the objective arbiter of trade-offs is the one who gets trusted
  (First Law in action).

## Intersections: architecture must align with…

- **Implementation** — do components match the intended structure?
- **Infrastructure** — deployment vs operational characteristics.
- **Data topologies** — monolithic vs domain-databases vs database-per-
  service must match the style.
- **Engineering practices** — TDD/deployment pipeline vs the style.
- **Team topologies** — org structure vs intended architecture.
- **Systems integration** — what you must talk to; ignoring this is how
  reliability dies.

## Cross-links

- ADRs record the trade-offs of the First Law: [architecture-thinking](../architecture-thinking/index.md).
- Fitness functions enforce ADR compliance: [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
- The styles these decisions choose between: [architecture-styles](../architecture-styles/index.md).
- Data topologies ↔ aggregates: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- Team boundaries ↔ bounded contexts: [domain-driven-design](../domain-driven-design/index.md).
