# RabbitMQ reliability rules

For important accepted work:

```text
durable queue
manual consumer acknowledgement
publisher confirms
bounded prefetch
idempotent consumer
dead-letter path
retry policy with max attempts
poison-message handling
```

## Quorum queues

Use quorum queues where replicated message safety is required **and** you
actually have enough RabbitMQ nodes/storage to benefit.

On a single-node homelab, a quorum queue cannot magically provide HA. Its
semantics do not create missing physical redundancy.

## Streams

Use RabbitMQ streams when the problem is an append/replay/high-throughput
event log rather than a normal work queue.

## Dapr defaults warning

The Dapr RabbitMQ component defaults (`durable=false`,
`deletedWhenUnused=true`, `deliveryMode=0`, `prefetchCount=0`,
`publisherConfirm=false`, `enableDeadLetter=false`) are **not** the reliability
policy selected by this architecture — configure explicit metadata for durable
event paths.
