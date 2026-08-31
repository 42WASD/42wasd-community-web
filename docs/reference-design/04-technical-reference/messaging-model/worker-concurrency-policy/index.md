# Worker concurrency policy

Do not have one global worker concurrency.

## Example

```text
notification:
  many lightweight workers

external provider:
  concurrency constrained to provider quota

CPU-heavy transform:
  concurrency constrained to CPU budget

database-heavy bulk:
  low concurrency
```

## Each worker class gets

```text
queue
prefetch
concurrency
retry
dead-letter
timeout
```

appropriate to its resource.
