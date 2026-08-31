# Baseline and platform preflight

Before changing application architecture, prove that the **actual cluster and
host** can run the selected platform versions (architecture doc Phase 1).
Version compatibility belongs in the implementation gate, not only in the
reference appendix.

## Capture application baseline

```text
current app startup
current page screenshots
current functional tests
current container image
current Kubernetes manifests
current package versions
current JSON data behavior
current auth behavior
```

## Capture platform preflight

```bash
kubectl version
cilium version
cilium status
argocd version
uname -r
lsblk -o NAME,SIZE,TYPE,FSTYPE,MOUNTPOINTS
pvs
vgs
lvs
lvm version
```

Record the Kubernetes server minor version explicitly. The selected
CloudNativePG **1.30.x** line must support the cluster minor you actually run.
If the cluster is outside that operator release's supported Kubernetes range,
do **not** force-install it; either upgrade Kubernetes first or select the
CloudNativePG release that officially supports the real cluster.

Also record:

```text
NVMe device/path and free capacity
HDD device/path and free capacity
filesystem/block topology
existing LVM ownership
current backup destination
current Cilium policy-enforcement state
```

Add/verify tests for current critical flows before migrating persistence.

## Acceptance

```text
[ ] current main branch builds
[ ] current test suite passes
[ ] current production/staging rollback image is known
[ ] current data files are backed up
[ ] Kubernetes minor is inside selected operator support matrix
[ ] Cilium is healthy before new policies are introduced
[ ] exact NVMe/HDD devices are recorded before any LVM command is run
[ ] LVM/cache setup has a rollback/recovery plan before destructive storage work
```
