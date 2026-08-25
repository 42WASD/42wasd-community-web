# Shared application state

Define the persistent cross-page state in `Shared.Model`.

## Goal

```fsharp
type Shared.Model =
    {
        Events    : RemoteData<Map<EventId, Event>>
        Projects  : RemoteData<Map<ProjectId, Project>>
        Members   : RemoteData<Map<MemberId, Member>>
        Account   : AccountState option
    }
```

## Rules

- Holds state that survives navigation and spans pages.
- Uses `RemoteData<'T>` for async server-backed values.
- Stores canonical entities normalized in maps by ID.

## Verification

Two pages reading the same entity list share one canonical cache.