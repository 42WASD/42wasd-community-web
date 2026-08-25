# The recommended RemoteData

A single, shared `RemoteData<'T>` type for async server state.

## Canonical shape

```fsharp
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Loaded of 'T
    | Failed of string
```

## Rules

- Use `RemoteData` for every async server-backed value.
- Map to view variants with `match` or helpers.
- Do not introduce ad-hoc `bool IsLoading` / `bool HasError` fields in
  parallel to `RemoteData`.