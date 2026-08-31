# Optimistic concurrency in PostgreSQL

Mutation request:

```text
postId
expectedVersion = 18
newTitle
```

## SQL concept

```sql
UPDATE forum.posts
SET title = $1,
    version = version + 1,
    updated_at = now()
WHERE id = $2
  AND version = $3;
```

If affected rows:

```text
1
  success

0
  not found OR version conflict
```

Resolve which case is required by application semantics.

Do not use long-lived edit locks for ordinary forum posts.
