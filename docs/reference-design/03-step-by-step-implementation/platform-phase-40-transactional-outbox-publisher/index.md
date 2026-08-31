# Implement the transactional outbox publisher

Each owning service DB transaction writes outbox (architecture doc Phase 40).

Publisher process:

```text
select unpublished with safe locking
publish
confirm
mark published
```

Support multiple publisher instances without duplicate corruption.

Duplicates are allowed at transport level; consumers are idempotent.

## Acceptance

```text
[ ] crash after DB commit before publish recovers
[ ] crash after publish before mark may duplicate but does not duplicate business effect
```
