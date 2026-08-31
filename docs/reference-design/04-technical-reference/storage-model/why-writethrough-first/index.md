# Why writethrough first

Writethrough:

```text
write must reach origin durability path
```

so cache-device loss does not leave the only copy of acknowledged dirty data
on the NVMe cache.

This is safer while validating the system.

## Writeback

Writeback can later reduce HDD write latency:

```text
acknowledge on cache
flush dirty blocks later
```

but cache-device failure with dirty data can cause data loss.

## Only consider writeback after

```text
UPS
tested backups
device-health monitoring
known recovery procedure
acceptable risk
```

and preferably redundant fast storage.
