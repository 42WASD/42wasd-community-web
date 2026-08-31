# Build the HDD + NVMe dm-cache volume

> **Destructive operation warning:** do not copy device placeholders directly.
> Verify disks using `lsblk`, device serials, filesystem mounts, SMART/NVMe
> health, and backups. A wrong device path can destroy data (architecture doc
> Phase 6).

## Desired host layout

```text
slow PV:
    ~1 TB HDD capacity

fast PV:
    ~30 GB NVMe capacity dedicated to DB cache

LVM VG:
    contains/has access to slow and fast PVs

origin LV:
    allocated from HDD

fast cache LV/cachepool:
    allocated from NVMe

cached LV:
    dm-cache, smq, writethrough
```

## Conceptual LVM flow

```text
create/identify slow origin LV
create fast LV on NVMe
attach fast LV using dm-cache
verify cache mode = writethrough
verify cache policy = smq
create filesystem only after block-size compatibility is understood
mount
benchmark
```

Do not tune chunk size/migration threshold immediately.

Start from LVM defaults and measure.

## Record

```text
physical device serials
VG/LV names
filesystem UUID
cache policy
cache mode
mount path
recovery steps
```

## Acceptance

```text
[ ] cached LV survives reboot
[ ] writethrough confirmed
[ ] smq confirmed
[ ] filesystem fsck/check clean
[ ] fio baseline measured on HDD alone and cached LV
[ ] cache hit statistics visible
```
