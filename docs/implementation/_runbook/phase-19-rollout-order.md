---
phase: 03-step-by-step-implementation/phase-19-rollout-order
---

**Phase 19 complete — rollout order.** This phase is a plan, not code: it fixes
the release sequence so each vertical slice ships independently and the app is
releasable after every slice, per the phase's rule.

### Reference

```text
- order vertical slices for release
- one working slice at a time
```

Rule: *the app is releasable after each vertical slice, not only at the end.*

### The adopted slice order

1. **S1 Infrastructure** — `Dockerfile`, `.dockerignore`, GHCR workflow,
   `deploy/k8s/*` (from Phase 18). Cluster can run a healthy instance.
2. **S2 Public data (read-only)** — all read-only pages render from the baked
   `data/` JSON: Games, Servers, Teams, Members, Tournaments, Home, About.
3. **S3 Auth** — `/api/getUsername`, sign-in/out gating for Members/Account.
   Security first, before public writes.
4. **S4 Writes** — cross-feature effects (tournament toggle, favourites) go live.
5. **S5 Hardening** — shared data-protection for auth cookies if scaling
   `> 1` replica, request-logging/metrics, monitoring.

### Safety rules in practice

- **Security before public data:** auth (S3) lands before any write (S4).
- **Read-only first:** S2 ships without write paths.
- **Verification gates every slice:** each slice is only "done" when
  `bash scripts/docs/verify.sh` → `VERIFY OK` and `dotnet test` passes.

### Scaling note (S5)

The app uses ASP.NET Core cookie auth + Blazor SignalR. At `replicas: 1` (the
Phase 18 Deployment) no extra work is needed. To scale out later: configure a
shared `IDataProtection` key ring (e.g. persisted to Redis/disk) so auth
cookies decrypt on any pod, and enable sticky sessions on the Service/Ingress
for SignalR.

### Verification

Releasable at every slice — the gate is always the same:

```bash
bash scripts/docs/verify.sh   # VERIFY OK
dotnet test                   # all pass
```

### Files changed

```
docs/reference-design/.../phase-19-rollout-order/index.md
docs/implementation/_runbook/phase-19-rollout-order.md   (this file)
docs/implementation/progress.yaml
docs/implementation/index.md
```

`verify.sh` reports `VERIFY OK`.