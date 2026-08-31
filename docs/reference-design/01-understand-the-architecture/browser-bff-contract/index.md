# Browser/BFF contract design

Keep explicit DTOs in `Community.Web.Shared`.

Recommended feature organization:

```text
src/Community.Web.Shared/
├── Common/
│   ├── Identifiers.fs
│   ├── Pagination.fs
│   ├── Errors.fs
│   └── Versioning.fs
├── Accounts/
│   └── AccountContracts.fs
├── Forum/
│   └── ForumContracts.fs
├── Servers/
│   └── ServerContracts.fs
├── Tournaments/
│   └── TournamentContracts.fs
├── Notifications/
│   └── NotificationContracts.fs
├── Media/
│   └── MediaContracts.fs
└── Realtime/
    └── RealtimeEvents.fs
```

## The projection boundary

Do not send SQLProvider-generated database rows directly to the browser.

Distinguish:

```text
Persistence row/type
Domain type
Browser DTO
Elmish page model
```

This intentional separation prevents an internal schema change from
accidentally becoming a public/browser contract change.
