# Step-by-step implementation

Part III is the **tracked** part: its phases drive the Implementation progress
page.

This part is `tracked: true`. Each phase below is a unit of work; the
Implementation page records `not-started` / `in-progress` / `done` for each.

## Reading order

### Client foundation (phases 0–19)

Phase 0 establishes the ownership rules that every later phase follows, then
the phases proceed through foundation, shared contracts, remoting, real
features, and finally the production-ready client rollout.

The phases are deliberately ordered so that each one leaves the app in a
working, verifiable state.

### Platform evolution (phases 20–80)

The platform phases continue where the client phases end, evolving the working
Bolero app into the stateful platform architecture:

- **20–24 Vocabulary → preflight → runtime → AppHost → GitOps → Cilium** —
  freeze names, verify the cluster/host, upgrade to .NET 10/Bolero 0.25,
  stand up local Aspire orchestration, prepare GitOps ownership and the
  Cilium/Hubble baseline.
- **25–32 Storage and database** — build the HDD+NVMe dm-cache volume, expose
  it as a local PV, install CloudNativePG, create least-privilege roles, first
  Atlas schema, SQLProvider access, migrate JSON data, real authentication.
- **33–38 Contracts and BFF** — Shared contract versioning, explicit BFF,
  PWA service worker.
- **39–44 Client data engine** — IndexedDB local store and migration policy,
  query coordinator, cursor/keyset pagination, active-route synchronization,
  entity versions/tombstones, optimistic concurrency.
- **45–48 Realtime** — SignalR for one topic first, then scoped expansion.
- **49–51 Caching and delivery** — HybridCache L1, Dragonfly L2, invalidation
  events, response compression.
- **52–53 Admission control** — rate limiting with per-replica vs global
  semantics.
- **54–62 Distributed backend** — Dapr deny-by-default, service template,
  extract Forum/Accounts/Game-Server/Tournament/Notification services,
  RabbitMQ, Dapr Pub/Sub reliability.
- **63–67 Durable work and media** — outbox publisher, background jobs, Dapr
  Workflow, BYOA media abstraction, YouTube OAuth/resumable upload, media
  preview.
- **68–72 Composition and capability** — progressive aggregation, search,
  optional WebRTC, Argo rollout model, CI gates.
- **73–77 Network and database operations** — Hubble policy workflow, network
  policy ownership, connection budget, indexing review, storage validation.
- **78–80 Verification and release** — backup/recovery, load testing,
  chaos/failure testing, PWA/mobile release, production readiness gate.