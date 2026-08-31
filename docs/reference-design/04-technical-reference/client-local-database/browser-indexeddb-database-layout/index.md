# Browser IndexedDB database layout

Recommended logical database:

```text
database:
  community-client

stores:
  cache_entries
  entity_versions
  scope_cursors
  drafts
  outbox
  upload_sessions
  client_metadata
```

## Suggested semantics

### `cache_entries`

```text
key
scope
entityType
entityId
version
payload
cachedAt
```

Disposable.

### `entity_versions`

```text
entityType
entityId
serverVersion
```

Used to avoid applying older realtime messages.

### `scope_cursors`

```text
scopeKey
cursor
lastSuccessfulSyncAt
contractVersion
```

The cursor advances only in the same IndexedDB transaction that successfully
stores all corresponding changes.

### `drafts`

```text
draftId
ownerAccountId
kind
payload
updatedAt
```

User-owned local data.

Do not casually delete on application cache migration.

### `outbox`

```text
operationId
kind
payload
expectedVersion
createdAt
retryCount
state
```

Used only for explicitly supported offline/resumable client actions.

### `upload_sessions`

```text
uploadId
provider
postId
providerSessionReference
localFileMetadata
bytesConfirmed
state
updatedAt
```

Do not store the entire large file body in IndexedDB merely to support uploads.

### `client_metadata`

```text
indexedDbSchemaVersion
contractVersion
lastAppVersion
```
