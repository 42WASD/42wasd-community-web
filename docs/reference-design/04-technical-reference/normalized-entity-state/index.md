# Normalized entity state

Store canonical entities once, by ID, in maps.

## Canonical shape

```fsharp
type EntityCache<'TId, 'T> =
    Map<'TId, 'T>
```

## Rules

- Store entities by ID once.
- Refer to entities by ID elsewhere.
- Update in place without duplicating the entity.

## Example

```fsharp
type Shared.Model =
    {
        Events : RemoteData<Map<EventId, Event>>
        Projects : RemoteData<Map<ProjectId, Project>>
    }
```

## Why

- Consistent identity across pages.
- No stale duplicates.
- Efficient point updates.