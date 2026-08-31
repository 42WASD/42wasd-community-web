# Internal `.proto` repository layout

Recommended inside the application repository or dedicated contract repository
if independently released later:

```text
proto/
  common/
    identifiers.proto
    pagination.proto
    errors.proto

  accounts/v1/
    accounts.proto

  forum/v1/
    forum.proto

  game_servers/v1/
    game_servers.proto

  tournaments/v1/
    tournaments.proto

  notifications/v1/
    notifications.proto
```

Use package names such as:

```protobuf
package community.forum.v1;
```

## Do not use unversioned

```protobuf
package forum;
```

for contracts expected to survive many releases.
