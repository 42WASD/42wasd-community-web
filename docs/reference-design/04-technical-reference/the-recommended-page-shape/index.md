# The recommended page shape

Every page follows the same shape.

## Canonical shape

```fsharp
module Pages.Events

type Model = { Events: RemoteData<Event list> }

type Msg =
    | Load
    | Loaded of Result<Event list, string>

let init = ...

let update msg model = ...

let view model dispatch = ...
```

## Rules

- Page module holds `Model`, `Msg`, `init`, `update`, `view`.
- No number prefixes in page H1s.
- The page's state is page-local unless it must be shared.