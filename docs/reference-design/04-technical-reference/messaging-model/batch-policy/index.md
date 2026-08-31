# Batch policy

Batch only where the underlying operation benefits.

## Pattern

```text
collect until:
  N items
  OR
  T milliseconds
```

## Example

```text
user summary lookup:
  N = 50
  T = 5 ms
```

might combine a burst into:

```sql
WHERE id = ANY($1)
```

instead of dozens of small queries.

Exact N/T values require benchmark data.

## The rule

Never introduce a 100 ms batching delay into a 10 ms interactive path just to
save one SQL query.
