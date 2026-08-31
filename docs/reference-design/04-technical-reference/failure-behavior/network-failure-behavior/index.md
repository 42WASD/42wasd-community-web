# Network failure behavior

## If Forum Service unavailable

```text
cached route may still render
freshness indicator may show stale
mutation returns controlled failure
BFF does not silently claim success
```

## If Dragonfly unavailable

```text
fall back to origin
protect origin with concurrency limits
alert
```

## If RabbitMQ unavailable

```text
business transaction can still commit with outbox
outbox backlog grows
publisher retries later
```

if product operation does not require immediate broker confirmation.

## If PostgreSQL unavailable

```text
writes fail
uncached authoritative reads fail
serve explicitly safe stale read-only cache only where product policy permits
```

Do not accept writes into memory pretending they are durable.
