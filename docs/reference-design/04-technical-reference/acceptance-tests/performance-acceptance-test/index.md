# Performance acceptance test

Before calling the architecture optimized, benchmark:

```text
cold first route
warm IndexedDB route
warm L1 server cache
warm Dragonfly cache
full Postgres origin
HDD cold-block path
NVMe cache-hit path
```

Record:

```text
latency
CPU
bytes transferred
DB reads
disk IOPS
cache hit ratios
```

Test at:

```text
1 user
100 concurrent users
1,000 simulated clients where hardware permits
10,000 open/mostly-idle connections only after realistic modeling
```

A browser being open must not imply active forum workload.

Simulate realistic route distributions.
