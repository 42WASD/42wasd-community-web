# Change-log design for active-scope sync

One straightforward design:

```sql
CREATE TABLE forum.change_log (
    sequence bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    scope_kind text NOT NULL,
    scope_id uuid NOT NULL,
    entity_kind text NOT NULL,
    entity_id uuid NOT NULL,
    entity_version bigint NOT NULL,
    operation text NOT NULL,
    occurred_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX forum_change_scope_sequence_idx
    ON forum.change_log (scope_kind, scope_id, sequence);
```

For a topic edit:

```text
scope_kind = topic
scope_id = topic ID
operation = upsert
```

A category list may also need a category-level change record when a post
affects that projection.

## The rule

Do not emit one universal feed to every browser.

Feeds are queried only for active scopes.
