# Browser route-scope model

Do not represent browser synchronization as:

```text
syncEverything()
```

Represent it as scopes.

## Illustrative

```fsharp
type DataScope =
    | HomeFeed of cursor: string option
    | ForumCategory of categoryId: CategoryId
    | Topic of topicId: TopicId
    | UserProfile of userId: UserId
    | ServerList
    | Tournament of tournamentId: TournamentId
```

At any moment the browser has:

```text
ActiveScope set
```

Example:

```text
/forum/general/topic/123

Active:
  ForumCategory general   optional, only if page needs the list
  Topic 123

Inactive:
  Topic 456
  ForumCategory support
  ServerList
  Tournament 77
```

## The rule

Only active scopes may:

```text
revalidate
delta-sync
subscribe to realtime updates
```

Inactive cached scopes remain untouched.
