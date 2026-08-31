# End-to-end BYOA video state machine

```text
User selects video
       │
       ▼
browser File object
       │
       ├─ local preview immediately
       ▼
authorize YouTube
       │
       ▼
start resumable upload session
       │
       ▼
save upload metadata in IndexedDB
       │
       ▼
user can create post with Pending media
       │
       ▼
upload chunks directly:
browser -> YouTube
       │
       ├─ network interruption
       │      ↓
       │   keep resumable state
       │
       └─ completion
              ↓
        provider resource ID
              ↓
        update MediaRef
              ↓
        post renders YouTube player
```

Bulk bytes never traverse the 42WASD home server.
