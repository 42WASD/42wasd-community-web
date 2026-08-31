# Technical reference

Part IV is a compact technical reference for the design decisions in this
document. It is `tracked: false` — narrative/reference, not on the progress
page.

## Reading order

The sections capture the canonical patterns and rules:

### Client/state reference (original)

- **The root source of truth model** — how state is shaped.
- **The root message** — how messages flow.
- **The recommended page shape** — the standard page module.
- **The recommended RemoteData** — async state handling.
- **Normalized entity state** — canonical entity caches.
- **The events page state** — a concrete example.
- **The dependency rule** — who may depend on whom.
- **The routing state rule** — route as source of truth.
- **The state lifetime rule** — how long state survives.
- **The functional acceptance test** — what "done" means.
- **Performance principles** — when to optimize.
- **Upgrade policy** — how upgrades are adopted.
- **Abstraction philosophy** — when to abstract.
- **Primary references** — the evidence base, plus the plain-English glossary.
- **The final recommendation** — the one-paragraph conclusion.

### Platform reference groups

- **Source-of-truth model** — ownership table; application-layer ownership.
- **Service code shape** — F# service layout; Functional Core contract.
- **Browser scope model** — scopes, lifecycle machine, query keys, policies.
- **Client local database** — IndexedDB layout, upgrades, cache versioning.
- **Contract model** — handshake, DTO organization, IDs, projection,
  progressive composition, streamed fragments.
- **Realtime model** — subscription identity, envelope, delta-sync API,
  pagination, prefetch.
- **BFF contract** — responsibilities; BFF-to-service rule.
- **Internal contract model** — proto layout, evolution, no shared F# types.
- **Persistence model** — schemas, forum example, change log, concurrency,
  outbox/inbox, Atlas, release compatibility, SQLProvider role.
- **Database topology** — connections, budget, CNPG Cluster/Pooler, layering.
- **Cache model** — candidates, key versioning, HybridCache, invalidation,
  Dragonfly, backplane qualification.
- **Messaging model** — RabbitMQ topology, work classes, jobs, workers,
  batching, reliability.
- **Dapr model** — ownership, invocation, pub/sub, workflow qualification.
- **BYOA media model** — MediaRef, upload states, client rules, rendering
  security.
- **Identity model** — data model; login resolution.
- **Admission control** — rate-limit classes, backpressure, compression,
  serialization.
- **Storage model** — storage contract, dm-cache runbook, writethrough,
  hot/cold placement.
- **Backup model** — architecture; test policy.
- **Network model** — dependency graph, default-deny, policy example, Hubble
  workflows and queries.
- **GitOps model** — repo ownership, Argo boundaries, secrets, Data
  Protection.
- **Observability reference** — conventions, metrics, SLOs, alerts.
- **Product capabilities** — search, WebRTC, mobile.
- **Release model** — artifact, upgrade order, deprecation, growth, NVMe
  expectations.
- **End-to-end state machines** — the eight complete flows.
- **Failure behavior** — network and provider failure rules.
- **Security invariants** — the never-violate list.
- **Acceptance tests** — performance, load, upgrade, disaster matrices.
- **Capacity model** — planning, premature optimization, growth order.
- **Platform order** — the final production dependency order and the final
  master checklist.