# Transactional outbox schema

Illustrative:

```sql
CREATE SCHEMA IF NOT EXISTS platform;

CREATE TABLE platform.outbox (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    topic text NOT NULL,
    event_type text NOT NULL,
    aggregate_id uuid NULL,
    payload jsonb NOT NULL,
    occurred_at timestamptz NOT NULL DEFAULT now(),
    published_at timestamptz NULL,
    attempts integer NOT NULL DEFAULT 0
);

CREATE INDEX outbox_unpublished_idx
    ON platform.outbox (occurred_at)
    WHERE published_at IS NULL;
```

In the same transaction:

```text
UPDATE/INSERT business row
+
INSERT outbox event
+
COMMIT
```

Then a publisher worker:

```text
reads unpublished rows
publishes through Dapr Pub/Sub
marks published
```

Consumers must still be idempotent because duplicate publication/delivery can
occur.
