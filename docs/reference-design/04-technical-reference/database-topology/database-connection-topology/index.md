# Database connection topology

Application Pods should normally connect through PgBouncer managed by
CloudNativePG:

```text
service Pod
    ↓
PgBouncer Pooler Service
    ↓
CloudNativePG rw Service
    ↓
PostgreSQL primary
```

## Reason

```text
many Kubernetes Pods
many request bursts
limited PostgreSQL backend process budget
```

## Pooling mode

Use transaction pooling unless a feature specifically requires session
semantics.

Audit any SQL feature that depends on session state before relying on
transaction pooling.

Note the PgBouncer-mode decision rule in the PgBouncer connection pooling
section: start with `session` mode, and only move to transaction/statement
pooling after compatibility testing.
