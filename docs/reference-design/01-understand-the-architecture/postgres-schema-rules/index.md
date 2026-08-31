# PostgreSQL schema design rules

Use normalized relational structures for stable domain facts.

Example:

```text
accounts
personas
external_logins
linked_profiles

forum_categories
forum_topics
forum_posts
forum_reactions
forum_subscriptions

conversations
conversation_members
messages

servers
server_integrations

tournaments
tournament_members

outbox_events
consumer_inbox
background_jobs
```

## JSONB is a bounded extension point

Use `jsonb` only for bounded extension points such as provider-specific
metadata:

```text
server_integration.metadata jsonb
media_provider_metadata jsonb
```

If a JSON property becomes:

```text
frequently queried
indexed
required
business-critical
```

promote it into an explicit typed column.

## Native types, not text

Use native PostgreSQL types:

```text
uuid
bigint/int
boolean
timestamptz
text
jsonb only where genuinely flexible
foreign keys
constraints
indexes
```

Hex is not a compact representation. One binary byte becomes two hexadecimal
characters. Base64 also expands binary data.

Wire compactness should be solved by projection, binary serialization where
needed, and compression — not by destroying database types.
