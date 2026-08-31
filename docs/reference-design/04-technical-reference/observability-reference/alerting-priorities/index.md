# Alerting priorities

Page only for actionable user-impacting conditions.

## High priority

```text
PostgreSQL unavailable
filesystem nearly full
backup repeatedly failing
RabbitMQ unavailable with critical backlog
all BFF replicas unavailable
Cilium policy blocks known critical path
OAuth/login broadly broken
```

## Lower priority/dashboard

```text
one external enrichment provider slow
cache hit ratio changed
one optional media preview unavailable
```

## The rule

Avoid paging for normal cache eviction.
