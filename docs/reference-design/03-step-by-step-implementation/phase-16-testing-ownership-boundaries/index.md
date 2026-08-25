# Testing ownership boundaries

Verify the ownership rules stay intact as the app grows.

## Test targets

```text
- pure update functions
- routing round-trips
- message ownership (no cross-boundary state reach)
- normalization of entity caches
```

## Tooling

```text
expecto / xUnit
pure tests need no DOM
a handful of browser integration tests
```

## Verification

```bash
dotnet test
```

All ownership-boundary tests pass, catching regressions that would reintroduce
monolithic state.