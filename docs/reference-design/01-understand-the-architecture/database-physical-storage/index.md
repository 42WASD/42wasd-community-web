# Database physical storage

## Selected architecture

```text
~1 TB HDD-backed LVM origin LV
          +
~30 GB NVMe fast LV
          ↓
LVM dm-cache
policy = smq
mode = writethrough initially
          ↓
filesystem
          ↓
Kubernetes local PV
          ↓
CloudNativePG PostgreSQL PVC
```

LVM automatically tracks hot spots and promotes commonly used blocks into the
fast layer.

## Why block-level, not tablespaces

PostgreSQL tablespaces can explicitly place known objects on different disks.

They do not automatically behave like:

```text
frequently accessed arbitrary disk block -> NVMe
cold block -> HDD
```

For the specified ~30 GB NVMe + ~1 TB HDD requirement, use a block-level
hot-spot cache such as **LVM dm-cache** below the filesystem/PV. PostgreSQL
sees one logical volume.

## The capacity rule

The 30 GB NVMe is an acceleration tier, not the database capacity limit. The
1 TB HDD remains durable capacity.
