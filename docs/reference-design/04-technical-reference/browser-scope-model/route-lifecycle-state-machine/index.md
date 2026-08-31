# Route lifecycle state machine

Every scope follows:

```text
INACTIVE
   │ route enters
   ▼
LOCAL_LOADING
   │
   ├── local cache hit ─────► RENDER_STALE
   │
   └── miss ────────────────► NETWORK_LOADING
                                   │
                                   ▼
                              RENDER_FRESH

RENDER_STALE
   │ foreground-triggered revalidation
   ▼
REVALIDATING
   │
   ├── unchanged ───────────► ACTIVE
   └── changes ─────────────► APPLY_DELTA -> ACTIVE

ACTIVE
   │
   ├── SignalR relevant event -> APPLY_DELTA/INVALIDATE
   │
   ├── reconnect ------------> REVALIDATING
   │
   └── route leaves ---------> UNSUBSCRIBE -> INACTIVE
```

## Hard rule

```text
INACTIVE never transitions to REVALIDATING merely because time passed.
```

No global background refetch interval.
