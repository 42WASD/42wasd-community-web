# Build the IQueryCoordinator

Before adding realtime, solve duplicate fetches (architecture doc Phase 19).

## Features

```text
query key
one in-flight loader
subscriber count
cache state
stale state
cancel policy
invalidate
prefetch
```

Prototype DotNetQuery if useful.

## Critical override

```text
NO timer-driven global forum background refetch
```

The active route owns permission to revalidate.

## Acceptance

```text
[ ] two components same key -> one network call
[ ] route leave decrements subscriber
[ ] expensive zero-subscriber request cancels
[ ] cheap almost-complete cacheable request can finish
```
