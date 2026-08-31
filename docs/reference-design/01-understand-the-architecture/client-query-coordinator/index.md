# Client query coordinator

The browser should have a query coordinator whose unit of ownership is the
**query key**, not the component instance.

Example key:

```text
forum-topic:123
```

Consumers:

```text
TopicPage
Sidebar
Prefetcher
```

One in-flight fetch:

```text
TopicPage ─┐
Sidebar   ─┼── QueryCoordinator ── one server request
Prefetcher─┘
```

## Subscriber rules

```text
first subscriber
    -> create/start query

additional subscriber
    -> observe/reuse same query

subscriber leaves
    -> decrement subscriber count

count > 0
    -> continue

count = 0
    -> apply request policy
```

Possible zero-subscriber policy:

```text
cheap + almost complete + cacheable -> finish and cache
large expensive unique query        -> cancel
durable command                     -> not owned by coordinator; it is a job
```

## F# abstraction

```fsharp
type QueryKey = QueryKey of string

type QueryState<'T> =
    | Idle
    | Loading of stale: 'T option
    | Ready of 'T
    | Failed of error: AppError * stale: 'T option

type IQueryCoordinator =
    abstract Observe<'T> :
        key: QueryKey *
        loader: (CancellationToken -> Task<'T>)
        -> IObservable<QueryState<'T>>

    abstract Invalidate : QueryKey -> unit
```

Keep Elmish dependent on your abstraction, not on a specific library.

## DotNetQuery usage policy

Prototype `DotNetQuery.Core` / DI integration because current DotNetQuery
provides:

```text
query cache
deduplication by query key
stale-while-revalidate
mutations
invalidation
retry
observable state
```

Override its default "background refetch" behavior wherever it conflicts with
the strict active-route synchronization rule.

**The route lifecycle, not a timer, decides whether a domain scope may
revalidate.**
