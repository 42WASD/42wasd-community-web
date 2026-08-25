# The state ownership model

Use this decision table to decide where any piece of state belongs.

| State | Owner |
|---|---|
| Current route | `Page` |
| Route parameters | `Page` DU |
| Temporary page state | `PageModel<'T>` / page `Model` |
| Canonical entities reused by many pages | `Shared.Model` |
| Authenticated user | `Shared.Model` |
| Community configuration | `Shared.Model` |
| Search text used only on Events page | `Events.Model` |
| Event filter | `Events.Model` |
| Selected event already present in shared cache | store `EventId`, not another `Event` |
| Static hero text | view/module constant, not Elmish state |
| Pure card props | function arguments |
| Remote call in progress | model owned by the feature/data slice that requested it |
| Server-returned canonical data | shared state if reused across routes |
| Login/password fields | page-local model; discard after leaving page |
| Dark theme persisted app-wide | shared/app UI state |
| Modal that only exists in one page | page-local model |

## The rule

> Put state at the **lowest level that fully owns it**, but **no lower than the
> level at which it must be shared**.

This keeps the root types small while avoiding both a monolith and excessive
nesting.