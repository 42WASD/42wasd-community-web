# CloudNativePG baseline Cluster

## Illustrative shape

```yaml
apiVersion: postgresql.cnpg.io/v1
kind: Cluster
metadata:
  name: community-db
  namespace: data
spec:
  instances: 1

  imageName: ghcr.io/cloudnative-pg/postgresql:18.4-standard-trixie@sha256:REPLACE_WITH_TESTED_DIGEST

  storage:
    size: 900Gi
    storageClass: postgres-cached-local

  monitoring:
    enablePodMonitor: true

  postgresql:
    parameters:
      shared_buffers: "4GB"
```

## Important

```text
exact memory parameters must be tuned to actual host RAM
image tag/digest must be pinned to a tested release
single physical node means instances:1 initially
```

Do not create three database replicas on one failing disk and call it high
availability.
