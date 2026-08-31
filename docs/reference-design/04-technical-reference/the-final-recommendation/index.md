# The final recommendation

## The recommendation

Use **one root Elmish program** with a **`Page` route** in the root model,
**`Shared.Model`** for persistent cross-page state, **`RemoteData<'T>`** for
async server values, **normalized entity maps** for canonical entities, and
**page-local `Model`/`Msg`** (lifted with `Cmd.map`) for the handful of pages
that genuinely need their own state. Keep the server boundary isolated behind
shared contract types and remoting.

At platform scale, the same design extends to the stateful platform:

```text
CLIENT
  .NET 10 Blazor WebAssembly
  Bolero 0.25
  Elmish
  Radzen 11.2.x
  PWA Service Worker / CacheStorage
  IndexedDB
  own IQueryCoordinator abstraction
    -> evaluate DotNetQuery implementation
  route-scoped foreground revalidation
  cursor/delta synchronization only for active scopes
  SignalR groups only for active scopes
  MessagePack realtime
  BYOA media providers
    -> YouTube first
    -> provider/resource identity in DB

WEB EDGE / BFF
  ASP.NET Core 10
  Bolero Remoting
  Identity/session
  contract version handshake
  progressive browser DTO aggregation
  HybridCache L1
  rate limiting
  SignalR
  Zstd/Brotli/gzip negotiated response compression

BUSINESS SERVICES
  coarse F# bounded-context microservices
  Functional Core / Imperative Shell
  ASP.NET Core host
  gRPC + Protobuf
  Dapr service invocation
  OpenTelemetry

DURABLE DATABASE
  PostgreSQL 18
  UUIDv7
  native PostgreSQL types
  FSharp.Data.SqlProvider + Npgsql
  Atlas versioned migrations
  CloudNativePG operator
  PgBouncer via Pooler CRD

CACHE
  HybridCache
  Dragonfly L2

ASYNC
  PostgreSQL transactional outbox
  Dapr Pub/Sub
  RabbitMQ
  idempotent inbox/consumers
  Dapr Workflow only for genuine durable multi-step processes

STORAGE
  ~1 TB HDD origin
  ~30 GB NVMe LVM dm-cache
  smq policy
  writethrough initially
  Local PV to CloudNativePG
  independent DB backup target

SECURITY
  ASP.NET Core Identity
  canonical Account + Personas
  external login credentials separate from linked profiles
  Cilium default deny
  application-specific Cilium policies
  Hubble visibility
  Audit Mode only during controlled policy discovery
  no automatic allow-from-observation

GITOPS
  Argo CD
  central cluster-infra repo for platform/cluster resources
  application-specific deployment/policy in application repo
  upstream third-party charts referenced/pinned rather than copied

LOCAL DEVELOPMENT
  .NET Aspire AppHost
  containers/services wired for development
  production remains Kubernetes/Argo
```

And enforce these product/architecture rules:

> **A browser cache is a convenience, not a continuously synchronized replica.
> Only the active route/data scope is revalidated or subscribed.**

> **PostgreSQL owns durable community truth; Dragonfly and browser caches are
> disposable.**

> **The BFF owns the browser boundary; bounded services own business
> capabilities.**

> **A network service boundary uses an explicit versioned contract. Sharing a
> programming language does not remove serialization.**

> **Slow or optional sources never block already available useful page
> content.**

> **Long work is accepted into a durable queue/workflow; ordinary reads stay
> direct and cancellable.**

> **User media bytes go directly from the user's browser to the user's
> authorized provider account; 42WASD stores provider-neutral pointers.**

> **Network reachability is deny-by-default and Git-reviewed; Hubble tells you
> what happened but never grants trust automatically.**

> **The 30 GB NVMe is an acceleration tier, not the database capacity limit.
> The 1 TB HDD remains durable capacity.**

## Why

- One routing source of truth keeps URL and UI consistent.
- Shared-vs-local separation keeps the root `update` small and testable.
- `RemoteData` and normalized caches remove ad-hoc flags and stale duplicates.
- Feature-oriented structure matches the way the domain actually changes.
- Route-scoped sync, disposable caches, and durable queues keep bandwidth and
  correctness separate concerns.

## The takeaway

The architecture is fixed: a single Elmish root, shared state for what spans
pages, page-local state for what does not, and remoting contracts for the
server — extended by PostgreSQL as the sole durable truth, disposable cache
layers, deny-by-default networking, and GitOps-owned desired state. Everything
else is discipline — and the acceptance test is that every feature survives
navigate-away-and-back in the correct state.