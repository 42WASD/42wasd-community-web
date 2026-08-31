# Database connection budget

Set an explicit budget (architecture doc Phase 53).

Example — not a universal number:

```text
PostgreSQL max_connections = bounded

PgBouncer pool:
    application-specific pool sizes

each service:
    Npgsql pool max size bounded
```

Calculate:

```text
sum(max service physical connections)
+
migration/admin
+
operator/monitoring
<
database safe budget
```

Do not use defaults independently in 20 services and discover later that
theoretical maximum is thousands of DB connections.
