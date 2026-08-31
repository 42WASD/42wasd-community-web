# Realtime contract

## SignalR is lazy

Do not necessarily keep a permanent realtime socket for every open home page.

Suggested policy:

```text
first route requiring realtime
    -> establish SignalR

join one or more active groups

no realtime groups remain
    -> start short idle grace timer

still zero
    -> close connection
```

This keeps 10,000 idle browsers from automatically becoming 10,000 active
forum synchronization sessions.

## Group examples

```text
forum:list:general:{filterHash}
forum:topic:{topicId}
conversation:{conversationId}
tournament:{tournamentId}
```

Avoid broadcasting every forum event to `Clients.All`.

## Tiny event policy

For small changes:

```fsharp
type ForumRealtimeEvent =
    | PostCreated of PostSummary
    | PostChanged of postId: PostId * version: int64
    | PostDeleted of postId: PostId * version: int64
    | ReactionCountsChanged of postId: PostId * version: int64 * counts: ReactionCounts
```

Use inline deltas when cheaper than another fetch.

Use invalidation/version-only messages when the changed object is large.

## Missed event repair

Every event includes enough revision/sequence information for the client to
detect a gap.

Gap:

```text
client last sequence = 120
receives sequence 123
```

Action:

```text
do not guess
run active-scope delta synchronization from 120
```

## The rule

SignalR is a low-latency **notification** path, not a durability or
synchronization guarantee. A browser can sleep, lose network, close the laptop,
suspend the tab, miss events, and reconnect to another Pod. Therefore:

```text
SignalR           = "something changed quickly"
cursor/change feed = "what did I miss?"
PostgreSQL        = "what is true?"
```
