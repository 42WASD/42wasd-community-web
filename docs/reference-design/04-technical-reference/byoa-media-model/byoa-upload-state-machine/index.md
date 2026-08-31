# BYOA upload state machine

```text
LOCAL_SELECTED
      │
      ▼
AUTHORIZING_PROVIDER
      │
      ▼
CREATING_PROVIDER_SESSION
      │
      ▼
UPLOADING
      │
      ├── network lost ─────► PAUSED_RESUMABLE
      │                            │
      │                            └── reopen/resume ─► UPLOADING
      │
      ▼
PROVIDER_PROCESSING
      │
      ├── success ───────────► AVAILABLE
      └── failed ────────────► FAILED
```

## Pending media on posts

Post creation may happen while media is:

```text
PendingUpload
```

if product policy permits.

The post then renders a clear placeholder/progress state.
