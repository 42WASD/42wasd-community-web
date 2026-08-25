# The root source of truth model

The root model is the single source of truth for all application state.

## Canonical shape

```fsharp
type Model =
    {
        Page : Page
        Shared : Shared.Model
        Local : PageLocalModel option
    }
```

## Rules

```text
Page is the active route
Shared holds persistent cross-page state
Local holds the active page's ephemeral state (when needed)
```

## The root source of truth

State that must be consistent across the app lives in `Shared`. Everything else
is page-local and owned by exactly one parent.