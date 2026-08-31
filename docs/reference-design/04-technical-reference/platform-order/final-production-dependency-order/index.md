# Final production dependency order

The platform should become available in this order:

```text
Linux storage / cached volume
        ↓
Kubernetes Local PV / StorageClass
        ↓
Cilium + Hubble
        ↓
Argo CD platform reconciliation
        ↓
CloudNativePG
        ↓
PostgreSQL cluster + PgBouncer
        ↓
Atlas migrations
        ↓
Dragonfly
        ↓
Dapr control plane
        ↓
RabbitMQ
        ↓
Accounts / Forum / Game / Tournament services
        ↓
workers
        ↓
Community Web BFF
        ↓
Bolero PWA client
```

Not every layer must block static site startup, but durable application
functionality depends on its required lower layers.
