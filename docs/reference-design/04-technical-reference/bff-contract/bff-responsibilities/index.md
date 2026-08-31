# BFF responsibilities

`Community.Web.Server` should own:

```text
Blazor/Bolero static hosting
PWA assets
Bolero Remoting
browser authentication/session
CSRF/request-boundary concerns where applicable
browser contract negotiation
browser-friendly DTO aggregation
route-scope synchronization endpoints
SignalR hubs
browser-specific rate limiting
HybridCache L1 for BFF projections
response compression
```

## It should not own

```text
every forum rule
every tournament rule
all notification delivery
all background work
direct Kubernetes orchestration
all persistence logic for every domain
```

The BFF is an adapter/facade.
