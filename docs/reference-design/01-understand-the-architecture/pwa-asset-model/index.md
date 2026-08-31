# PWA asset model

During publish, the Blazor build produces a hashed asset manifest.

Use it to cache a coherent snapshot:

```text
.NET WebAssembly/runtime assets
compiled app assets
CSS
JavaScript
icons
static UI assets
```

Do not place a huge catalog of dynamic forum images inside the PWA `wwwroot`
manifest; the service worker may try to cache every manifest-listed asset.

## Two separate version axes

Runtime/application update and IndexedDB schema update are separate:

```text
App version
    -> Service Worker snapshot/version

Local data version
    -> IndexedDB database version / migration
```

On incompatible disposable cache change:

```text
drop/recreate cache_* stores
lazy refill active scopes
```

Preserve/migrate:

```text
drafts
outbox
upload_sessions
```

Old tabs can block an IndexedDB schema upgrade. Handle `versionchange`/blocked
events and ask the user to reload when necessary.
