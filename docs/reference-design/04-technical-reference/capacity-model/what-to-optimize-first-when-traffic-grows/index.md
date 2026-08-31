# What to optimize first when traffic grows

Order:

```text
1. query/index plans
2. avoid unnecessary data
3. route-scoped synchronization
4. cache hot projections
5. batch appropriate origin calls
6. DB pool sizing
7. disk/cache hit analysis
8. service concurrency
9. compression tuning
10. protocol changes only if still justified
```

## The rule

Always measure before and after.
