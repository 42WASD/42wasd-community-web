# Database backup policy

## Database backup is not optional

The BYOA rule applies to user media.

It does **not** mean the platform may omit backups of its authoritative
PostgreSQL database.

A 30 GB cache in front of a 1 TB disk is performance tiering, **not backup**.

## A backup must survive

```text
HDD failure
filesystem corruption
operator mistake
database corruption
node theft/fire
bad migration
```

Therefore keep at least one copy outside the primary disk/node.

## Preferred CloudNativePG path

```text
Barman Cloud CNPG-I plugin
        ↓
physical base backup + continuous WAL archive
        ↓
independent object-storage target
        ↓
PITR-capable restore path
```

CloudNativePG's legacy in-tree `barmanObjectStore` integration is deprecated;
new deployments should use the plugin-based path when choosing Barman Cloud.

## Snapshots caveat

Volume snapshots are useful only if the underlying storage provides
independent snapshot durability; a same-disk local snapshot is not disaster
recovery.

If you refuse any remote object storage, use a physically independent backup
disk/host and a PostgreSQL physical backup mechanism. The architecture cannot
manufacture disaster recovery from one physical disk.
