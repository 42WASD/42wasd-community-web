# Architecture vocabulary freeze

Before changing code, agree on stable identifiers (architecture doc Phase 0).

## Suggested Kubernetes namespaces

```text
community
community-data
community-messaging
community-platform
```

Or keep fewer namespaces initially if your existing cluster convention prefers
it.

## Suggested service IDs

```text
community-web
accounts
forum
game-server
tournament
notifications
```

Dapr app IDs use the same logical names.

## Suggested database

```text
community
```

## Suggested proto packages

```text
community.accounts.v1
community.forum.v1
community.servers.v1
community.tournaments.v1
community.notifications.v1
```

## Acceptance

```text
[ ] names are documented
[ ] names are not user-facing display strings
[ ] namespaces follow existing cluster convention
[ ] Dapr app IDs are stable
```
