# Cross-feature effects

Handle effects that span multiple features.

## Goal

```text
an action in one page affects shared state
a shared update is dispatched, not reached into
```

## How

- Shared effects are messages the root/shared layer owns.
- A feature dispatches a shared effect message rather than mutating shared
  state directly.
- Ownership is enforced by message shape, not by ad-hoc global handlers.

## Verification

An action on one page updates the canonical shared entity cache, and another
page reflecting the same entity shows the updated value.