# Rendering optimization

Optimize rendering only by evidence.

## When to optimize

```text
measured slow re-renders on large lists
expensive derivations
```

## How

```text
ElmishComponent / isolation where justified
keep most rendering as pure functions
normalize data so updates are O(1) per entity
```

## Rule

> Do not split rendering into components preemptively. Optimize by evidence.