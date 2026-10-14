# Domain Modeling with Types — Making Illegal States Unrepresentable

> Source: *Domain Modeling Made Functional* (Scott Wlaschin) — ch 4
> (Understanding Types), ch 5 (Domain Modeling with Types), ch 6
> (Integrity and Consistency). Practiced throughout *Stylish F# 6* ch 2
> and 7; applied to UI state in *The Elmish Book* part 2.

## The bug that was already in the room

This is the actual `Tournament` type of this repo
(`src/Community.Web.Shared/Domain/Tournament.fs`):

```fsharp
type Tournament =
    {
        id: string
        name: string
        gameId: string
        startsAt: DateTime
        prize: string
        registrationOpen: bool
    }
```

Six fields, zero rules. Nothing stops `registrationOpen = true` on a
tournament that started three days ago. Nothing stops `gameId` and `id`
being mixed up — both are just `string`.

And the UI trusts the flag. The countdown badge on the Tournaments page
only checks it:

```fsharp
if tournament.registrationOpen then
    let span = tournament.startsAt - System.DateTime.Now
    // ...
    else "Starting now"
```

So a tournament that concluded last week renders **"Starting now"
forever** — no crash, just a lie.

**Illegal states don't throw — they mislead.** The **domain** here is the
world the software is about (tournaments, players, servers) and its rules.
Every consumer re-derives "what phase is this tournament in?" from raw
fields, and gets a fresh chance to be wrong. This page is the fix.

## 1. The move you already made once

The mistake: two independent `bool`s give four combinations — and two of
them are nonsense.

```fsharp
// ❌ isLoading + hasError: "loading AND failed" compiles; nothing handles it
{ data = None; isLoading = true; hasError = true }
```

The fix is already in this repo (`State/RemoteData.fs`) — the doc comment
there names the alternative it killed:

```fsharp
// ✅ four states, all legal, all forced-handled by the compiler
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Loaded of 'T
    | Failed of string
```

**Takeaway:** a type is a claim about what can exist. `bool` claims almost
nothing. The fix is a **choice type** — also called a *discriminated union*
or DU: a type whose value must be *exactly one of a fixed list of named
cases* (`NotAsked`, `Loading`, `Loaded`, `Failed`) — and the compiler holds
every consumer to that list.

## 2. Name the possibilities: the algebra of AND and OR

Read the domain sentence: *a tournament is upcoming, or registration is
open, or it is running, or it is concluded.* One big **OR** — and the
current record has nowhere to put it, which is why a `bool` had to smuggle
it in as a flag.

Two building blocks cover almost every domain sentence:

- A **record** (AND type) bundles several values that *all* exist together.
- A **choice type** (OR type) says the value is *exactly one* of a fixed
  list of cases — and each case can carry its own data.

DMMF's canonical demo is a payment: *a payment method is cash, or a check,
or a card.* Cash carries nothing; a check carries a number; a card carries
its details — a perfect fit for an OR with per-case data:

```fsharp
// From DMMF ch 4 — each case carries exactly what it needs, no more
type CheckNumber = CheckNumber of int
type CardType = Visa | Mastercard
type CreditCardInfo = { CardType: CardType; CardNumber: string }

type PaymentMethod =
    | Cash
    | Check of CheckNumber      // a check has a number
    | Card of CreditCardInfo    // a card has type + number
```

Requirement sentences translate almost mechanically: "an open tournament
has a name AND a start time AND a prize" is a record; "a tournament is
upcoming OR running OR concluded" is a choice type.

## 3. The refined type: make the phase a choice

Process of elimination on what `registrationOpen: bool` could be instead:

| Option | What it claims | Eliminated because |
| --- | --- | --- |
| `bool` (status quo) | two states | the domain has four phases; UI + stats mislead on the other two |
| `string` status | anything | unmatchable, no exhaustiveness, same hole as `GameServer.status` |
| `DateTime option` closedAt | two real states | timestamps are data, phases are rules; still re-derives phase per consumer |
| **phase DU** | the exact legal states | nothing else makes illegal phases a *compile error* |

So:

```fsharp
type TournamentPhase =
    | Upcoming
    | RegistrationOpen
    | Running
    | Concluded

type Tournament =
    {
        id: TournamentId
        name: string
        gameId: GameId
        startsAt: DateTime
        prize: string
        phase: TournamentPhase
    }
```

`registrationOpen` stops being a field and becomes a *question the type
answers*. The countdown badge, before and after:

```fsharp
// ❌ BEFORE: trusts a bool — a concluded tournament renders
// "Starting now" forever
let countdown t =
    if t.registrationOpen then countDown t.startsAt else none

// ✅ AFTER: phase DU — the match must answer for every phase
let countdown t =
    match t.phase with
    | Concluded | Running -> none        // already decided — no countdown
    | Upcoming | RegistrationOpen -> countDown t.startsAt
```

One line of cure for the opening bug: `Concluded` **cannot reach** the
"Starting now" branch. F# matches are **exhaustive** — the compiler errors
if any case is missing — so when `Cancelled` is added next month, the
compiler lists every countdown in the codebase that must decide. New
requirements arrive as **compiler errors at the sites that care**, not as
runtime surprises in a page you forgot existed.

## 4. Wrap the primitives before they lie

In the current repo every id is a `string`. Swap two arguments and the
compiler watches it happen. DMMF calls this **primitive obsession** —
letting raw built-in types (`string`, `int`, `float`) stand in for domain
concepts. The fix is a **single-case choice type**: a one-case DU that
wraps one primitive, giving the concept its own name and its own rules:

```fsharp
// ❌ both ids are strings — this compiles, and it's backwards
persist t.gameId t.id

// ✅ each concept its own type — the swap is now a compile error
type TournamentId = TournamentId of string
type GameId = GameId of string
persist t.gameId t.id        // ← rejected: the types don't match
```

To go further, make the constructor **private** — a *smart constructor*:
the only door in is a `create` function that validates first (DMMF ch 9's
canonical `OrderId`):

```fsharp
// From DMMF ch 9 — the canonical constrained-type pattern
module Domain =
    type OrderId = private OrderId of string

    module OrderId =
        /// Smart constructor: string -> OrderId (fails on bad input)
        let create str =
            if System.String.IsNullOrEmpty str
            then failwith "OrderId must not be null or empty"
            elif str.Length > 50
            then failwith "OrderId must not be more than 50 chars"
            else OrderId str

        /// Extract the inner value: OrderId -> string
        let value (OrderId str) = str
```

With `create` as the only door in, an invalid `OrderId` **cannot exist** —
validation happened once, at construction, and no consumer ever
re-checks. Same family: **units of measure** (`float<kg>`) give numbers a
unit so mixing kilograms with metres is a compile error.

## 5. Value objects vs entities

Is a `TournamentId` a *value* or an *entity*? Two flavors of domain data:

- A **value object** has no identity — it *is* its contents. Two copies of
  the same email address are interchangeable.
- An **entity** has an identity (an id) and changes over time — two
  customers with the same name are still different customers.

| | Value object | Entity |
| --- | --- | --- |
| Identity | contents (no id) | an explicit id |
| Equality | same data = equal | same id = equal |
| Lifetime | replace wholesale | changes over time |
| Example | `EmailAddress`, `Money` | `Customer`, `Order` |

`Game`, `News`, `Tournament` are entities; a `TournamentId` wrapper or an
email address is a value object. Watch the pattern in this repo:
`Player.favoriteGames` holds **ids of** games, never embedded copies.
That's also why the **aggregate** boundary matters — an **aggregate** is a
cluster of entities with one **root** that owns all consistency rules;
outsiders touch children only via the root:

- Aggregate = the boundary of what must be updated **in one transaction**
  (all-or-nothing save). Order + its lines yes; Order + Customer no
  (Customer is referenced by id).
- Aggregates map naturally to whole **documents** in a document database
  (see cross-links).

## 6. Integrity: invariants enforced at construction

An **invariant** is a rule that must *always* be true for the data —
"an order's total always equals the sum of its lines". Rules live **at
the boundary** (where data enters), not scattered in consumers. DMMF's
split:

- **Internal integrity** (invariants): enforce **at construction** — build
  types that are correct by construction; the rest of the code never
  re-checks.
- **External integrity**: a **DTO** (data transfer object — a plain,
  no-rules shape used to carry data across a boundary, e.g. over the
  network) arrives, a `create` validates it and either rejects (collect
  *all* errors, not just the first) or produces a type that cannot be
  invalid.

If a concept has states (Unpaid → Paid → Shipped), make **each state its
own type** and transitions functions between them:

```fsharp
type UnvalidatedOrder = { (* anything *) }
type ValidatedOrder  = { CheckedShipping: ShipmentMethod; (* ... *) }
type PricedOrder     = { Lines: PricedLine list; Total: Total }

// ❌ flags: one type + IsPaid + IsShipped = 6 illegal combinations
// ✅ states as types: ShippedOrder can only be created from PaidOrder
```

Every illegal combination becomes **unrepresentable** — a whole class of
bugs vanishes before it is written.

## 7. The same move, worked again: `GameServer.status`

The phase DU was not a one-off. `GameServer` has the stringly-typed twin of
`registrationOpen` — its own comment invites the fix. Three real places
trust the string today:

```fsharp
// ❌ (actual repo code)
// Domain/GameServer.fs
status: string // e.g. "online" | "maintenance" | "offline"

// Ui/RadzenUI.fs — a silent catch-all hides typos
| _ -> badgePill darkBadge "offline"
//  ↑ typo "onlne" doesn't error — it silently renders as "offline"

// Pages/Servers.fs — string comparison, typo compiles
let online = list |> Array.filter (fun s -> s.status = "online") |> Array.length
//  ↑ "Onine" would make this silently return 0
```

The ✅ is the textbook cure — DMMF ch 6's canonical example. The domain
rule: *a customer's email is either unverified or verified.* Model the two
as separate cases, and make the `Verified` case's inner type only
constructible by the verification service:

```fsharp
// From DMMF ch 6 — the canonical "make illegal states unrepresentable"
type CustomerEmail =
    | Unverified of EmailAddress
    | Verified of VerifiedEmailAddress   // a DIFFERENT type — private

// Only the verification service can create a VerifiedEmailAddress,
// so the Verified case can't be conjured by accident. Sending a
// password reset now requires the proof, in the type:
type SendPasswordResetEmail = VerifiedEmailAddress -> ...
```

The same cure applied to the server status — one type stating what can
exist, validated once at the boundary:

```fsharp
type ServerStatus = Online | Maintenance | Offline

module ServerStatus =
    let create = function
        | "online"      -> Ok Online
        | "maintenance" -> Ok Maintenance
        | "offline"     -> Ok Offline
        | other         -> Error $"unknown status: %s{other}"
//  unknown input is now a visible Error — not a silent "offline" badge
```

Add `Restarting` next month and the compiler lists every badge, filter and
pulsing dot that must decide.

**The move, generalized.** Whether the field was a `bool` or a `string`, the
disease was identical — *the type claimed less than the domain knows* — and
the cure is identical: name the legal states as a type, validate at the
boundary, let exhaustiveness do the rest.

## Revise

The flowchart below is the **review map** of this page, not the lesson.
Each node is a beat you just walked in narrative form — story → failure →
elimination table → phase DU → wrapped primitives → value objects vs
entities → invariants at construction. Return to it when you need to
check you've covered the whole move; don't try to learn from it.

```mermaid
flowchart LR
    n1(["Modeling with types"])
    n2["AND types: records"]
    n3["group related data"]
    n4["OR types: choice types"]
    n5["one of several alternatives"]
    n6["exhaustive matching"]
    n7["Simple values"]
    n8["single-case DUs wrap primitives"]
    n9["units of measure"]
    n10["constrained types validate on creation"]
    n11["Value objects vs entities"]
    n12["equal by contents"]
    n13["equal by identity"]
    n14["entities get ids"]
    n15["Aggregates"]
    n16["root entity owns consistency"]
    n17["document boundaries"]
    n18["Integrity &amp; invariants"]
    n19["internal vsadmitted at boundary"]
    n20["state machines encode legality"]
    n21["transitions = functions"]
    n22["flags forbidden"]
    n23["collected at construction"]
    n1 --> n2
    n1 --> n4
    n4 --> n5
    n4 --> n6
    n1 --> n7
    n7 --> n8
    n7 --> n9
    n7 --> n10
    n1 --> n11
    n11 --> n12
    n11 --> n13
    n13 --> n14
    n1 --> n15
    n15 --> n16
    n15 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n20 --> n21
    n20 --> n22
    n18 --> n23
```

## Cross-links

- `Option` and pattern-matching mechanics: [fsharp-style](../fsharp-style/index.md).
- The workflows these types feed: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- Serialization/persistence of aggregates: [dev-workflow](../dev-workflow/index.md) tooling and [fable-and-elm-architecture](../fable-and-elm-architecture/index.md) for the wire format.
- Ubiquitous language naming these types: [domain-driven-design](../domain-driven-design/index.md).
- Aggregates as bounded-context quanta: [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
