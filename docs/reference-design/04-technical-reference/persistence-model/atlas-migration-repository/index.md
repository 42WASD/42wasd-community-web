# Atlas migration repository

Recommended:

```text
database/
  atlas.hcl
  schema/
    identity.sql
    forum.sql
    game.sql
    tournament.sql
    notification.sql
    platform.sql

  migrations/
    202608270001_initial.sql
    atlas.sum
```

## Development workflow

```text
edit desired schema
    ↓
atlas migrate diff
    ↓
review generated SQL
    ↓
ephemeral-PostgreSQL migration apply/test; optional Atlas Pro `migrate lint`
    ↓
PR
```

## Deployment

```text
backup/recovery readiness
    ↓
migration job
    ↓
atlas migrate apply
    ↓
application rollout
```

Do not allow every application Pod to race to run production migrations on
startup.
