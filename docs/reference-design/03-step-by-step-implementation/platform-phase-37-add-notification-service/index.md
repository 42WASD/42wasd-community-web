# Add the Notification Service

Do not make notifications a synchronous dependency of every write
(architecture doc Phase 37).

## Flow

```text
Forum Service commits PostCreated
      ↓
outbox
      ↓
Dapr Pub/Sub
      ↓
Notification Service
      ↓
persist notification / route realtime if recipient currently active
```

## Acceptance

```text
[ ] forum post succeeds even if notification worker is temporarily unavailable
[ ] notification catches up after recovery
```
