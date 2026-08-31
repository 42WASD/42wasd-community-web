# Add SignalR for one topic only

Do not begin with global forum realtime (architecture doc Phase 24).

First prove:

```text
topic:123
```

Lifecycle:

```text
route enter
connect lazily
join group
receive event
route leave
leave group
close connection after grace if no groups
```

Use MessagePack.

## Acceptance

```text
[ ] only subscribed topic receives event
[ ] user on profile receives none
[ ] reconnect rejoins active group
[ ] missed sequence repairs through delta sync
```
