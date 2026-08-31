# Turn Community.Web.Server into an explicit BFF

Restructure (architecture doc Phase 15):

```text
Community.Web.Server/
├── Startup/Program
├── Remoting/
├── Realtime/
├── Auth/
├── Aggregation/
├── Caching/
└── Integrations/
```

Move domain rules away from `CommunityApiService`.

`CommunityApiService` becomes a thin inbound adapter.

Example:

```text
Bolero remote call
    ↓
ForumBffHandler
    ↓
Forum client/application port
```

## Acceptance

```text
[ ] BFF business rules are minimal
[ ] services/ports can be unit tested without Bolero
[ ] browser contracts remain unchanged through refactor
```
