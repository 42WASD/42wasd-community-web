# Extract the Game/Server Service

Owns (architecture doc Phase 35):

```text
server catalog
integration endpoints
current server status aggregation
provider-specific clients
cached live status
```

For internal K8s services:

```text
Dapr service invocation / gRPC
```

For external platforms:

```text
HttpClientFactory in service
provider-specific timeout/retry/rate limit
```

Cache fast-changing status for a short TTL if many users request the same
state.

## Acceptance

```text
[ ] 1,000 same-status reads do not become 1,000 external calls
[ ] external outage does not block unrelated forum content
```
