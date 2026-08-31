# Streamed page-fragment operation

Use a single stream when:

```text
the server owns the orchestration
the fragments are naturally one workflow
the client benefits from one cancellation boundary
ordering is useful
```

## Example union

```fsharp
type TopicPageUpdate =
    | CoreLoaded of TopicDto
    | CommentsLoaded of CommentPageDto
    | ReactionSummaryLoaded of ReactionSummaryDto
    | GameContextLoaded of GameContextDto
    | SourceFailed of source: string * error: AppError
```

SignalR streaming can emit each result.

## Default remains

```text
parallel independent queries
```

because it isolates failures and caching better.
