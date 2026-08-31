# Deploy RabbitMQ

Central infra deploys pinned RabbitMQ 4.2.x (architecture doc Phase 38).

On single-node cluster, start simple.

Create:

```text
pubsub exchange/topics
dead-letter exchange
work queues
```

Do not claim HA without 3 real broker nodes/failure domains.

## Acceptance

```text
[ ] publisher confirms enabled for important flows
[ ] manual consumer acks
[ ] redelivery test
[ ] dead-letter test
[ ] max queue age alert
```
