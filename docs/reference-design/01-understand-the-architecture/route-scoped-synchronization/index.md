# Route-scoped synchronization contract

This rule is central enough to define formally. A browser cache is a
convenience, **not** a continuously synchronized replica.

## Scope examples

```text
ForumCategoryList("general", filterHash)
ForumTopic(PostId)
UserProfile(UserId)
Tournament(TournamentId)
Conversation(ConversationId)
```

Each scope has:

```text
cache key
local last-known revision/cursor
fetch function
realtime group name if needed
activation/deactivation lifecycle
```

## Inactive scope

If the user is not viewing a scope:

```text
no periodic refresh
no delta fetch
no SignalR group
no cache revalidation
```

Cached data can remain in IndexedDB until eviction policy removes it.

## Scope activation

Example: `/forum/topic/123`.

```text
route entered
    ↓
read cached projection from IndexedDB
    ↓
if present -> render immediately as stale/local
    ↓
start ONE active-scope revalidation/sync
    ↓
fetch changes since scope cursor or fetch current projection
    ↓
commit IndexedDB changes + cursor atomically
    ↓
update Elmish
    ↓
join realtime group topic:123
```

This is **foreground-triggered SWR**.

## Scope deactivation

```text
route leaves
    ↓
unsubscribe topic:123
    ↓
remove UI subscriber from QueryCoordinator
    ↓
cancel only if request policy says it is no longer useful
    ↓
keep completed cache locally
```

## First-ever load

Do not fetch an entire category history.

Use cursor/keyset pagination.

Example:

```sql
SELECT ...
FROM topic_summary
WHERE category_id = $category
ORDER BY bumped_at DESC, id DESC
LIMIT 30
```

Return:

```text
items
nextCursor
scopeRevision
```

While the user reads, optionally prefetch the next small page if
network/behavior heuristics say it is useful.

## Entity version

A mutable entity/projection should carry a revision/version.

```fsharp
type PostDto =
    {
        Id: PostId
        Version: int64
        Title: string
        Body: string
        UpdatedAt: DateTimeOffset
    }
```

## Deletion tombstone

Change feed:

```fsharp
type ForumChange =
    | UpsertPost of PostDto
    | DeletePost of postId: PostId * version: int64
```

Without tombstones the client cannot distinguish "not returned" from
"deleted."

## Cursor commit rule

Bad:

```text
receive changes through cursor 900
save cursor 900
then write entities
browser crashes halfway
```

Good:

```text
IndexedDB transaction
    apply all received changes
    store cursor 900
COMMIT
```

Cursor advances only with the data.

## Optimistic concurrency

Client sends:

```text
postId = 123
expectedVersion = 7
newTitle = ...
```

Server has version 9:

```text
return Conflict(currentVersion=9)
```

Then the client can reload/merge/show conflict.

Do not use long-held database locks for normal forum editing.
