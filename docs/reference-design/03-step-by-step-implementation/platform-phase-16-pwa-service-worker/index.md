# Enable PWA Service Worker

Add/verify (architecture doc Phase 16):

```text
manifest.json
service-worker.js
service-worker.published.js
service-worker asset manifest
```

Cache only application/static resources appropriate for offline startup.

## Do not cache

```text
all forum media
arbitrary dynamic API responses
millions of content URLs
```

## Acceptance

```text
[ ] first visit downloads runtime
[ ] refresh uses local app snapshot
[ ] new deployment downloads new snapshot coherently
[ ] old app continues until update activation/reload
```
