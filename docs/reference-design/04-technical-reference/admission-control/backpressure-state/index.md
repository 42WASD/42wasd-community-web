# Backpressure state

Expose internal saturation signals.

## Example

```text
queue depth
DB pool wait
external API throttle
worker utilization
CPU
```

## When thresholds are exceeded

```text
reduce prefetch
reject new expensive background jobs
return Retry-After
degrade optional enrichment
preserve core interactive reads
```

## A healthy system should prefer

```text
"optional server status temporarily unavailable"
```

over:

```text
entire forum becomes unavailable because an enrichment API is slow
```
