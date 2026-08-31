# LVM dm-cache creation runbook

Because exact device paths are machine-specific and destructive, the
production runbook must begin:

```text
1. stop workloads that might touch target disks
2. verify backups
3. run lsblk with model/serial/size
4. map exact HDD device
5. map exact NVMe cache device/partition
6. record serial numbers in operations documentation
7. confirm neither contains irreplaceable data
```

Then implement this logical sequence:

```text
HDD physical volume
       ↓
volume group
       ↓
large origin logical volume

NVMe physical volume/cache area
       ↓
cache data + cache metadata
       ↓
attach as LVM cache pool

cache policy:
  smq

initial cache mode:
  writethrough
```

Do not copy a destructive command from a generic document without substituting
and re-verifying the actual devices.

## Acceptance

```text
[ ] LVM reports LV as cached
[ ] policy = smq
[ ] mode = writethrough
[ ] filesystem survives reboot
[ ] Local PV path stable
[ ] fio baseline recorded
[ ] cache hit statistics observable
```
