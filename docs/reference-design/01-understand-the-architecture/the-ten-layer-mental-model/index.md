# The ten-layer mental model

The complete mental model decomposes the platform into **ten separate layers**.
Each layer answers one focused question. This extends the original six-layer
client model (App, Routing, Shared, Page, Effects, View) with the
server/platform layers required by a stateful community platform.

## 2.1 PRESENTATION — "What is currently visible?"

Recommended:

```text
Bolero 0.25.x
Elmish
Radzen Blazor 11.2.x
.NET 10 / Blazor WebAssembly
```

Responsibilities:

```text
route
visible components
form state
loading/error/success states
optimistic visual changes
user interactions
rendering
```

Elmish is the immediate state machine, not durable persistence.

The client-side sub-layers of this layer remain the six-layer model:

```text
APP / ROUTING / SHARED / PAGE / EFFECTS / VIEW
```

## 2.2 LOCAL DATA — "What can the device remember?"

Recommended:

```text
IndexedDB
+
Service Worker CacheStorage
```

Separate responsibilities:

```text
IndexedDB
    cache_*
    drafts
    outbox
    upload_sessions
    metadata/sync cursors

CacheStorage
    immutable/static application snapshot
    WASM/runtime assets
    application assemblies/assets
    CSS/JS
```

Do not manually put WASM/runtime assets into IndexedDB.

## 2.3 CLIENT DATA COORDINATION — "Who owns an in-flight request?"

Recommended architecture:

```text
IQueryCoordinator
    key-based deduplication
    subscriber tracking
    stale/fresh state
    cancellation policy
    optional prefetch
```

Candidate implementation to prototype:

```text
DotNetQuery.Core / DotNetQuery.Blazor 1.3.x
```

But hide it behind your own F# abstraction because it is young relative to the
rest of the stack.

## 2.4 CLIENT SYNCHRONIZATION — "What active scope is stale?"

Selected model:

```text
foreground-triggered stale-while-revalidate
+
cursor/delta synchronization
+
entity versions
+
tombstones
+
optimistic concurrency
```

Only active route scopes synchronize.

IndexedDB does not attempt to remain globally fresh.

## 2.5 REALTIME — "What should the active screen hear immediately?"

Recommended:

```text
SignalR
+
MessagePack
+
route/topic groups
```

Possible scopes:

```text
forum:list:{category}:{filterHash}
forum:topic:{topicId}
dm:{conversationId}
user:{userId}:critical-session-events   # only if truly required
```

The default rule is **no domain subscription for invisible content**.

Open the SignalR connection lazily when the first live-capable scope becomes
active. Optionally close it after a short grace period when there are no
subscriptions.

## 2.6 BROWSER-FACING SERVER — "What does the browser trust?"

Recommended:

```text
Community.Web.Server
=
ASP.NET Core BFF
```

Responsibilities:

```text
serve Blazor/PWA assets
Bolero Remoting
browser authentication/session
browser contract version negotiation
route/page-specific aggregation
SignalR hubs
rate limiting
response compression
HybridCache L1 usage
call backend services
```

It should not own every business rule in the system.

## 2.7 DOMAIN SERVICES — "Who owns each business capability?"

Start with coarse boundaries:

```text
Accounts Service
Forum Service
Game/Server Service
Tournament Service
Notification Service
```

Add only when justified:

```text
Search Service
Moderation Service
Analytics/Activity Service
```

Avoid:

```text
CreatePostService
GetPostService
CountCommentService
```

That is network-distributed function decomposition, not good microservice
design.

## 2.8 DISTRIBUTED PLUMBING — "How do services talk and survive failure?"

Recommended:

```text
Dapr 1.18.2
gRPC + Protobuf
RabbitMQ 4.2
Dapr Pub/Sub
Dapr Workflow for complex durable processes
OpenTelemetry
```

Dapr owns reusable distributed plumbing.

Your services own domain behavior.

## 2.9 DATA — "Where is durable truth and reusable hot data?"

Recommended:

```text
PostgreSQL 18.x
CloudNativePG 1.30.x
FSharp.Data.SqlProvider / SQLProvider.PostgreSql
Npgsql
Atlas versioned migrations

HybridCache
    L1: process MemoryCache
    L2: Dragonfly 1.40.x
```

PostgreSQL is authoritative.

Dragonfly is disposable.

## 2.10 PLATFORM — "Who controls deployment, networking, and physical storage?"

Recommended:

```text
Kubernetes
Argo CD 3.5.x
Cilium 1.20.x
Hubble
LVM dm-cache
local PersistentVolume / chosen local CSI layer
OpenTelemetry Collector
Prometheus/Grafana-compatible monitoring
```

## The rule

Each layer answers exactly one question. No layer is allowed to absorb another
layer's responsibility merely for convenience — that is the root cause of
fragile platform design.

---

## Appendix — the original six-layer client model

For the browser-only view (which the earlier edition of this page described),
layers 2.1–2.6 above map onto the six client layers. They remain the correct
decomposition *inside* the Bolero client:

## A.1 APP — “What composes the entire application?”

Responsibilities:

```text
one ProgramComponent
root Model
root Msg
root update
router attachment
top-level view
dependency wiring
```

Target shape:

```fsharp
type Model =
    {
        Page : Page
        Shared : Shared.Model
    }

type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

The root should stay boring. That is a feature.

## A.2 ROUTING — “Which routable state is currently active?”

Use a Bolero endpoint DU:

```fsharp
type Page =
    | [<EndPoint "/">]
      Home

    | [<EndPoint "/about">]
      About

    | [<EndPoint "/events">]
      Events of PageModel<Events.Model>
```

Static pages need no local model. Stateful pages can carry `PageModel<'T>`
(for search/filters/pagination) — use route parameters for state that should be
encoded in the URL.

## A.3 SHARED STATE — “What must persist or be reused across pages?”

```text
authenticated user
community metadata
event/project/member entity caches
feature flags
persistent preferences
```

Example:

```fsharp
module Shared

type Model =
    {
        CurrentUser : User option
        Community : RemoteData<CommunityInfo>
        Events : RemoteData<Map<EventId, Event>>
    }
```

This is the cross-page source of truth.

## A.4 PAGE / FEATURE STATE — “What only makes sense while this feature is active?”

```fsharp
module Events

type Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
        SelectedEventId : EventId option
    }
```

This state belongs to the Events experience and should not pollute
`App.Model`.

## A.5 EFFECTS — “What touches the impure world?”

```text
Bolero Remoting, HTTP APIs, browser storage, clipboard, timers, analytics,
GitHub API, Discord/community API
```

Trigger them from Elmish commands:

```text
pure update -> new Model | Cmd -> impure work -> Msg
```

The module that understands the returned result owns the result message.

## A.6 VIEW — “What renders the current model?”

Views consume data and dispatch messages. They should not become a second
state architecture.

```text
App.view
├── Layout.view
└── Page.view
     ├── Home.view
     ├── Events.view
     │    ├── EventCard.view
     │    └── EventFilters.view
     └── Projects.view
```

Keep Hero, Navbar, Footer, Card, Badge, Button, Stats section, Sponsor grid as
ordinary functions/modules unless they truly own independent behavior.