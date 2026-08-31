# Primary references

The evidence base for this design.

## Bolero / Elmish

- Bolero main repository
- Bolero routing documentation
- Bolero `ProgramComponent` / `ElmishComponent` documentation
- Elmish documentation

## Functional architectures

- Elm Application Architecture (program structure)
- Elm parent-child composition guidance

## Framework-agnostic feature organization

- Angular feature-area guidance
- Redux feature-folder guidance

## Verification tooling

- mkdocs `--strict` builds
- deterministic manifest tests
- the golden test asserting generator idempotence

## The verified architecture document

This reference design **is** the single verified architecture for the
community-web project. It was originally distilled from a standalone
architecture document (verified 2026-08-27); that document has since been
fully reconciled into these pages and retired — this reference design is now
the authoritative source.

## Platform evidence base (verified 2026-08-27)

The platform layers of this design were verified against current primary
documentation; re-audit before major upgrades because .NET, Kubernetes, Dapr,
Cilium, Argo CD, database operators, and providers continue to evolve:

```text
Bolero 0.25 release (fsbolero.io) and docs
Radzen.Blazor NuGet (pin tested patch after compatibility testing)
Microsoft: Blazor PWA / response compression / HybridCache
Microsoft: SignalR Redis backplane / MessagePack / streaming
PostgreSQL 18 release notes, UUID functions, data types, TOAST,
  tablespaces, partitioning
FSharp.Data.SQLProvider (PostgreSQL provider) and Npgsql
Atlas (atlasgo.io) versioned apply + declarative workflow
CloudNativePG 1.30 release, docs, connection pooling (Pooler CRD)
Dapr: building blocks, service invocation, pub/sub, workflow,
  quickstarts, dapr-store sample, eShopOnDapr (archived — reference only)
.NET Aspire overview
Protocol Buffers + gRPC documentation
RabbitMQ 4.2: quorum queues, publisher confirms/consumer acks
Dragonfly documentation (Redis/Memcached compatibility)
Cilium 1.20: policy enforcement, policy creation/audit mode, Hubble
Argo CD docs, ApplicationSet, best practices (intentional divergence noted)
LVM cache (lvmcache(7)) and Linux bcache docs
Microsoft: ASP.NET Core external auth providers and Identity
Google Identity Services authorization for web (code model/PKCE)
YouTube Data API: OAuth for client-side web apps, resumable uploads,
  videos.insert
MDN: WebRTC, WebTransport (optional/future paths only)
```

## Plain-English glossary

Quick definitions for every term this design relies on:

**BFF** — Backend For Frontend. The browser-facing server tailored to one
client/application.

**MVU** — Model-View-Update. Elmish state architecture where messages
transform a model and the view is derived from the model.

**IndexedDB** — Browser-native persistent structured database.

**Service Worker** — Browser background worker able to intercept/cache
application network resources and implement a PWA asset snapshot.

**SWR** — Stale-While-Revalidate. Show cached data immediately, then check
freshness. In this design it is triggered only by an active route/scope.

**DTO** — Data Transfer Object. Explicit contract crossing a process/network
boundary.

**Domain model** — Business concepts/rules independent of transport/database
implementation.

**Port** — An abstraction/capability required by application logic, such as
`IPostStore`.

**Adapter** — Concrete implementation of a port, such as PostgreSQL or Dapr.

**DI** — Dependency Injection. Runtime composition mechanism that supplies
concrete dependencies to code that declares what it needs.

**Bounded context** — A coherent business boundary that is a better
microservice unit than an individual function.

**gRPC** — RPC framework typically using HTTP/2 and Protocol Buffers. It is
not a compression algorithm.

**Protobuf** — Schema-driven compact binary serialization/wire format.

**Dapr** — Distributed Application Runtime. Reusable sidecar/control-plane
building blocks for service invocation, pub/sub, workflow, etc.

**Outbox** — Database table/record written transactionally with business data
so events can be published reliably after commit.

**Inbox** — Consumer-side deduplication/idempotency record used to tolerate
duplicate delivery.

**HybridCache** — .NET two-level cache abstraction with in-process cache and
optional distributed secondary cache.

**Dragonfly** — Redis/Memcached-compatible multithreaded in-memory datastore
selected as the distributed cache.

**CloudNativePG** — Kubernetes operator for PostgreSQL lifecycle.

**dm-cache** — Linux device-mapper hot-spot block cache used by LVM to
accelerate a large slow logical volume with smaller fast storage.

**Hubble** — Cilium network/security observability layer.

**Tombstone** — Explicit record/event stating that an entity was deleted.

**Cursor** — Opaque/ordered synchronization or pagination position.

**Optimistic concurrency** — Update method that checks the version expected by
the caller instead of holding a long lock.

**BYOA** — Bring Your Own Account. User authorizes their own external provider
account to store media.