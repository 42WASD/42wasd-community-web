# Extract the Tournament Service

Owns (architecture doc Phase 36):

```text
tournaments
members
brackets
match schedule
state transitions
```

Keep long orchestration optional until needed.

## Acceptance

```text
[ ] tournament writes are transactional
[ ] invalid state transitions rejected by pure domain functions
```
