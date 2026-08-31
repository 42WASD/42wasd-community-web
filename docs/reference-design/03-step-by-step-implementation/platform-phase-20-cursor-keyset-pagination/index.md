# Implement cursor/keyset pagination

Forum list API/remoting contract (architecture doc Phase 20):

```fsharp
type PageRequest =
    {
        Cursor: string option
        Limit: int
    }

type Page<'T> =
    {
        Items: 'T array
        NextCursor: string option
        ScopeRevision: int64
    }
```

Use keyset pagination rather than deep `OFFSET`.

## Acceptance

```text
[ ] stable ordering under new inserts
[ ] no duplicated/skipped rows for normal pagination
[ ] limit enforced server-side
```
