# Network policy ownership

Central infra (architecture doc Phase 52):

```text
cluster baseline/default-deny
DNS
platform control-plane policies
Hubble/Cilium config
```

App repo:

```text
source workload -> allowed destination dependencies
ingress allowed from BFF/other services
```

## Example conceptual dependency graph

```text
community-web
    -> accounts gRPC
    -> forum gRPC
    -> server gRPC
    -> tournament gRPC
    -> Dragonfly
    -> SignalR backplane if direct
    -> DNS

forum
    -> PostgreSQL/PgBouncer
    -> Dapr sidecar
    -> DNS

notification
    -> PostgreSQL/PgBouncer
    -> Dapr/RabbitMQ path
```

Use Dapr architecture to determine whether application Pods contact broker
directly or only their sidecars.
