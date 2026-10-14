# Domain-Driven Design — Understanding the Problem First

> Source: *Domain Modeling Made Functional* (Scott Wlaschin) — ch 1
> (Introducing DDD), ch 2 (Understanding the Domain), ch 3 (A Functional
> Architecture). Echoed by *FSA* ch 7 (bounded contexts as quanta) and
> ch 18 (microservices).

Code is the easy part; understanding the problem is the hard part. DDD =
build a **shared model** between domain experts and developers so the code
reflects the expert's mental model directly — no translation loss.

## "We call it a Placed Order"

The canonical story is DMMF's event-storming session at the fictional
Widgets Inc — watch a **ubiquitous language** get born in four lines
(ch 1, abridged):

> **You:** "What would you call that as an event?"
> **Sam (shipping):** "How about 'Order Available'?"
> **Ollie (orders):** "We call an order that's completed and ready to
> ship a 'Placed Order'. Can we agree on using that term everywhere?"
> **Sam:** "So 'Order Placed' would be the event we care about, yes?"

Sam said "Order Available", Ollie said "Placed Order" — same fact, two
dialects. The moment they agree on **Order Placed**, the code has its name:
the event type, the workflow, the message. That agreement *is* the
**ubiquitous language** — the set of concepts and vocabulary shared by
everyone on the team, used in speech and in code with no translation
step.

## 1. The shared model — and why "Telephone" fails

A mismatch between the developer's understanding and the domain expert's
is fatal — like the children's game "Telephone". DMMF's three approaches:

| Approach | Mechanism | Problem |
| --- | --- | --- |
| ✅ DDD | code **is** the shared model | needs discipline |
| Agile iteration | feedback loop corrects drift | developer still a translator |
| ❌ Telephone | expert → translator → code | distortion at every hop |

Guidelines: focus on **business events** and workflows (not data
structures), partition into subdomains, model each subdomain, develop a
ubiquitous language used everywhere in the code.

## 2. Discover the domain: events and commands

An **event storming** workshop puts everyone with questions and everyone
with answers in one room, a wall, and sticky notes. Two vocabularies carry
it:

- **Domain events** — things that happened, always past tense, because a
  fact can't be changed: `Order form received`, `Order placed`,
  `Order shipped`.
- **Commands** — requests to do something, always imperative:
  `Place an order`. A command may fail; if it succeeds, a workflow runs
  and emits new events.

The pattern: `event → command → workflow → events`. And when the wall
shows a gap — "do you tell the customer the order was placed?" — the
missing event (`Order acknowledgment sent`) *is* the missing requirement.
Follow the event chain to the edges in both directions; gaps surface
where you least expect them.

## 3. Partitioning: subdomains and bounded contexts

- **Core domain** = what makes money / competitive advantage. Supportive
  and generic subdomains can be bought or simplified.
- A large domain splits into multiple **bounded contexts** — each with its
  own model, its own dialect, its own data.
- Contexts relate via a **context map**: Shared Kernel, Customer/Supplier,
  Conformist, or the all-important **Anti-Corruption Layer** (translate
  foreign models at the boundary; never let them leak in).

## 4. The ubiquitous language, in code

One dialect **per bounded context**. "Ship" means something different to
Sales and to the Warehouse — and that's fine.

```fsharp
// ❌ words that carry no domain meaning
type ProductManager = ...
let helper data = ...

// ✅ the expert's words, verbatim — no translation step
type OrderPlaced = ...
type OrderAcknowledgementSent = ...
```

## 5. Functional architecture: the onion

- Each bounded context = an autonomous component: **workflows as
  pipelines** (functions in, functions out).
- **DTOs at trust boundaries** — serialize at the edge, pass domain types
  inside.
- **Onion / Hexagonal / Clean**: domain at the center, infrastructure
  around it, **dependencies point inward**.
- **I/O at the edges only** — no DB, randomness, or mutation inside a
  workflow. Pure workflows are testable workflows ("IO sandwich").

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Domain-Driven Design"])
    n2["Shared model"]
    n3["no translation loss"]
    n4["code reflects expert mental model"]
    n5["Discover via events"]
    n6["event storming workshop"]
    n7["domain events: past tense"]
    n8["commands: imperative"]
    n9["event → command → workflow → events"]
    n10["Partition the domain"]
    n11["core vs supportive vs generic"]
    n12["Bounded contexts"]
    n13["one model per context"]
    n14["context maps: upstream/downstream"]
    n15["relationships"]
    n16["Shared Kernel"]
    n17["Customer/Supplier"]
    n18["Conformist"]
    n19["Anti-Corruption Layer"]
    n20["Ubiquitous language"]
    n21["one dialect per context"]
    n22["no Manager/Helper types"]
    n23["Functional architecture"]
    n24["workflows as pipelines"]
    n25["DTOs at trust boundaries"]
    n26["Onion: dependencies point inward"]
    n27["I/O at the edges only"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n1 --> n5
    n5 --> n6
    n5 --> n7
    n5 --> n8
    n5 --> n9
    n1 --> n10
    n10 --> n11
    n1 --> n12
    n12 --> n13
    n12 --> n14
    n12 --> n15
    n15 --> n16
    n15 --> n17
    n15 --> n18
    n15 --> n19
    n1 --> n20
    n20 --> n21
    n20 --> n22
    n1 --> n23
    n23 --> n24
    n23 --> n25
    n23 --> n26
    n23 --> n27
```

## Cross-links

- Modeling those events as types: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- Workflows as pipelines in code: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Bounded contexts as architecture quanta: [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
- Domain partitioning and microservices: [architecture-styles](../architecture-styles/index.md).
- The same "pages as programs" decomposition on the client: [scaling-and-routing](../scaling-and-routing/index.md).
