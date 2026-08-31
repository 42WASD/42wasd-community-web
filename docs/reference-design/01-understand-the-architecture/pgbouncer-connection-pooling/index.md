# PgBouncer connection pooling

Do **not** deploy PgBouncer merely because it exists. Npgsql already has an
efficient client-side pool, so the first production version should use
**Npgsql pooling only** while the service count and connection budget are still
small and measurable.

Add CloudNativePG `Pooler`/PgBouncer when connection churn or the aggregate
number of service pools justifies a central cap.

## Recommended first mode

```text
microservices
     ↓
PgBouncer SESSION pooling
     ↓
PostgreSQL
```

Why start with `session`:

```text
least semantic surprise
supports normal PostgreSQL session features
safer first fit for SQLProvider
safer first fit for ASP.NET Identity / EF Core
```

## Do not stack two pools blindly

Do not blindly stack two independent pools without defining who owns pooling.
Npgsql documents two valid approaches:

```text
A. PgBouncer owns pooling
   -> Pooling=false in Npgsql

B. Keep Npgsql pooling too
   -> size both layers deliberately
```

If you later change PgBouncer to **transaction** or **statement** pooling while
retaining Npgsql pooling, follow Npgsql's compatibility requirement and set
`No Reset On Close=true`; transaction/statement pooling also disables some
PostgreSQL session-level behavior, so it must pass SQLProvider, Identity,
migration and integration tests before production.

CloudNativePG's `Pooler` examples use `poolMode: session`; treat transaction
pooling as an optimization to prove, not a starting default.

## Connection budget

Define a hard connection budget:

```text
PostgreSQL max_connections
  > PgBouncer backend connections
  > aggregate expected steady-state service demand
```

and alert before saturation.
