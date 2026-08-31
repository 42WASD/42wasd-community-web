# The one-sentence idea

Build one **Bolero `ProgramComponent` / Elmish program** whose root model stays
small, keep cross-page data in a persistent `Shared.Model`, keep temporary
state close to the page or feature that owns it, use Bolero routing and
`PageModel<'T>` for route/page state, use `Cmd` for effects, and keep ordinary
visual elements as pure view functions.

## The platform one-sentence idea

At platform scale, the same idea extends into one complete sentence covering
every layer:

> Keep **Bolero + Elmish + Radzen** as the F# browser stack, turn
> `Community.Web.Server` into a thin **ASP.NET Core BFF**, keep browser data
> **locally cached but only synchronized on demand for the active route**, put
> durable community truth in **PostgreSQL 18**, use **HybridCache + Dragonfly**
> for disposable hot data, use **Dapr + gRPC/Protobuf + RabbitMQ** for
> independently deployable backend services and durable work, use **SignalR +
> MessagePack** only for active realtime scopes, use **Cilium + Hubble** to
> enforce and observe service reachability, use **Argo CD** as the production
> deployment controller, and place the database on an **HDD-backed LVM
> dm-cache logical volume accelerated by the small NVMe tier**.

## The platform mental model

```text
                                  USER DEVICE
┌─────────────────────────────────────────────────────────────────────────────┐
│ Browser / PWA                                                               │
│                                                                             │
│  Bolero + Elmish + Radzen                                                   │
│      │                                                                      │
│      ├── Elmish Model ........ current visible/UI state                     │
│      ├── IndexedDB ........... local structured cache/outbox/drafts         │
│      ├── Service Worker ...... immutable app/runtime asset snapshot         │
│      ├── Query Coordinator ... in-flight dedupe/subscribers/SWR             │
│      ├── SignalR ............. only active route/topic scopes               │
│      └── BYOA Media .......... YouTube / future provider adapters           │
│                                                                             │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                    HTTPS / Bolero Remoting
                    sufficiently-large bodies: zstd/br/gzip
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ COMMUNITY WEB / BFF — ASP.NET CORE                                          │
│                                                                             │
│  static/PWA asset hosting                                                   │
│  Bolero Remoting                                                            │
│  authentication/session                                                     │
│  browser DTO/version negotiation                                            │
│  route/page aggregation                                                     │
│  SignalR hubs/groups                                                        │
│  rate limiting                                                              │
│  HybridCache L1                                                             │
│                                                                             │
│                    THIS IS NOT THE WHOLE BUSINESS CORE                       │
└───────────────────────┬─────────────────────────────────────────────────────┘
                        │
              Dapr service invocation
              native gRPC + Protobuf
                        │
      ┌─────────────────┼────────────────────┬─────────────────────┐
      │                 │                    │                     │
      ▼                 ▼                    ▼                     ▼
┌─────────────┐  ┌──────────────┐   ┌──────────────┐      ┌──────────────┐
│ Accounts    │  │ Forum        │   │ Game/Server  │      │ Tournament   │
│ Service     │  │ Service      │   │ Service      │      │ Service      │
└──────┬──────┘  └───────┬──────┘   └──────┬───────┘      └──────┬───────┘
       │                 │                 │                      │
       └────────────┬────┴────────────┬────┴──────────────────────┘
                    │                 │
                    ▼                 ▼
              PostgreSQL       external/internal APIs
                    │                 │
                    │             HTTP/gRPC/Dapr
                    │
            ┌───────┴────────┐
            │                │
            ▼                ▼
      HybridCache L2       RabbitMQ
        Dragonfly        Dapr Pub/Sub
                            │
                            ▼
                         Workers /
                      Dapr Workflows

SERVER-SIDE SECURITY / CONTROL
──────────────────────────────────────────────────────────────────────────────
Cilium default deny -> CiliumNetworkPolicy -> Hubble flow visibility
Argo CD -> reconciles production manifests from Git
OpenTelemetry -> traces/metrics/log correlation

DATABASE STORAGE PATH
──────────────────────────────────────────────────────────────────────────────
application cache RAM
        ↓ miss
PostgreSQL shared buffers + Linux page cache
        ↓ miss
~30 GB NVMe dm-cache hot blocks
        ↓ miss
~1 TB HDD origin volume
```

## The crucial rules

```text
                         ┌───────────────────────────┐
                         │        BROWSER / URL      │
                         └─────────────┬─────────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │ Bolero Router   │
                              │ Page DU         │
                              └────────┬────────┘
                                       │
                                       ▼
                         ┌───────────────────────────┐
                         │    ONE ELMISH PROGRAM     │
                         │ ProgramComponent<M, Msg>  │
                         └─────────────┬─────────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
                App.Model          Shared.Model        Page state
                orchestration      persistent          temporary /
                                    cross-page          route-owned
                    │                  │                  │
                    └──────────────────┼──────────────────┘
                                       │
                                       ▼
                                    update
                                       │
                           ┌───────────┴───────────┐
                           │                       │
                           ▼                       ▼
                         Model                    Cmd
                                                   │
                                                   ▼
                                        remoting / browser /
                                        API / other effects
                                                   │
                                                   ▼
                                                  Msg
                                       │
                                       ▼
                                     view
                                       │
                         ┌─────────────┼─────────────┐
                         │             │             │
                         ▼             ▼             ▼
                       page          feature       shared UI
                       views         views         functions
```

## The crucial rules

### Client rules

```text
route state != shared domain state
page-local state != persistent application state
visual component != independent MVU program
effect != direct mutation
framework shell != application architecture
```

### Platform rules

```text
Elmish state               != IndexedDB cache
IndexedDB cache            != PostgreSQL truth
SignalR event              != synchronization guarantee
cache                      != source of truth
Bolero Remoting            != business layer
ASP.NET Core DI            != database abstraction by itself
gRPC                       != compression
Protobuf                   != zero-copy shared memory
microservice               != one function per service
Dapr                       != your business logic
Cilium Service reachability!= application authorization
BYOA media reference       != first-party blob ownership
NVMe cache                 != backup
```

Everything else in this design is an unpacking of these ideas.