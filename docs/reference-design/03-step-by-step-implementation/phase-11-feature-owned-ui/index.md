# Feature-owned UI

Let the owning feature render and own its UI state.

## Goal

```text
each page/feature owns its model, message, update, view
Shared state is selected, not duplicated
render isolation (ElmishComponent) used only where measured
```

## Rules

- Views depend on the feature's own state.
- Views never reach into another owner's state.
- Rendering boundary != state ownership boundary.

## Verification

A feature's local state changes without triggering unrelated re-renders, and
no component duplicates canonical shared data.