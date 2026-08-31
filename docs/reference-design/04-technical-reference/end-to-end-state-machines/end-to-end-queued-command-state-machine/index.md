# End-to-end queued command state machine

```text
Client requests long operation
       │
       ▼
authenticate + validate
       │
       ▼
create durable JobId
       │
       ▼
persist command/outbox
       │
       ▼
return Accepted(JobId)
       │
       ▼
Dapr/RabbitMQ
       │
       ▼
worker
       │
       ├─ transient failure -> retry
       ├─ duplicate -> idempotent no-op
       ├─ permanent failure -> dead-letter / Failed
       └─ success -> Completed
```

## Client may leave

Job continues because:

```text
queue owns lifetime
```

not the browser connection.
