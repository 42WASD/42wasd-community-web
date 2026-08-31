# Argo rollout model for services

Each service repo owns (architecture doc Phase 49):

```text
deploy/k8s/base
deploy/k8s/overlays/staging
deploy/k8s/overlays/production
```

Central `cluster-infra` owns Argo registration.

Use Argo sync waves/hooks carefully for:

```text
migration job before dependent rollout
```

Do not hide database migrations inside every Pod startup.

## Acceptance

```text
[ ] staging auto/reliably syncs
[ ] production promotion policy defined
[ ] rollback does not run irreversible DB downgrade automatically
```
