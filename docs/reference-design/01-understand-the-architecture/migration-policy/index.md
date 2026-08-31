# Migration policy

Production workflow:

```text
developer changes desired schema
       ↓
atlas migrate diff
       ↓
generated versioned SQL migration
       ↓
migration validation against ephemeral PostgreSQL in CI; add
`atlas migrate lint` only when Atlas Pro is intentionally enabled
       ↓
review SQL in PR
       ↓
merge
       ↓
build migration artifact/job
       ↓
deployment migration step
       ↓
atlas migrate apply
       ↓
roll out services that depend on new schema
```

## No startup auto-migration

Do not have every application replica race to auto-migrate the production
database on startup.

## Expand/contract for zero-downtime changes

For zero/low-downtime schema changes use **expand/contract**:

```text
1. add compatible new column/table
2. deploy code that can read/write both if necessary
3. backfill
4. switch reads
5. remove old dependency later
6. drop old column in a later migration
```

## Licensing note

`atlas migrate diff` and `atlas migrate apply` are usable in the OSS CLI;
`atlas migrate lint` is an Atlas Pro feature from v0.38. OSS CI must not assume
that command is available.
