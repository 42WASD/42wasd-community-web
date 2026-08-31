# Final master checklist

The closing checklist of the verified architecture. When all of these
contracts are true, `42wasd-community-web` has evolved from a browser UI
backed by mutable JSON-era state into a typed, persistent, cache-aware,
bandwidth-conscious, Kubernetes-native community platform without sacrificing
the advantages that originally motivated the F#/Bolero/Elmish stack.

## Architecture

```text
[ ] browser, BFF, services, DB, cache, queue and media ownership are separate
[ ] no component is being used outside its intended responsibility merely for convenience
[ ] business service boundaries are coarse bounded contexts
```

## Client

```text
[ ] Bolero/Elmish/Radzen upgraded and pinned
[ ] PWA Service Worker enabled
[ ] IndexedDB stores separated into disposable vs precious
[ ] query coordinator abstraction implemented
[ ] route-scoped sync only
[ ] no inactive global forum refresh
[ ] active SignalR groups only
[ ] contract handshake implemented
```

## Server/BFF

```text
[ ] Community.Web.Server is thin BFF
[ ] response compression benchmarked
[ ] rate limits named by workload class
[ ] HybridCache configured
[ ] cancellation/deadline propagation works
```

## Services

```text
[ ] Accounts boundary
[ ] Forum boundary
[ ] Game/Server boundary
[ ] Tournament boundary
[ ] Notification/worker boundary
[ ] gRPC/Protobuf contracts versioned
[ ] Dapr sidecars/configuration in place
```

## Persistence

```text
[ ] PostgreSQL 18
[ ] UUIDv7 convention
[ ] native types
[ ] SQLProvider/Npgsql
[ ] Atlas migration workflow
[ ] PgBouncer pool budget
[ ] optimistic concurrency
[ ] change logs/tombstones
[ ] transactional outbox/inbox
```

## Caching

```text
[ ] HybridCache L1
[ ] Dragonfly L2
[ ] invalidation/TTL documented per cache key
[ ] cache can be destroyed safely
```

## Async

```text
[ ] RabbitMQ/Dapr PubSub
[ ] publisher/consumer reliability configured
[ ] worker concurrency isolated by workload
[ ] durable workflow used only where justified
```

## Media

```text
[ ] BYOA policy
[ ] provider-neutral MediaRef
[ ] YouTube resumable upload
[ ] no bulk media relay through BFF
[ ] upload resume state in IndexedDB
```

## Storage

```text
[ ] HDD + 30 GB NVMe dm-cache
[ ] smq
[ ] writethrough initially
[ ] stable Local PV
[ ] CNPG volume uses correct StorageClass
[ ] independent backup destination
[ ] restore tested
```

## Security/network

```text
[ ] Cilium default deny
[ ] app dependency policies
[ ] Hubble visibility
[ ] Audit Mode not left enabled in production
[ ] Argo-reviewed network changes
[ ] no cluster-admin business service accounts
```

## GitOps

```text
[ ] central cluster-infra ownership clear
[ ] application manifests remain in application repo
[ ] AppProjects restrict privileges
[ ] third-party sources pinned
[ ] exact production image digests recorded
```

## Operations

```text
[ ] dashboards
[ ] alerts
[ ] tracing
[ ] queue metrics
[ ] DB metrics
[ ] dm-cache/disk metrics
[ ] Cilium/Hubble network metrics
[ ] load test
[ ] failure test
[ ] backup restore test
[ ] rollback runbook
```

This checklist is the summary form of the platform functional acceptance test
and the production readiness gate; use either level depending on whether you
are auditing the whole platform or one release.
