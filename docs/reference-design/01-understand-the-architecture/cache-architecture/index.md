# Cache architecture

Selected:

```text
HybridCache
    L1 = per-Pod MemoryCache
    L2 = Dragonfly
```

## Read path

```text
request
   ↓
HybridCache L1
   │ hit -> return
   ↓ miss
Dragonfly L2
   │ hit -> populate L1 -> return
   ↓ miss
PostgreSQL / backend service
   ↓
populate L2
   ↓
populate L1
```

## Stampede behavior

HybridCache's stampede protection coalesces same-key work within one
HybridCache instance. It does not globally single-flight all Pods.

That is acceptable initially:

```text
3 BFF Pods
instead of 3,000 DB misses
may produce ~3 origin fetches
```

Do not add distributed locks around every cache miss unless profiling proves
it necessary.

## Invalidation rule

PostgreSQL remains truth.

When a write commits:

```text
commit business row
+
commit outbox event
```

then event publication can cause:

```text
relevant cache key/tag invalidation
+
active realtime notification
```

Use versioned keys/projections where useful:

```text
forum:topic:{id}:v{version}
```

Cache invalidation failure must not corrupt truth; at worst it briefly serves
stale content within bounded TTL/version checking.
