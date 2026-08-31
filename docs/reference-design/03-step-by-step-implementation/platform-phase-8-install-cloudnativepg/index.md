# Install CloudNativePG

Central infra deploys and pins (architecture doc Phase 8):

```text
CloudNativePG 1.30.x
```

Do not copy the operator source into the app repo.

Verify operator health.

Create database namespace and cluster.

## Illustrative — not production-complete

```yaml
apiVersion: postgresql.cnpg.io/v1
kind: Cluster
metadata:
  name: community-postgres
  namespace: community-data
spec:
  instances: 1

  # Example only. Prefer a verified CloudNativePG ClusterImageCatalog or
  # pin an exact supported standard image by digest. Do not use the
  # deprecated rolling `:18` system-image tag in production.
  imageName: ghcr.io/cloudnative-pg/postgresql:18.4-standard-trixie@sha256:REPLACE_WITH_TESTED_DIGEST

  storage:
    storageClass: community-postgres-cached
    size: 900Gi
```

Pin exact tested image/operator versions according to CloudNativePG guidance
rather than relying on this illustrative tag. Current CloudNativePG image
guidance deprecates the old rolling `MM`/`MM.mm` system tags; prefer
`minimal`/`standard` images with explicit distro/version and digest, or a
project-maintained `ClusterImageCatalog`. If using Barman Cloud backups, pair
`standard`/`minimal` images with the Barman Cloud CNPG-I plugin rather than
building new deployments around deprecated in-tree Barman support.

## Acceptance

```text
[ ] PostgreSQL starts
[ ] PVC is on cached local volume
[ ] service endpoint is private
[ ] Prometheus-compatible metrics available
[ ] restart retains data
```
