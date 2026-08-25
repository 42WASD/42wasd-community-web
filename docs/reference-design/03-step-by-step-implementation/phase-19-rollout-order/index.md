# Rollout order

Plan the sequence for production rollout.

## Goal

```text
order vertical slices for release
one working slice at a time
```

## Considerations

```text
security (auth) before public data
read-only first, then writes
documentation and verification run green before each release
```

## Verification

The app is releasable after each vertical slice, not only at the end.