# Implement cache invalidation events

After transactional write (architecture doc Phase 28):

```text
outbox event
    ↓
invalidate key/tag/version
```

Do not build hundreds of handwritten interdependent invalidation calls spread
through controllers.

Centralize cache-key construction.

## Acceptance

```text
[ ] update becomes visible after bounded delay
[ ] stale cache cannot remain forever
```
