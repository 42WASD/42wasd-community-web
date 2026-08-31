# SignalR scale-out

## One replica: no backplane

With one BFF replica:

```text
no backplane required
```

## Multiple replicas: Redis-protocol backplane

```text
BFF A ─┐
BFF B ─┼─ Redis-protocol pub/sub backplane
BFF C ─┘
```

ASP.NET Core officially documents Redis as the self-hosted backplane.

Dragonfly is Redis-compatible and has active pub/sub support, but the
production decision must be:

```text
run official SignalR StackExchangeRedis integration tests
run reconnect/group/fanout load test against Dragonfly
```

If anything fails, use a dedicated Valkey/Redis instance for the SignalR
backplane while keeping Dragonfly as application cache.

## The rule

Do not sacrifice realtime correctness merely to use one fewer daemon.
