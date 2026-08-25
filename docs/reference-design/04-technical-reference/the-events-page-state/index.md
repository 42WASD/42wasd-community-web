# The events page state

A concrete worked example of the recommended state for the Events page.

## Canonical shape

```fsharp
type EventsPage.Model =
    {
        Events : RemoteData<Event list>
        Editing : EventId option   // when applicable
    }
```

## Rules applied

- `Events` is page-local here.
- If events were needed across pages, the canonical cache would be a
  `RemoteData<Map<EventId, Event>>` in `Shared`, and this page would select
  from it.
- Loading/error states come from `RemoteData`, not ad-hoc flags.