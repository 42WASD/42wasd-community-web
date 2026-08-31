# Async work architecture

Queue by workload semantics, not browser connection.

## Suggested topics/queues

```text
notifications
external-api-refresh
bulk-import
search-index
moderation-jobs
analytics-events
```

## Work classes

```text
interactive async
normal
bulk
external-provider-limited
```

Give each worker pool independent concurrency.

## Query versus durable command

A durable command and a cancellable query have different lifetimes:

```text
QUERY / short command
    -> direct request
    -> cancellation token
    -> no durable queue unless required

ASYNC COMMAND
    -> persist/queue job
    -> return JobId
    -> worker continues independently

MULTI-STEP DURABLE PROCESS
    -> Dapr Workflow
```

Do not put every operation through a queue.
