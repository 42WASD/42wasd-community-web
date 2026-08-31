# F# domain type conventions

Prefer small strong types:

```fsharp
type UserId = UserId of Guid
type PostId = PostId of Guid
type ConversationId = ConversationId of Guid
```

Prefer discriminated unions for closed states:

```fsharp
type MediaStatus =
    | Pending
    | Available
    | Failed of string
    | Removed

type JobState =
    | Accepted
    | Running of progress: int
    | Completed
    | Failed of AppError
```

Avoid universal bags:

```fsharp
type UniversalEntity =
    {
        Type: string
        Properties: Map<string,string>
    }
```

unless you are explicitly modeling truly arbitrary metadata.

## Why

Strong identifiers prevent `deletePost(accountId)` from compiling merely
because every ID is a bare `Guid`. Closed DUs make illegal states
unrepresentable and keep `update`/`update`-shaped logic exhaustive.
