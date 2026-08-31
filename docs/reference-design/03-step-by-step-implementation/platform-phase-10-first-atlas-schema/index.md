# Create the first Atlas schema

Create (architecture doc Phase 10):

```text
db/
├── schema/
└── migrations/
```

Start with foundational tables only.

Example domains:

```text
accounts
personas
external_logins
linked_profiles
forum_categories
forum_topics
forum_posts
forum_reactions
outbox_events
consumer_inbox
```

Use:

```text
UUIDv7-capable IDs
timestamptz
foreign keys
unique constraints
check constraints
```

Do not over-model future features before they exist.

## Acceptance

```text
[ ] empty DB can be created exclusively from migration history
[ ] Atlas lint passes
[ ] migration tested in disposable dev DB
[ ] destructive operations require explicit review
```
