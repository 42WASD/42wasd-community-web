# Add durable background jobs

Define (architecture doc Phase 41):

```text
JobId
JobType
owner
state
progress
createdAt
startedAt
completedAt
error
idempotencyKey
```

Use for:

```text
bulk import
long external sync
large notification fanout
report generation
```

Return JobId immediately.

## Acceptance

```text
[ ] browser can leave
[ ] job continues
[ ] user can reload job status
[ ] cancellation policy explicit
```
