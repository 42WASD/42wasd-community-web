# Transactional outbox

## The pattern

Forum write:

```text
BEGIN PostgreSQL transaction

INSERT post
INSERT outbox_event(PostCreated)

COMMIT
```

A separate publisher:

```text
read unpublished outbox
    ↓
publish through Dapr/RabbitMQ
    ↓
receive confirmation
    ↓
mark published
```

## What it avoids

```text
DB commit succeeds
process crashes before event publish
```

## Consumers

Consumers use an inbox/dedup table or idempotency key where duplicate delivery
would be harmful.

Duplicate publish is expected and must be tolerated — the outbox publisher can
die after publishing but before marking the row published.
