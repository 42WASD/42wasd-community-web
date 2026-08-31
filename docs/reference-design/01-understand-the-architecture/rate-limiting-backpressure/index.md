# Rate limiting and backpressure

ASP.NET Core supports:

```text
fixed window
sliding window
token bucket
concurrency limit
```

## Partitioning

Partition by:

```text
authenticated account
IP where appropriate
API/provider key
endpoint class
```

## Suggested policy classes

```text
cheap read
expensive search
posting/comment mutation
login/auth
external provider proxy
job submission
SignalR connection
```

## The multi-replica scaling rule

```text
ASP.NET Core in-process limiter
    = protects one application replica

multiple BFF replicas
    != one automatically shared cluster-wide quota
```

Therefore use the built-in limiter as a **per-replica protection layer**. If
the product needs a strict account-wide/global quota after the BFF scales
horizontally, put that quota in a shared enforcement point such as the
ingress/API edge or a purpose-built distributed counter/limiter backed by
Dragonfly. Do not assume two Pods share limiter counters merely because they
use the same policy names.

## Backpressure

Queue depth is part of backpressure.

When the system is saturated, reject or delay admission instead of accepting
infinite work.
