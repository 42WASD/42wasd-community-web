# Storage performance validation

Generate a realistic DB working set (architecture doc Phase 55).

Measure:

```text
cold HDD
warm dm-cache
PostgreSQL RAM warm
```

Do not benchmark only tiny data that fits entirely in RAM.

Record:

```text
read p95
write p95
TPS
cache hit rate
HDD queue depth
NVMe wear/health
```

## Acceptance

```text
[ ] hot working set benefits from NVMe
[ ] sequential backup traffic does not destroy cache usefulness excessively
```
