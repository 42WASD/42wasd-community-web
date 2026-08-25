# The dependency rule

Dependencies point downward and inward; they never point up.

## Canonical direction

```text
App root (ProgramComponent, routing)
    |
    v
Pages
    |
    v
Features / Components
    |
    v
Shared / Framework (Bolero, Elmish)
```

## Rules

- Pages may not depend on the app root.
- Views may not depend on the server.
- The server may not depend on client-specific modules.
- Both client and server depend on `Community.Shared` contracts only.