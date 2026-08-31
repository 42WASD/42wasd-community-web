# Observability

Instrument every service with OpenTelemetry.

## Required correlation fields

```text
trace_id
span_id
request_id
authenticated_account_id where safe
service
operation
deployment/version
```

## Never log

```text
password
refresh token
OAuth access token
session secret
full sensitive request bodies
```

## Track

```text
BFF request latency p50/p95/p99
DB query latency
cache hit L1/L2 ratio
cache origin load
gRPC latency/error/deadline
Dapr invocation latency
queue depth
queue age
worker processing time
workflow duration
SignalR active connections
SignalR active groups/subscriptions
delta-sync bytes
full-bootstrap bytes
IndexedDB cache hit rate (client telemetry sampled)
response compressed/uncompressed bytes
PostgreSQL buffer/cache/disk metrics
dm-cache hit/miss/promotion
HDD latency/queue depth
Hubble drops/policy verdicts
```

## The most important UX metric

```text
time from route activation
to first useful content rendered
```

Measure separately:

```text
IndexedDB cache hit
BFF L1 cache hit
Dragonfly L2 hit
PostgreSQL origin
external integration required
```
