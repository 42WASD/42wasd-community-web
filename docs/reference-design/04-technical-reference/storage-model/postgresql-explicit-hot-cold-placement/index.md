# PostgreSQL explicit hot/cold placement

dm-cache is automatic block heat.

If later you know:

```text
specific archive partition should always be cold
```

you can additionally use:

```text
PostgreSQL partitioning
tablespaces
```

## Example

```text
recent activity partitions
  cached/main path

old audit partitions
  explicit HDD tablespace
```

## The rule

Do not add this complexity before data size justifies it.
