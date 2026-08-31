# Performance principles

Optimize only by evidence.

## Principles

- Write correct, simple MVU first.
- Measure before optimizing.
- Use `ElmishComponent` / render isolation only where measured to matter.
- Keep most rendering in pure functions.
- Prefer `Map` lookups over traversals for entity data.

## The guide

```text
Profile or identify the actual bottleneck
Fix only that
Re-measure
```

## Performance: when it matters

- Large lists re-rendered on every keystroke → isolate or memoize.
- Expensive derivations → compute once, reuse.
- Normalize data so updates are O(1) per entity.

## Platform optimization order

At platform scale, optimize in this order — never reversed:

```text
correct service boundary
correct data ownership
correct query/projection
active-scope-only synchronization
client cache
request deduplication
server cache
database indexes
async/cancellation
batching/backpressure
compact serialization
compression
low-level format tuning
```

A 20% better codec cannot compensate for downloading 1,000 rows the user
never sees.

## When traffic grows

Optimize first:

```text
1. query/index plans
2. avoid unnecessary data
3. route-scoped synchronization
4. cache hot projections
5. batch appropriate origin calls
6. DB pool sizing
7. disk/cache hit analysis
8. service concurrency
9. compression tuning
10. protocol changes only if still justified
```