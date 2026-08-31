# RabbitMQ reliability

For durable critical work:

```text
publisher confirms
manual consumer acknowledgements
bounded prefetch
dead-letter handling
idempotent consumer
```

## Quorum queues reality

Quorum queues are appropriate only when RabbitMQ actually has a meaningful
multi-node failure domain.

On a single physical server:

```text
three RabbitMQ Pods on the same machine
```

do not provide machine-level HA.

## Single-node guidance

Start with a topology appropriate to one node and backups. Do not label three
broker Pods on one physical machine as HA. For ordinary work queues on the
single-node homelab, choose queue durability and retention based on recovery
requirements; reserve quorum queues for the point where a real 3-node (or
other odd-sized) RabbitMQ failure domain exists. RabbitMQ documents a default
quorum size of three and notes that quorum queues trade latency for
replicated safety.

Upgrade to true quorum replication when multiple independent nodes/storage
failure domains exist.
