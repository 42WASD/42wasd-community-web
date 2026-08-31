# Define IndexedDB version/migration policy

Database version (architecture doc Phase 18):

```text
v1
v2
...
```

Every release that changes important local store structure has explicit
migration.

For disposable cache incompatibility:

```text
drop affected cache store
recreate
```

For user work:

```text
migrate
never silently discard
```

## Acceptance

```text
[ ] upgrade from previous production IndexedDB version tested
[ ] old-tab blocked upgrade UX tested
[ ] downgrade behavior defined
```
