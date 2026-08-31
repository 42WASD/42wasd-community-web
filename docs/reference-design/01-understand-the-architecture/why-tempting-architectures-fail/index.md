# Why tempting architectures fail

Before writing pages, it is worth understanding why the obvious approaches do
not scale. Each problem below motivates a specific design decision in the
rest of this reference design — first the six client-side problems, then the
ten platform-side problems that motivate the stateful-platform architecture.

## Problem 1 — One giant root `Model` does not scale

It starts innocently, then grows without bound:

```fsharp
type Model =
    {
        Page : Page
        MobileMenuOpen : bool
        Events : Event list
        Projects : Project list
        // ...
    }
```

The issue is not that Elmish cannot handle a large record — it is
**ownership**. When every field sits at the root, developers stop knowing:

```text
who owns this field?
which update is allowed to change it?
should it survive navigation?
is this canonical data or temporary UI state?
```

The better separation:

```text
App.Model
├── Page
└── Shared

Shared.Model
└── persistent cross-page state
```

## Problem 2 — One giant root `Msg` becomes an event dump

A flat union is fine for a small application, but it is not the long-term
shape. Prefer a small root namespace:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

Complexity then grows where it belongs — inside the page or feature, not at the
root.

## Problem 3 — Mirroring the visual tree with MVU creates boilerplate

Do not assume every visual element needs `Model`/`Msg`/`update`. Most
landing-page UI does not own independent state machines. A reusable card should
be a pure function:

```fsharp
module EventCard =

    let view (event : Event) (onOpen : EventId -> unit) =
        article {
            h3 { event.Title }
            button {
                on.click (fun _ -> onOpen event.Id)
                "View event"
            }
        }
```

The page owns the behavior; the card renders it.

## Problem 4 — Splitting globally by technical type scatters features

Avoid a repo shaped like `Models/`, `Messages/`, `Updates/`, `Views/`,
`Services/`, `Components/`. Adding an Events feature then touches six folders.
Prefer feature/page colocation:

```text
Pages/
└── Events/
    ├── Events.fs
    ├── EventCard.fs
    └── EventFilters.fs
```

## Problem 5 — Shared entities should not be copied into page models

Avoid having the same entity exist in several versions. Prefer a canonical,
normalized cache in `Shared`, and page-local references by ID:

```text
Shared.Events : Map<EventId, Event>
```

## Problem 6 — Calling HTTP/remoting directly from views breaks the MVU boundary

Always route effects through the message loop:

```text
button click -> Msg -> update -> Cmd -> remote call -> result Msg -> update
```

## Summary

These six failure modes are the reason the design separates **routing**,
**persistent state**, **page-local state**, **domain data**, **effects**, and
**rendering** — rather than collapsing them into one monolith or, conversely,
splitting them into excessive MVU components.

## Platform problems 1–10

At platform scale, ten more tempting designs fail. Each one is answered
elsewhere in this reference design; the mapping is noted so no decision is
left unanchored.

### Problem 1 — Bolero is not a database or integration framework

Bolero solves the F#/Blazor browser/application problem well:

```text
F# UI
Elmish MVU
routing
Blazor integration
typed client/server remoting
```

It should not also be expected to be:

```text
PostgreSQL ORM
Redis client
RabbitMQ broker
Kubernetes service mesh
identity provider
background scheduler
object store
```

The browser-to-server bridge is Bolero's job. What the server does after
receiving the call belongs to ASP.NET Core and the broader .NET/
distributed-systems stack. → *The ten-layer mental model; the selected tool
stack.*

### Problem 2 — "Stateful web application" does not mean state should live in the web Pod

The web process may temporarily hold:

```text
active SignalR connections
small L1 caches
request-local objects
in-flight query coordination
```

but authoritative business facts must not depend on that Pod surviving.

Bad:

```text
Community.Web.Server
   let mutable posts = ...
   let mutable users = ...
   let mutable forum = ...
```

Better:

```text
replaceable BFF Pod
       ↓
PostgreSQL = durable truth
Dragonfly  = disposable acceleration
RabbitMQ   = durable unfinished work
```

A Pod can be rescheduled or replaced without losing forum posts. → *The
source-of-truth model.*

### Problem 3 — Browser caching does not imply continuous synchronization

The browser should **remember previously useful data**, but it should not
remain a replica that continuously tracks all changes.

Bad:

```text
10,000 browser tabs open
        ↓
10,000 clients constantly syncing forum
        ↓
every forum event wakes every client
```

Selected rule:

```text
not on forum route
    -> no forum sync
    -> no forum topic/list subscription

on /forum/general
    -> synchronize only that list/projection
    -> subscribe only to that active list scope

on /forum/topic/123
    -> synchronize only topic 123
    -> subscribe only to topic 123

leave route
    -> unsubscribe
    -> stop keeping that scope fresh
    -> retain cached data as stale local cache
```

This is **route-scoped / demand-driven synchronization**. → *Route-scoped
synchronization contract.*

### Problem 4 — Realtime transport does not provide durable correctness

SignalR is a low-latency notification path. A browser can sleep, lose network,
close the laptop, suspend the tab, miss events, and reconnect to another Pod.
Therefore:

```text
SignalR
    = "something changed quickly"

cursor/change feed
    = "what did I miss?"

PostgreSQL
    = "what is true?"
```

→ *Realtime contract.*

### Problem 5 — A microservice boundary cannot eliminate wire representation

If Service A and Service B are separate processes:

```text
A memory
  ↓
bytes
  ↓
socket/network
  ↓
bytes
  ↓
B memory
```

Sharing the same F# source type does not allow one process to dereference
another process's object.

Use **gRPC + Protobuf** as the default internal synchronous contract because
it is compact, strongly schema-driven, supports streaming/deadlines/
cancellation, and allows future services in Go/Rust/Python/etc. If profiling
later finds an extraordinary hot path where materialization overhead is
dominant, benchmark FlatBuffers/Cap'n Proto or colocated shared-memory
designs. Do not make that the default architecture. → *Internal microservice
contracts.*

### Problem 6 — Dependency injection does not make incompatible technologies interchangeable

An application can depend on ports:

```text
IPostStore
ICache
INotificationPublisher
IClock
```

and ASP.NET Core DI can supply adapters:

```text
PostgresPostStore
DragonflyCache
DaprNotificationPublisher
SystemClock
```

But DI does not mean:

```text
PostgreSQL can be swapped for a graph database
with zero semantic changes
```

That is only possible where the **port/interface itself is technology-neutral
enough**.

```text
Good abstraction:            Leaky abstraction:
GetPost(PostId)              ExecutePostgresJsonbContainsQuery(...)
SavePost(Post)
```

The first can plausibly have multiple adapters. The second is
PostgreSQL-specific by design. → *Abstraction philosophy.*

### Problem 7 — A durable command and a cancellable query have different lifetimes

Query: "show search page 250" — if every interested browser subscriber
disappears, cancel expensive work.

Durable command: import account history, generate reports, send thousands of
notifications, run a multi-stage tournament operation — once accepted, it
should survive the browser leaving.

```text
QUERY / short command      -> direct request, cancellation token,
                              no durable queue unless required
ASYNC COMMAND              -> persist/queue job, return JobId,
                              worker continues independently
MULTI-STEP DURABLE PROCESS -> Dapr Workflow
```

Do not put every operation through a queue. → *Async work architecture.*

### Problem 8 — First-party media storage conflicts with the selected product constraint

The selected product convention is:

```text
42WASD stores:
provider
resource ID
kind
status
metadata needed by the post

provider stores:
video/image/file bytes
```

The browser talks directly to the provider when the provider's API permits it.
42WASD does not proxy hundreds of megabytes through the home server merely to
attach a forum video. → *BYOA media model.*

### Problem 9 — "All database values as text" wastes type information and often space

Use native PostgreSQL types:

```text
uuid
bigint/int
boolean
timestamptz
text
jsonb only where genuinely flexible
foreign keys
constraints
indexes
```

Hex is not a compact representation — one binary byte becomes two hexadecimal
characters, and Base64 also expands binary data. Wire compactness should be
solved by projection, binary serialization where needed, and compression —
not by destroying database types. → *PostgreSQL schema design rules.*

### Problem 10 — NVMe hot storage and HDD capacity are a storage-layer problem

PostgreSQL tablespaces can explicitly place known objects on different disks.
They do not automatically behave like:

```text
frequently accessed arbitrary disk block -> NVMe
cold block -> HDD
```

For the specified ~30 GB NVMe + ~1 TB HDD requirement, use a block-level
hot-spot cache such as **LVM dm-cache** below the filesystem/PV. PostgreSQL
sees one logical volume. → *Database physical storage.*