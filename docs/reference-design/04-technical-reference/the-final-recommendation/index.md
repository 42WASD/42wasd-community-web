# The final recommendation

## The recommendation

Use **one root Elmish program** with a **`Page` route** in the root model,
**`Shared.Model`** for persistent cross-page state, **`RemoteData<'T>`** for
async server values, **normalized entity maps** for canonical entities, and
**page-local `Model`/`Msg`** (lifted with `Cmd.map`) for the handful of pages
that genuinely need their own state. Keep the server boundary isolated behind
shared contract types and remoting.

## Why

- One routing source of truth keeps URL and UI consistent.
- Shared-vs-local separation keeps the root `update` small and testable.
- `RemoteData` and normalized caches remove ad-hoc flags and stale duplicates.
- Feature-oriented structure matches the way the domain actually changes.

## The takeaway

The architecture is fixed: a single Elmish root, shared state for what spans
pages, page-local state for what does not, and remoting contracts for the
server. Everything else is discipline — and the acceptance test is that every
feature survives navigate-away-and-back in the correct state.