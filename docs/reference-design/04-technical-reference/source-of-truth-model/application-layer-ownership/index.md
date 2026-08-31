# Application-layer ownership

Use bounded contexts rather than one global application model.

## Recommended first boundaries

```text
Accounts
Forum
GameServers
Tournaments
Notifications
```

## Possible future boundaries after measurement

```text
Search
Moderation
Recommendations
Analytics
Chat
```

Do not create a microservice merely because a source folder exists.

A service boundary should normally have:

```text
its own business responsibility
its own API contract
a clear owner
independent scaling/release value
limited synchronous dependencies
```

## Avoid

```text
PostService
CommentCountService
UsernameService
AvatarService
PermissionBooleanService
```

as separate network services.

That creates a distributed monolith.
