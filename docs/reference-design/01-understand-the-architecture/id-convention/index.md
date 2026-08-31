# ID convention

Selected default domain ID:

# **UUIDv7**

Reasons:

```text
generated independently by services/clients where needed
128-bit distributed uniqueness
time-ordered compared with random UUIDv4
native PostgreSQL uuid type
PostgreSQL 18 has uuidv7()
better B-tree locality than random v4
```

Do not encode UUIDs as hex strings inside PostgreSQL.

For purely database-local high-volume surrogate keys where distributed creation
is unnecessary, `bigint` remains reasonable.

Use a stable domain ID rule per entity type and do not change casually.

## Strong identifiers

Pair the UUID convention with domain-specific F# wrappers so IDs cannot be
confused with each other (see F# domain type conventions):

```fsharp
type PostId = PostId of Guid
```
