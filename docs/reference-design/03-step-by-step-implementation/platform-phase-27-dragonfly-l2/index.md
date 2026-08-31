# Deploy Dragonfly L2

Central platform deploys pinned Dragonfly (architecture doc Phase 27).

Keep private ClusterIP.

Configure `IDistributedCache`/compatible adapter used by HybridCache.

## Acceptance

```text
[ ] BFF Pod A populates L2
[ ] BFF Pod B can reuse L2
[ ] Dragonfly restart causes performance degradation, not data loss
[ ] memory limit/eviction policy tested
```
