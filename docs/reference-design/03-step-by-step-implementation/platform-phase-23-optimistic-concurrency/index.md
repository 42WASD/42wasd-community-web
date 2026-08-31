# Add optimistic concurrency

Mutating contracts include (architecture doc Phase 23):

```text
expectedVersion
```

On conflict:

```text
409/typed conflict result
current version metadata
```

Elmish UI:

```text
reload
merge if safe
or ask user
```

## Acceptance

```text
[ ] simultaneous editors do not silently overwrite unexpectedly
```
