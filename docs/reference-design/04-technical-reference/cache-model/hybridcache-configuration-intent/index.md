# HybridCache configuration intent

Use:

```text
L1:
  small
  short-lived
  per-process

L2 Dragonfly:
  larger
  shared
  slightly longer TTL
```

## Example

```text
L1 public topic projection:
  10–30 seconds

L2:
  1–5 minutes

event-driven invalidation:
  immediately on known mutation
```

These are starting ranges, not universal values.

The more reliable the event-driven invalidation is, the longer L2 can
sometimes be.
