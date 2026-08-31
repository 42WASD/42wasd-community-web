# Observability semantic conventions

Every request should have:

```text
trace ID
request/operation ID
authenticated account ID where privacy policy permits
service name
route/RPC method
result status
duration
```

Every queued job should propagate:

```text
correlation/trace context
job ID
```

Every outbox event should include:

```text
event ID
aggregate ID
occurredAt
```

## This permits

```text
browser action
  ↓
BFF span
  ↓
gRPC span
  ↓
Forum service
  ↓
SQL
  ↓
outbox
  ↓
RabbitMQ
  ↓
Notification worker
```

to be investigated as one logical operation.
