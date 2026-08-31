# Add entity versions and tombstones

Database writes increment/use entity revision (architecture doc Phase 22).

Change feed includes:

```text
upsert
delete tombstone
sequence/cursor
```

## Acceptance

```text
[ ] edit replaces cached older version
[ ] delete removes cached entity
[ ] duplicate event is harmless
[ ] gap triggers repair
```
