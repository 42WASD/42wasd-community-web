# Configure Dapr Pub/Sub over RabbitMQ with explicit reliability settings

Create the Dapr component centrally or namespace-scoped according to the
secret/ownership model (architecture doc Phase 39).

Do **not** rely on the RabbitMQ component defaults for critical durable events.
Current Dapr documentation lists defaults including `durable=false`,
`deletedWhenUnused=true`, `deliveryMode=0`, `prefetchCount=0` (all available
messages), `publisherConfirm=false`, and `enableDeadLetter=false`. Those
defaults are not the reliability policy selected by this architecture.

## Illustrative baseline

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: community-pubsub
  namespace: community
spec:
  type: pubsub.rabbitmq
  version: v1
  metadata:
    - name: host
      secretKeyRef:
        name: rabbitmq-credentials
        key: connectionString
    - name: durable
      value: "true"
    - name: deletedWhenUnused
      value: "false"
    - name: autoAck
      value: "false"
    - name: deliveryMode
      value: "2"
    - name: publisherConfirm
      value: "true"
    - name: prefetchCount
      # Initial safe bounded value only; tune from consumer latency/load tests.
      value: "50"
    - name: concurrencyMode
      value: "parallel"
    - name: enableDeadLetter
      value: "true"
```

Also restrict which apps may consume/use the component with Dapr component
scopes or namespace ownership where appropriate. Use a Dapr/Kubernetes secret
store rather than plaintext credentials.

`prefetchCount=50` is an **initial benchmark value**, not a universal optimum.
Tune it together with app concurrency, processing duration, memory, and
RabbitMQ redelivery behavior.

Applications publish logical topics rather than RabbitMQ-specific code where
portability is desired.

Keep event contracts versioned.

## Acceptance

```text
[ ] Forum publishes with publisher confirms enabled
[ ] Notification consumes with manual acknowledgement behavior
[ ] broker restart/redelivery test succeeds
[ ] duplicate delivery tolerated by inbox/idempotency logic
[ ] poison message reaches dead-letter path according to policy
[ ] bounded prefetch verified under load
[ ] broker credentials not in source
```
