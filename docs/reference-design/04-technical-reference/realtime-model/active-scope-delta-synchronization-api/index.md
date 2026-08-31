# Active-scope delta synchronization API

Conceptual shared contract:

```fsharp
type ScopeCursor = ScopeCursor of int64

type ScopeSyncRequest =
    {
        Scope: DataScope
        After: ScopeCursor option
        Limit: int
    }

type Change<'TId, 'TPayload> =
    | Upsert of id: 'TId * version: int64 * payload: 'TPayload
    | Delete of id: 'TId * version: int64

type ScopeSyncResponse<'TChange> =
    {
        Through: ScopeCursor
        Changes: 'TChange array
        HasMore: bool
    }
```

## Algorithm

```text
route enters
  ↓
render local cache if any
  ↓
load scope cursor
  ↓
request changes after cursor
  ↓
apply one batch transactionally
  ↓
advance cursor in same local transaction
  ↓
if HasMore AND route still active:
      request next batch
  else:
      stop
```

If route leaves:

```text
cancel remaining network calls
keep already committed batches
```

This matches the product requirement:

```text
do not throw away completed units
do not keep synchronizing invisible scopes
```
