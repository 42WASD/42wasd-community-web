# Performance principles

Optimize only by evidence.

## Principles

- Write correct, simple MVU first.
- Measure before optimizing.
- Use `ElmishComponent` / render isolation only where measured to matter.
- Keep most rendering in pure functions.
- Prefer `Map` lookups over traversals for entity data.

## The guide

```text
Profile or identify the actual bottleneck
Fix only that
Re-measure
```

## Performance: when it matters

- Large lists re-rendered on every keystroke → isolate or memoize.
- Expensive derivations → compute once, reuse.
- Normalize data so updates are O(1) per entity.