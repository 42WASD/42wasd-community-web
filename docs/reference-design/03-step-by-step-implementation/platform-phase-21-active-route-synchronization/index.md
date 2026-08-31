# Implement active-route synchronization

Build route hooks/messages (architecture doc Phase 21):

```text
ActivateScope
ScopeCacheLoaded
ScopeSyncStarted
ScopeChangesApplied
DeactivateScope
```

## Rules

```text
inactive -> no sync
active -> one revalidation
reconnect -> active scope only
```

## Acceptance

```text
[ ] user on profile causes zero forum sync
[ ] entering forum causes only visible list sync
[ ] entering topic stops irrelevant list live subscription
[ ] leaving forum stops forum synchronization
```

This acceptance test is **mandatory** because it encodes the bandwidth
requirement.
