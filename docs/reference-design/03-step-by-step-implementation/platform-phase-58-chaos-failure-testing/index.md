# Chaos/failure testing

Staging tests (architecture doc Phase 58):

```text
kill BFF Pod
kill Forum Service Pod
restart Dragonfly
restart RabbitMQ
temporarily block service with Cilium
restart Postgres Pod
break external provider
disconnect browser SignalR
interrupt YouTube upload
```

Expected behavior must match the failure-mode section.
