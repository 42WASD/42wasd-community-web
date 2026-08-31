# SQLProvider role

Selected persistence access:

```text
PostgreSQL schema
       ↓
FSharp.Data.SqlProvider
       ↓
typed F# database access
```

The provider is an infrastructure dependency.

It does not define your browser DTOs.

It does not replace:

```text
domain rules
transactions
migration policy
indexes
query design
```

## Escape hatch

If SQLProvider becomes limiting for a particular hot query:

```text
use Npgsql/raw SQL for that adapter
```

without changing the domain contract.
