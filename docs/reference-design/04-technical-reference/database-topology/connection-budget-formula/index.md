# Connection-budget formula

Define a real budget.

## Example conceptual budget

```text
PostgreSQL safe application backend target:
  100 active DB sessions

reserve:
  admin/maintenance = 10
  migrations = 5
  monitoring = 5

remaining app capacity:
  80
```

PgBouncer can accept more client connections while multiplexing them onto a
bounded backend pool.

## Do not set

```text
every Pod max pool = 100
```

and deploy 20 Pods.

That accidentally creates:

```text
2,000 possible backend sessions
```

against a small server.
