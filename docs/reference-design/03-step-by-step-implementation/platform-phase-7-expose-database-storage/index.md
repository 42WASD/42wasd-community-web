# Expose database storage to Kubernetes

Because storage is local to the home server, use a local persistent storage
method appropriate for your cluster (architecture doc Phase 7).

## Simplest conceptual model

```text
cached LV filesystem/mount
     ↓
local PersistentVolume
     ↓
StorageClass / PVC binding
     ↓
CloudNativePG
```

Use node affinity so PostgreSQL is scheduled only to the node that physically
owns the volume. For a pre-created Kubernetes `local` PV, use a StorageClass
with `volumeBindingMode: WaitForFirstConsumer` so the scheduler can consider
the Pod's topology before binding the claim.

## Illustrative StorageClass and PV

Replace the hostname, path and capacity with verified values:

```yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: community-postgres-cached
provisioner: kubernetes.io/no-provisioner
volumeBindingMode: WaitForFirstConsumer
reclaimPolicy: Retain
---
apiVersion: v1
kind: PersistentVolume
metadata:
  name: community-postgres-local-pv
spec:
  capacity:
    storage: 900Gi
  volumeMode: Filesystem
  accessModes:
    - ReadWriteOnce
  persistentVolumeReclaimPolicy: Retain
  storageClassName: community-postgres-cached
  local:
    path: /var/lib/42wasd/postgres-cached
  nodeAffinity:
    required:
      nodeSelectorTerms:
        - matchExpressions:
            - key: kubernetes.io/hostname
              operator: In
              values:
                - REPLACE_WITH_ACTUAL_NODE_NAME
```

Do not use `hostPath` as a substitute for a proper `local`
PersistentVolume, and do not use `nodeName` to force the database Pod when
relying on `WaitForFirstConsumer`; let the scheduler honor PV node affinity.

Do not advertise local disk as portable storage.

## Acceptance

```text
[ ] Pod cannot accidentally schedule to node without disk
[ ] PVC remains Bound across Pod restart
[ ] write/read survives Pod recreation
```
