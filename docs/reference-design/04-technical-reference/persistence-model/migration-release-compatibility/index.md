# Migration release compatibility

Use expand/contract for breaking schema transitions.

## Example rename

Bad:

```text
release A:
DROP old_column
ADD new_column

while old Pods still use old_column
```

## Better

```text
Release 1:
ADD new_column
application writes old + new

Release 2:
backfill new column
read new column

Release 3:
old version no longer supported
DROP old_column
```

This permits rolling application deployments.
