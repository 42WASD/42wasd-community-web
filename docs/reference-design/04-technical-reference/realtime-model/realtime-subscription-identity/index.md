# Realtime subscription identity

Use narrow group names:

```text
topic:{topicId}
forum-category:{categoryId}
tournament:{tournamentId}
dm-conversation:{conversationId}
```

## Do not use

```text
all-forum-users
all-connected-clients
```

for ordinary entity updates.

Joining a group is an authorization operation.

Before:

```text
Groups.AddToGroupAsync(connectionId, "dm-conversation:123")
```

verify:

```text
authenticated account belongs to conversation 123
```

Never trust a client-provided group name without authorization.
