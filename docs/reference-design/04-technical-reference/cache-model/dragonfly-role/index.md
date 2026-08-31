# Dragonfly role

Selected:

```text
Dragonfly
  Redis-compatible L2 cache
  shared hot projections
  optional SignalR backplane after compatibility validation
```

## Do not make it

```text
forum database
account source of truth
migration store
only copy of job state
```

## For a first deployment on the same physical server

```text
one Dragonfly instance
resource limits
persistence optional depending on cache-only use
```

If it is purely disposable:

```text
persistence is not required for correctness
```
