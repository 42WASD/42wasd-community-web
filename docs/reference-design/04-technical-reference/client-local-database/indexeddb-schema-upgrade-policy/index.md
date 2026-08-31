# IndexedDB schema upgrade policy

Treat local stores as two classes:

```text
DISPOSABLE
  cache_entries
  entity_versions
  scope_cursors

PRECIOUS
  drafts
  outbox
  upload_sessions
```

## Disposable

If a disposable cache format changes incompatibly:

```text
drop/rebuild relevant cache store
```

is valid.

## Precious

If a precious store format changes:

```text
explicit migration
test upgrade from every supported previous version
never silently delete
```

## Release testing must include

```text
old application + old IndexedDB
new application install
onupgradeneeded
migration
draft still present
outbox still present
upload session still resumable
```
