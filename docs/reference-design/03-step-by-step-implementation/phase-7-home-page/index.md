# Home page

Build the Home page from shared state.

## Goal

```text
Hero
Stats
Upcoming events
Featured projects
Members
Join Discord / GitHub
```

## State ownership

- Home reads from `Shared.Model`.
- It does not own a canonical copy of entities.

## Verification

```bash
dotnet run
```

Navigate to `/` and see the Home page render from shared state.