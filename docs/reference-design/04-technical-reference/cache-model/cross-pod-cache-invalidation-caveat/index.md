# Cross-Pod cache invalidation caveat

HybridCache invalidation updates:

```text
current process L1
distributed L2
```

but another Pod may still have its own short-lived L1 copy.

## Therefore

```text
keep L1 TTL short
```

or distribute explicit invalidation messages where correctness demands it.

## Never

Never put authorization-critical truth behind a cache policy that tolerates
stale access decisions.
