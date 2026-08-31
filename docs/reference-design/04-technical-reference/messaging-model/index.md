# Messaging model

RabbitMQ topology, interactive-vs-queued decisions, job envelopes, worker
concurrency, batching, and reliability rules.

## Subsections

- **RabbitMQ topology** — exchanges, routing classes, events vs commands.
- **Interactive versus queued work** — semantic execution modes.
- **Job envelope** — typed job messages.
- **Worker concurrency policy** — per-workload-class pools.
- **Batch policy** — collect until N items or T ms.
- **RabbitMQ reliability** — confirms, acks, quorum reality on one node.
