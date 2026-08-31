# CloudNativePG Pooler baseline

## Illustrative

```yaml
apiVersion: postgresql.cnpg.io/v1
kind: Pooler
metadata:
  name: community-db-rw-pool
  namespace: data
spec:
  cluster:
    name: community-db

  instances: 1
  type: rw

  pgbouncer:
    poolMode: transaction
    parameters:
      max_client_conn: "1000"
      default_pool_size: "40"
```

Values are starting examples only.

Load test and tune them.

CloudNativePG's `Pooler` CRD exists specifically to manage PgBouncer as the
access layer.

See the PgBouncer connection pooling section for the pooling-mode decision
rule (start with `session`).
