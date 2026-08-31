# Persistence model

PostgreSQL logical schemas, forum schema example, change log, concurrency,
outbox/inbox, Atlas repository, release compatibility, and the SQLProvider
role.

## Subsections

- **PostgreSQL logical schema organization** — schemas per bounded context.
- **Core forum schema example** — illustrative posts/comments SQL.
- **Change-log design for active-scope sync** — per-scope sequence feed.
- **Change-log retention** — reset path when cursors age out.
- **Optimistic concurrency in PostgreSQL** — version-guarded UPDATE.
- **Transactional outbox schema** — platform.outbox.
- **Inbox/idempotency schema** — platform.inbox.
- **Atlas migration repository** — schema/migrations layout.
- **Migration release compatibility** — expand/contract.
- **SQLProvider role** — infrastructure dependency, not contract owner.
