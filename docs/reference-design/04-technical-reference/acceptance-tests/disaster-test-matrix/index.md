# Disaster test matrix

Test intentionally:

```text
kill BFF Pod
kill Forum Service Pod
restart Dragonfly
restart RabbitMQ
restart PostgreSQL Pod
reboot Kubernetes node
block one service path with Cilium
fill test filesystem near threshold
interrupt DB backup
interrupt YouTube upload
disconnect browser mid-sync batch
close browser with pending draft/outbox/upload
```

Expected results must be documented before production.
