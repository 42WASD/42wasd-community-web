# First-load pagination policy

For a brand-new client with no local cache:

```text
load only viewport-sized first projection
```

## Recommended initial targets to benchmark

```text
forum topic list:
  20–40 rows

comments:
  30–50 lightweight rows

server list:
  visible/filter-matching rows only

profile:
  top-level profile + first relevant page
```

Use cursor/keyset pagination when ordering is stable.

Avoid huge:

```text
OFFSET 200000
```

queries as data grows.

## The cursor

The cursor should contain the stable sort boundary required by the query.

Example logical ordering:

```text
created_at DESC, id DESC
```

cursor contains:

```text
last_created_at
last_id
```
