# Database physical storage contract

Selected physical layout:

```text
1 TB HDD = capacity/origin
30 GB NVMe = LVM dm-cache cache tier
```

Linux exposes:

```text
one cached logical block device
```

Filesystem lives on it.

Kubernetes Local PV points to it.

CloudNativePG PVC uses that StorageClass.

PostgreSQL sees ordinary durable filesystem storage.

It does not know which block is currently on NVMe.
