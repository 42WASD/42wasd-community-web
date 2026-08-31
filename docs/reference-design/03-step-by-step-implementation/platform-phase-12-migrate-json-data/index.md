# Migrate current JSON-backed data

Write one migration/import program (architecture doc Phase 12).

## Rules

```text
read current JSON
validate every record
assign stable new IDs/mappings
insert in transaction/batches
produce reconciliation report
```

Do not let the production web app simultaneously write JSON and PostgreSQL
indefinitely.

## Possible transition

```text
maintenance window
stop writes
export JSON snapshot
run importer
verify counts/checks
switch application DB adapter
deploy
```

If zero downtime is required later, design dual-write/change-capture carefully;
it is probably unnecessary for the current migration.

## Acceptance

```text
[ ] record counts match
[ ] key user/profile data matches
[ ] rollback snapshot exists
[ ] application reads PostgreSQL after cutover
[ ] JSON is no longer authoritative
```
