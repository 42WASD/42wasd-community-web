# Realtime event envelope

Every application realtime event should carry enough metadata to reject
stale/duplicate events.

## Conceptual

```fsharp
type RealtimeEnvelope<'T> =
    {
        EventId: Guid
        Scope: string
        Sequence: int64
        OccurredAt: DateTimeOffset
        Payload: 'T
    }
```

## Example

```text
PostEdited
  postId = P
  version = 19
```

## Client behavior

```text
if local version >= 19:
    ignore

elif event includes sufficient delta:
    apply

else:
    invalidate/fetch active scope
```

SignalR is an acceleration path.

Cursor/delta synchronization remains the repair path.
