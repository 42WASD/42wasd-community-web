# Ownership rules

Before writing code, establish the ownership rules every later phase follows.

## Rules

```text
1. Shared.Model owns persistent cross-page application state.
2. PageLocal owns state that exists only on one page.
3. A page-local Model may hold UI/transient state but must not duplicate
   canonical shared entities.
4. Navigation changes the Page route in the root model, never arbitrary UI
   flags.
5. Effects that reach the server belong in the Server boundary (remoting), not
   scattered in views.
```

## Deliverable

A written statement of these rules (this page) agreed to before Phase 1 begins.