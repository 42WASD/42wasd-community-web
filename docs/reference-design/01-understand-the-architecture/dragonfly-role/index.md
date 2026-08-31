# Dragonfly role

Dragonfly is for:

```text
distributed L2 cache
hot public projections
expensive integration results with TTL
ephemeral counters where exact durability is not required
possibly SignalR backplane after explicit compatibility/load testing
```

## Not for

```text
primary forum database
only copy of a post
only copy of a user's account
```

## The rule

If Dragonfly is deleted, the platform should recover by warming from
PostgreSQL/external sources. Disposable means disposable.

## Version note

Dragonfly 1.40.x is the verified line (Redis/Memcached API compatibility,
multithreaded shared-nothing design, active pub/sub and tiered-storage work).
Pin a tested 1.40.x patch.
