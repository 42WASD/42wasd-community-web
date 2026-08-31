# End-to-end realtime mutation state machine

```text
User A creates comment
        │
        ▼
BFF
        │
        ▼
Forum Service
        │
        ▼
PostgreSQL transaction
  insert comment
  increment/version projection as needed
  insert change-log row
  insert outbox row
        │
        ▼
COMMIT
        │
        ├──────────────► return CommentDto to User A
        │
        ▼
Outbox publisher
        │
        ▼
Dapr Pub/Sub
        │
        ▼
Realtime/Notification consumer
        │
        ▼
SignalR group topic:123
        │
        ▼
only currently subscribed browsers
        │
        ▼
version/sequence check
        │
        ├── inline delta sufficient -> apply
        └── otherwise -> active-scope refetch
```

Users not viewing Topic 123 receive nothing from this realtime path.
