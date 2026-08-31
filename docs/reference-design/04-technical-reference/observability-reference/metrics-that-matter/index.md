# Metrics that matter

## Browser/BFF

```text
route cold-load latency
route cache-render latency
active-scope revalidation latency
bytes per route transition
compressed/uncompressed response bytes
SignalR connected clients
SignalR group memberships
contract upgrade-required count
```

## RequestCoordinator

```text
query dedupe ratio
query cancellation count
prefetch hit ratio
cache hit ratio
in-flight query count
```

## Services

```text
RPC p50/p95/p99
error rate
cancellation rate
external API latency
```

## PostgreSQL

```text
active sessions
pool wait
slow queries
cache hit ratio
WAL rate
checkpoint behavior
disk latency
database size
table/index bloat
```

## Dragonfly

```text
memory use
hit/miss
evictions
commands/sec
latency
```

## RabbitMQ

```text
queue depth
oldest message age
publish rate
delivery rate
redeliveries
consumer utilization
dead-letter count
```

## Storage

```text
HDD latency
NVMe health
dm-cache hit/miss
dirty cache blocks if writeback ever enabled
filesystem capacity
```

## Network

```text
Cilium denied flows
unexpected egress
DNS failures
Dapr invocation failures
```
