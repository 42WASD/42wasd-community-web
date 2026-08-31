# Understand the architecture

Part I is the conceptual foundation of the **42WASD Community Web** design. It
defines *why* the architecture is shaped the way it is, before any code is
written — from the browser UI through the ten-layer platform model down to
Kubernetes networking and physical storage.

This part is `tracked: false` — it is narrative/conceptual and does not appear
on the Implementation progress page.

## Reading order

### Client and architecture core

- **The one-sentence idea** — the entire architecture in a single sentence,
  with the platform mental model and the crucial inequality rules.
- **Why tempting architectures fail** — the pitfalls that motivate the design:
  six client-side problems plus the ten platform-side problems.
- **The ten-layer mental model** — Presentation, Local Data, Client Data
  Coordination, Client Synchronization, Realtime, Browser-Facing Server,
  Domain Services, Distributed Plumbing, Data, Platform (the original
  six-layer client model is retained inside the Presentation layer).
- **Functional core, imperative shell** — the coding rule for the platform.
- **The state ownership model** — who owns each kind of state.
- **The source-of-truth model** — what each store is authoritative for.
- **The selected tool stack** — the pinned tool per layer.
- **Rejected foundations** — attractive tools deliberately not selected.

### Client data and synchronization

- **Route-scoped synchronization contract** — active scopes only, cursors,
  tombstones, optimistic concurrency.
- **Client query coordinator** — dedup/subscriber/cancellation semantics.
- **Client local database layout** — disposable vs user-owned local stores.
- **PWA asset model** — coherent hashed snapshot, separate version axes.
- **Realtime contract** — lazy SignalR, groups, tiny events, gap repair.

### Contracts and persistence

- **Browser/BFF contract design** — explicit Shared DTOs, projection boundary.
- **F# domain type conventions** — strong IDs, closed DUs.
- **Internal microservice contracts** — gRPC/Protobuf wire rules.
- **ID convention** — UUIDv7.
- **PostgreSQL schema design rules** — native types, bounded jsonb.
- **Database-first F# access** — schema → SQLProvider.
- **Migration policy** — Atlas diff/apply, expand/contract.

### Caching, messaging, and durable work

- **Cache architecture** — HybridCache L1/L2, invalidation rule.
- **Dragonfly role** — disposable L2 cache, not truth.
- **SignalR scale-out** — backplane qualification.
- **Dapr role** — plumbing, not business logic.
- **Reference applications** — donors to study, not fork.
- **The 42wasd-service-template** — pre-wired service skeleton.
- **Async work architecture** — query vs command vs workflow.
- **RabbitMQ reliability rules** — durable queues, confirms, dead letters.
- **Transactional outbox** — atomic business write + event.
- **Dapr Workflow** — only for genuine multi-step durable flows.
- **Rate limiting and backpressure** — per-replica vs product quotas.
- **Multi-source page rendering** — progressive per-source composition.
- **Response compression** — zstd/br/gzip negotiation and order.

### Storage and platform

- **Database physical storage** — HDD origin + NVMe dm-cache.
- **Why writethrough first** — cache-device failure safety.
- **CloudNativePG** — operator, single-node reality, image pinning.
- **PgBouncer connection pooling** — Npgsql first, session mode first.
- **Database backup policy** — independent target, plugin-based Barman path.

### Network and GitOps

- **Cilium security model** — default deny, explicit allows.
- **Hubble observability** — flow evidence workflow.
- **Policy discovery** — audit mode in staging only.
- **Dapr plus Cilium** — two separate authorization layers.
- **GitOps ownership model** — app manifests stay with app source.
- **Central infrastructure repository** — platform components, pinned charts.
- **Application repository** — per-app deploy/k8s ownership.
- **Argo CD application generation** — AppProjects/ApplicationSets.
- **Local development with Aspire** — dev orchestration, not production.
- **Observability** — correlation fields, metric targets.

### Client organization (unchanged)

- **Message organization** — keeping the root message small.
- **Page and feature organization** — route is not always feature.
- **Recommended repository structure** — feature-oriented layout.
- **Keeping the Ui folder small** — don't recreate folder-by-type.
- **Developer vs gaming community** — same architecture, different vocabulary.
- **Design language** — visual theme is independent from MVU architecture.

## Core ideas

Build one **Bolero `ProgramComponent` / Elmish program** whose root model stays
small. Keep cross-page data in a persistent `Shared.Model`, temporary state
close to the page that owns it, use Bolero routing and `PageModel<'T>` for
route state, `Cmd` for effects, and pure view functions for ordinary UI.

At platform scale, the same ownership discipline extends beyond the browser:

```text
route state != shared domain state
page-local state != persistent application state
visual component != independent MVU program
effect != direct mutation
framework shell != application architecture

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