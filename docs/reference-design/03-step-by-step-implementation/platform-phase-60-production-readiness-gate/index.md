# Production readiness gate

Production is not ready until (architecture doc Phase 60):

```text
[ ] no authoritative JSON persistence
[ ] PostgreSQL restore tested
[ ] DB on verified dm-cache volume
[ ] cache can be destroyed safely
[ ] migrations linted/versioned
[ ] browser PWA update tested
[ ] IndexedDB upgrade tested
[ ] inactive route creates no forum sync
[ ] active-route delta sync repairs missed events
[ ] SignalR groups authorize membership
[ ] duplicate queue delivery is safe
[ ] BFF/service CancellationToken propagation works
[ ] rate limits protect expensive paths
[ ] Hubble sees/diagnoses denied flows
[ ] Cilium default-deny is enforced
[ ] Argo owns desired state
[ ] secrets absent from Git
[ ] BYOA media does not relay bulk bytes through home server
[ ] external login/account ownership rules tested
[ ] dashboards/alerts exist
[ ] rollback procedure documented
```
