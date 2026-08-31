# End-to-end reconnect state machine

```text
browser loses network
       │
       ▼
SignalR disconnected
       │
       ▼
user remains on Topic 123
       │
       ▼
network returns
       │
       ▼
reconnect SignalR
       │
       ▼
read local Topic:123 cursor
       │
       ▼
request deltas AFTER cursor
       │
       ▼
apply transactionally
       │
       ▼
join/confirm topic:123 group
```

Do not resync unrelated IndexedDB entries.
