# CloudNativePG

Selected Kubernetes database operator:

```text
CloudNativePG 1.30.x
PostgreSQL 18.x
```

Use it for:

```text
Postgres instance lifecycle
services
TLS/cert lifecycle
managed roles
PgBouncer Pooler resource
monitoring metrics
controlled upgrades
backup/recovery integration
```

## Single-node reality

A single-node home server still has a single physical failure domain.

Running `instances: 3` on one physical node does not create meaningful
hardware HA and can triple disk/memory overhead.

Initial home setup:

```text
instances: 1
```

with good backups and recovery tests.

Scale to true replicas only after multiple independent nodes/storage failure
domains exist.

## Image pinning

Do not use the deprecated rolling `:18` system-image tag. Pin an explicit
supported image/digest or a verified `ClusterImageCatalog`.

## Backup path

New deployments should prefer the Barman Cloud CNPG-I plugin instead of
deprecated in-tree Barman Cloud configuration.
