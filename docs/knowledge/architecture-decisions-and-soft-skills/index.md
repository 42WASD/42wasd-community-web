# Architecture Decisions and Soft Skills — The Human Layer

> Source: *Fundamentals of Software Architecture* (Richards & Ford) —
> ch 20–26: Architectural Patterns, Decisions (ADRs), Analyzing Risk,
> Diagramming, Making Teams Effective, Negotiation, Intersections.

Technical brilliance fails without communication. Decisions need records,
risk needs a shared language, and teams need the right kind of architect —
not too tight, not too loose.

## The decision that evaporates

The Traefik/Argo CD health customization (from
[architecture-thinking](../architecture-thinking/index.md)) *works* — but
if its *why* lived only in one head, the first maintainer who didn't know
the story would see a "weird" health override and rip it out. The app
would silently regress to `Progressing` forever. **A decision without a
record is a decision waiting to be undone.** That is the entire point of
the ADR — this page's core tool.

## 1. Making decisions: ADRs

- Architectural decisions shape **structure** or technology choices that
  impact characteristics; good ones **guide** teams to the right
  technical choices rather than dictating every detail.
- **Architecture Decision Record** (Nygard, 2011): a one- to two-page
  markdown record per decision. The basic structure — **Title, Status,
  Context, Decision, Consequences** — plus the authors' additions
  **Compliance** (how it's enforced — possibly via fitness functions) and
  **Notes**. ADRs fix the Second Law's problem: the *why* survives team
  turnover.
- The book's worked example, ADR 76 (abridged to the teaching skeleton):

```text
ADR 76. Separate Queues for Bid Streamer and Bidder Tracker Services
STATUS:      Accepted
CONTEXT:     Bid Capture must forward each bid to Bid Streamer and
             Bidder Tracker — via one pub/sub topic, separate queues,
             or REST. Streamer needs first-bid-only in FIFO order;
             Tracker needs all bids.
DECISION:    We will use separate queues for the two services.
CONSEQUENCES: Requires clustering/HA of the message queues; Capture
             sends the same info twice; bid events bypass API-layer
             security checks (ARB reviewed and accepted this).
COMPLIANCE:  Periodic manual code reviews ensure async messaging
             stays in place.
```

Note the voice the book insists on: **"We will use..."** — affirmative
and commanding, not "I think...". Status can be *Proposed*, *Accepted*,
or *Superseded* (superseded ADRs stay, chained to their successor —
decisions have history, not just a present).

## 2. Analyzing risk

- Risk = operational (availability, data integrity) **and** structural
  (coupling, decay). Use a **risk matrix** (likelihood × impact, each
  rated low 1 / medium 2 / high 3, multiplied) with characteristics as
  assessment criteria.
- **Risk storming**: (1) individuals assess alone — no anchoring;
  (2) consensus on the hot spots; (3) mitigation plans. Repeat when the
  architecture changes.

## 3. Diagramming

- **C4** (Simon Brown): zoom levels **Context → Container → Component →
  Class**. Use layers **semantically** (each layer = real meaning), not
  decoratively (boxes grouped for looks). Don't let one tool become an
  irrational artifact attachment; pick the diagram the audience needs.

## 4. Teams: three architect personalities

| Personality | Boundaries | Result |
| --- | --- | --- |
| **Control freak** | too tight — writes the pseudocode, bans libraries | steals the craft; devs disengage |
| **Armchair** | too loose — absent, diagrams too high-level | team does the architect's work, confusion |
| **Effective** | right-sized constraints | guidance + availability + roadblocks removed |

## 5. Negotiation

- Architecture work is persuasion across audiences: developers, managers,
  other architects. Bring **data and trade-off analysis**, not evangelism
  — the objective arbiter of trade-offs is the one who gets trusted
  (First Law in action). The book's 4 Cs of architecture leadership:
  **communication, collaboration, clear, concise**.

## 6. Intersections: architecture must align with…

- **Implementation** — do components match the intended structure?
- **Infrastructure** — deployment vs operational characteristics.
- **Data topologies** — monolithic vs domain-databases vs database-per-
  service must match the style.
- **Engineering practices** — TDD/deployment pipeline vs the style.
- **Team topologies** — org structure vs intended architecture.
- **Systems integration** — what you must talk to; ignoring this is how
  reliability dies.

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

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

## Cross-links

- ADRs record the trade-offs of the First Law: [architecture-thinking](../architecture-thinking/index.md).
- Fitness functions enforce ADR compliance: [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
- The styles these decisions choose between: [architecture-styles](../architecture-styles/index.md).
- Data topologies ↔ aggregates: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- Team boundaries ↔ bounded contexts: [domain-driven-design](../domain-driven-design/index.md).
