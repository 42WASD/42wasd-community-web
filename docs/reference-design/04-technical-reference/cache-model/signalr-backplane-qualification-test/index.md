# SignalR backplane qualification test

Before using Dragonfly for SignalR scale-out:

```text
1. Run at least two BFF replicas.
2. Connect clients to different replicas.
3. Join same authorized SignalR group.
4. Publish through replica A.
5. Verify client connected to replica B receives it.
6. Restart Dragonfly.
7. Verify SignalR reconnect/recovery behavior.
8. Load-test pub/sub churn.
```

## If Redis compatibility is insufficient

If the exact StackExchange.Redis behavior is not satisfied:

```text
deploy a small dedicated Valkey/Redis backplane
```

and keep Dragonfly as HybridCache L2.

Do not couple the whole architecture to an untested compatibility assumption.
