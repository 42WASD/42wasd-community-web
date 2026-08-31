# Why writethrough initially

## Writethrough

```text
write -> cache + origin
ack after safe path
```

Loss of the cache device does not imply loss of acknowledged origin data merely
because it was dirty only on cache.

## Writeback

```text
write -> fast cache
ack
later flush -> HDD
```

can greatly improve HDD write latency but a failed cache containing dirty
blocks can cause data loss.

## Only move to writeback after

```text
UPS
tested shutdown behavior
reliable NVMe
independent DB backup
recovery drill
monitoring of dirty/cache health
explicit acceptance of risk
```

Until all of those are true, writethrough is the initial selected mode.
