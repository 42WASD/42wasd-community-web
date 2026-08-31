# Observability requirements

## Browser

Sample rather than collect every event.

Track:

```text
app version
contract version
route
cache hit source: IndexedDB/network
first useful paint/content
query duration
bytes received
sync delta count
SignalR reconnect count
upload progress/failure class
IndexedDB migration failure
```

Do not collect private forum content merely for diagnostics.

## BFF

Track:

```text
HTTP/RPC request count
p50/p95/p99
active requests
cancellations
rate-limit rejection
compression encoding
bytes before/after compression where practical
HybridCache L1 hit
HybridCache L2 hit
origin load
SignalR connections/groups/messages
```

## Services

Track:

```text
gRPC request latency
deadline exceeded
retry count
Dapr invocation errors
Postgres query duration
connection pool usage
outbox backlog
inbox dedup count
```

## RabbitMQ

Track:

```text
ready messages
unacked messages
oldest message age
publish confirm latency
consumer rate
redeliveries
dead-letter count
prefetch/concurrency
disk/memory alarms
```

## PostgreSQL

Track:

```text
connections
transaction rate
lock waits
deadlocks
slow queries
buffer hit ratio
table/index size
vacuum/analyze
WAL
checkpoint latency
disk I/O
replication if later added
```

## Storage

Track:

```text
HDD latency
HDD queue depth
NVMe health
dm-cache hit/miss
cache usage
cache promotions/demotions
filesystem usage
PVC usage
```

30 GB fast capacity means cache churn must be watched.

## Network

Hubble:

```text
drops
policy denied
DNS
HTTP/L7 where enabled and justified
source/destination workloads
```

Avoid exploding Hubble metric label cardinality.
