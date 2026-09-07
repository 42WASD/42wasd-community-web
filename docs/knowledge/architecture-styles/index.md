# Architecture Styles — The Catalog of Shapes

> Source: *Fundamentals of Software Architecture* (Richards & Ford) —
> ch 9 (Foundations) through ch 19 (Choosing the Appropriate Style).

A style defines **topology** (component shapes, communication, data) and
fall within two families: **monolith** (one deployment unit) and
**distributed** (many units, network calls). Every style is a trade-off
bundle — none is "best".

```mermaid
flowchart LR
    n1(["Architecture styles"])
    n2["Style vs pattern"]
    n3["style: named topology"]
    n4["pattern: lower-level solution"]
    n5["Partitioning"]
    n6["technical: layers by capability"]
    n7["domain: by workflow (DDD)"]
    n8["Conway's law binds both"]
    n9["Monolithic family"]
    n10["layered: familiar, cheap"]
    n11["modular monolith: DDD inside"]
    n12["pipeline: pipes &amp; filters"]
    n13["microkernel: core + plug-ins"]
    n14["Distributed family"]
    n15["service-based: pragmatic middle"]
    n16["event-driven: async, reactive"]
    n17["space-based: scale by replication"]
    n18["microservices: domain-owned"]
    n19["soa: the elder era"]
    n20["Choosing"]
    n21["it depends — on characteristics"]
    n22["katas: practice the analysis"]
    n23["monolith vs distributed"]
    n24["distributed: fallacies apply"]
    n25["picking the least worst"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n1 --> n5
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
    n14 --> n18
    n14 --> n19
    n1 --> n20
    n20 --> n21
    n20 --> n22
    n20 --> n23
    n20 --> n24
    n20 --> n25
```

## Foundations: styles, partitioning, Conway

- **Style** = topology + deployment + communication + data shapes.
  **Pattern** = a smaller, reusable solution inside a style.
- **Technical partitioning** (presentation/business/persistence layers):
  familiar, cheap to staff — but every workflow cuts across all layers.
  **Domain partitioning** (components by workflow, from DDD): workflows
  change in one place.
- **Conway's Law**: designs copy the organization's communication
  structure. Want domain-partitioned software? Arrange teams that way
  (**Inverse Conway Maneuver** / team topologies).

## The monolithic family

| Style | Idea | Best when |
| --- | --- | --- |
| **Layered** | technical layers, top-down calls | simplicity/familiarity dominate; beware "accidental architecture" |
| **Modular monolith** | domain-partitioned, single deploy | DDD fits, no distributed costs; the modern default candidate |
| **Pipeline** | filters connected by pipes | streams, ETL, batch — each filter transforms independently (producers/consumers over topics, à la Kafka); including DMMF's workflow pipelines |
| **Microkernel** | core system + plug-ins | product apps with per-customer customization |

## The distributed family

- **Service-based**: a few coarse services (UI + domain + DB each),
  pragmatic and cost-effective — microservices' calmer sibling.
- **Event-driven**: decoupled processors reacting to events. Choreography
  (broadcast) vs **mediator topology** (an orchestrator controls the
  workflow); highly scalable, but eventual consistency and workflow
  debugging are the price.
- **Space-based**: replicate processing units + in-memory data grids to
  shed the database bottleneck under bursty load.
- **Microservices**: one service per bounded context, **database per
  service**, independent deploys. Buys scalability/agility/fault
  tolerance, pays in operational complexity and data consistency.
- **Orchestration-driven SOA**: the ESB-centric predecessor — a lesson in
  how eras age.

## Choosing: it depends, on what?

- Never binary — score styles against the prioritized characteristics
  from [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md)
  and the domain's needs; pick the **least worst** fit.
- Practice with **architecture katas** (Silicon Sandwiches is the book's
  running case).
- Monolith vs distributed is the first fork: distributed styles share
  known challenges — network reliability, latency, bandwidth, security,
  consistency. Don't pay that tax without a reason.

## Cross-links

- The laws that forbid "best" answers: [architecture-thinking](../architecture-thinking/index.md).
- DDD → domain partitioning → modular monolith / microservices:
  [domain-driven-design](../domain-driven-design/index.md).
- Pipelines as code: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Quanta and fitness functions that police the style: [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
- Recording the choice as an ADR: [architecture-decisions-and-soft-skills](../architecture-decisions-and-soft-skills/index.md).
