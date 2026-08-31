# Load testing

Test at layers (architecture doc Phase 57).

## Browser/BFF

```text
cached page hit
uncached page
topic read
topic mutation
sync delta
SignalR group event
```

## Service

```text
gRPC latency
pool saturation
timeout/cancellation
```

## Cache

```text
hot key
many keys
stampede
Dragonfly fail/restart
```

## Queue

```text
worker slow
queue backlog
redelivery
dead-letter
```

## Database

```text
read-heavy forum
write bursts
index maintenance
cold cache
```

Do not claim "supports 10,000 users" from one endpoint benchmark.

Define concurrent behavior mix.
