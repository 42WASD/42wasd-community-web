# Storage model

The physical storage contract, the LVM dm-cache runbook, the writethrough
rationale, and later explicit hot/cold placement.

## Subsections

- **Database physical storage contract** — HDD origin + NVMe cache tier.
- **LVM dm-cache creation runbook** — verification-first destructive work.
- **Why writethrough first** — cache-device failure safety.
- **PostgreSQL explicit hot/cold placement** — later tablespaces/partitioning.
