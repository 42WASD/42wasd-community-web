# Extract the Forum Service

This is the best first bounded service because it will be the first major
dynamic content domain (architecture doc Phase 33).

## Owns

```text
categories
topics
posts/comments
reactions
subscriptions/watch state
forum authorization/moderation basics
forum change feed
forum outbox events
```

## Does not own

```text
Google OAuth
Minecraft server status
tournament bracket
```

BFF uses gRPC/Dapr service invocation.

## Acceptance

```text
[ ] forum can run/test without Bolero process
[ ] BFF contract remains browser-friendly
[ ] DB ownership documented
```
