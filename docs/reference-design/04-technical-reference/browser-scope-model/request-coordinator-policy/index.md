# RequestCoordinator policy

Model each in-flight query as:

```text
QueryEntry
  key
  state
  startedAt
  cancellationSource
  subscriberCount
  priority
  estimatedCost
  cachePolicy
```

## Suggested policies

```text
INTERACTIVE_VISIBLE
  high priority
  cancel only when no subscribers and still expensive

PREFETCH
  low priority
  cancel aggressively when scope changes

CHEAP_CACHEABLE
  if almost finished and subscribers become zero:
      allow completion and cache

EXPENSIVE_UNIQUE
  if subscriber count becomes zero:
      cancel

MUTATION
  not represented as a shareable read query
```

Do not let DotNetQuery or any third-party package define product semantics.

## Wrap it

```fsharp
type IQueryCoordinator =
    abstract Observe<'T> :
        key: QueryKey *
        policy: QueryPolicy *
        loader: (CancellationToken -> Task<'T>)
        -> IObservable<QueryState<'T>>
```

DotNetQuery can implement this interface.
