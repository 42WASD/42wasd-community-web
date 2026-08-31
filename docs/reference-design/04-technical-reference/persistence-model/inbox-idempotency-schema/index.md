# Inbox/idempotency schema

Illustrative:

```sql
CREATE TABLE platform.inbox (
    consumer text NOT NULL,
    message_id uuid NOT NULL,
    processed_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (consumer, message_id)
);
```

## Consumer logic

```text
begin transaction
  ↓
insert inbox key
  ↓
if duplicate:
    no-op / acknowledge
  ↓
apply business change
  ↓
commit
  ↓
ack message
```

This makes at-least-once delivery safe for operations designed to be
idempotent.
