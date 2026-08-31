# Cilium service dependency graph

Initial logical allow graph:

```text
Internet
   ↓
Ingress / public edge
   ↓
Community Web BFF

Community Web BFF
   ├── Accounts Service
   ├── Forum Service
   ├── Game/Server Service
   ├── Tournament Service
   ├── Dragonfly
   └── DNS

Forum Service
   ├── PgBouncer
   ├── Dapr sidecar/control dependencies
   ├── RabbitMQ through Dapr component path
   └── DNS

Accounts Service
   ├── PgBouncer
   ├── external OAuth providers where required
   └── DNS

Workers
   ├── RabbitMQ/Dapr
   ├── PgBouncer when required
   ├── approved provider APIs
   └── DNS

PostgreSQL
   ← PgBouncer only where architecture permits

Dragonfly
   ← BFF/services that explicitly need cache

RabbitMQ
   ← Dapr/broker clients only
```

Do not turn this drawing directly into policies without checking
Dapr/control-plane traffic.

Use Hubble/Audit Mode to discover platform paths safely.
