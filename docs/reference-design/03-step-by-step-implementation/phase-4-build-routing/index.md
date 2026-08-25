# Build routing

Establish routing with a `Page` union bound to the URL.

## Goal

```text
Page DU:
  Home
  Events
  Projects
  Members
  Contact
  ...
```

## Implementation

```text
Page stored in the root model
PageChanged message updates it
Router.infer binds route <-> Page
PageModel<'T> holds page state not in the URL
```

## Verification

```bash
dotnet run
```

- `/` -> Home
- `/events` -> Events

Wrong URLs fall back predictably.