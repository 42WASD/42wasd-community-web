# The root message

The root message drives navigation and ownership changes.

## Canonical shape

```fsharp
type Msg =
    | PageChanged of Page
    | Shared of Shared.Msg
    | Local of LocalMsg
```

## Notes

- `PageChanged` is the only message that changes routing.
- Shared messages delegate to shared state updates.
- Local messages are lifted with `Cmd.map` from the active page.

## Discipline

No message reaches into another owner's state. Ownership is enforced by
message shape.