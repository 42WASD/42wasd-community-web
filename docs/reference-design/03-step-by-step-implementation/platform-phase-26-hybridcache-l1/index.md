# Add HybridCache L1

Install/configure HybridCache (architecture doc Phase 26).

Start with a few obvious projections:

```text
public profile summary
forum category metadata
server catalog metadata
```

Do not cache personalized authorization-sensitive data without a correct key.

## Acceptance

```text
[ ] repeated same request hits L1
[ ] cache keys include tenant/user dimensions where required
[ ] cache TTL documented
```
