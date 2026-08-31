# Functional Core / Imperative Shell contract

Use this shape:

```text
IMPURE
load required state
      ↓
PURE
validate / decide / transform
      ↓
IMPURE
commit durable state
      ↓
IMPURE
publish/notify
```

## Example

```fsharp
type EditPostError =
    | PostNotFound
    | Forbidden
    | VersionConflict
    | InvalidTitle

type EditDecision =
    {
        UpdatedPost: Post
        Event: PostEdited
    }

let decideEdit actor expectedVersion newTitle post =
    if not (Permissions.canEdit actor post) then
        Error Forbidden
    elif post.Version <> expectedVersion then
        Error VersionConflict
    elif String.IsNullOrWhiteSpace newTitle then
        Error InvalidTitle
    else
        let updated =
            { post with
                Title = newTitle
                Version = post.Version + 1L }

        Ok {
            UpdatedPost = updated
            Event = PostEdited(updated.Id, updated.Version)
        }
```

The database adapter then persists the result.

This pure function can be tested without:

```text
PostgreSQL
network
clock
Redis
Kubernetes
```
