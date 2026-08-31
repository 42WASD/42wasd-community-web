# End-to-end forum read state machine

```text
USER ENTERS TOPIC 123
        │
        ▼
activate scope Topic:123
        │
        ▼
RequestCoordinator asks LocalStore
        │
   ┌────┴─────┐
   │          │
 cache hit   miss
   │          │
render stale  │
   │          │
   └──────┬───┘
          ▼
foreground revalidate
          │
          ▼
BFF HybridCache L1
          │ miss
          ▼
Dragonfly L2
          │ miss
          ▼
Forum Service gRPC
          │
          ▼
PgBouncer
          │
          ▼
PostgreSQL
          │
          ▼
RAM/page cache
          │ miss
          ▼
NVMe dm-cache
          │ miss
          ▼
HDD
          │
          ▼
result
          │
          ├─ populate caches as policy allows
          ▼
BFF compressed response
          │
          ▼
IndexedDB transaction
  data + cursor commit
          │
          ▼
Elmish update
          │
          ▼
join SignalR group topic:123
```

## On route leave

```text
leave topic:123
cancel no-longer-useful expensive reads
keep already committed cache
NO further Topic:123 sync
```
