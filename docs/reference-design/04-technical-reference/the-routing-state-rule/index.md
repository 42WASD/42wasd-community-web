# The routing state rule

The route is the single source of truth for what page is active.

## Rules

- `Page` is a union whose cases map to routes.
- The route lives in the root model.
- `PageChanged` is the only message that changes the route.
- `PageModel<'T>` holds page state that is **not** part of the URL.

## Consequences

- Navigation and visible state never disagree.
- The browser URL and the UI stay in sync.
- Unknown routes fall back predictably.