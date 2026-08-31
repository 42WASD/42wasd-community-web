# App/runtime cache versioning

The PWA Service Worker and IndexedDB are separate.

```text
Service Worker:
  application executable/static snapshot

IndexedDB:
  mutable local application data
```

Do not use the IndexedDB version to represent the .NET runtime version.

Do not use the Service Worker asset manifest as the database schema version.

## On release

```text
new static assets
    ↓
new Service Worker snapshot

separately:

client opens IndexedDB
    ↓
upgrade database schema if needed
```

If the network contract becomes incompatible:

```text
server returns UpgradeRequired
    ↓
client asks user to reload
    ↓
new app snapshot activates
```
