# Backup architecture

Database backup is not the same problem as user-media storage.

BYOA rule:

```text
42WASD does not own users' uploaded media bytes
```

does **not** mean:

```text
42WASD should have no database backups
```

The database contains irreplaceable:

```text
accounts
forum text
relationships
tournaments
permissions
```

Backups must exist outside the primary cached HDD.

## Minimum acceptable

```text
primary:
  local PostgreSQL storage

backup:
  second independent physical disk OR remote machine/provider
```

A backup stored only on the same HDD:

```text
is not disaster recovery
```

because one disk failure destroys both.
