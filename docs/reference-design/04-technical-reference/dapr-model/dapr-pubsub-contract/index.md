# Dapr Pub/Sub contract

Dapr Pub/Sub abstracts broker integration.

Selected broker:

```text
RabbitMQ
```

App publishes:

```text
topic = forum.post-created
```

Dapr component routes to RabbitMQ.

Subscriber declares interest.

## Recommended

```text
topic scopes
dead-letter topics
message TTL only where semantically correct
bulk publish/subscribe only after measurement
```

Dapr uses CloudEvents by default for Pub/Sub messages.

Record this in integration-test fixtures so consumers do not accidentally
depend on undocumented envelope details.
