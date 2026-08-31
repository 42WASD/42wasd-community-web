# BFF-to-service rule

Every synchronous BFF call must have:

```text
deadline
CancellationToken
bounded retry policy
trace context
```

## Do not allow

```text
browser waits forever
    ↓
BFF waits forever
    ↓
service waits forever
```

Cancellation propagation should stop expensive work when the result has no
remaining consumer.

## Exception

```text
accepted durable command
```

has already crossed into queue/workflow ownership and is no longer tied to
browser connection lifetime.
