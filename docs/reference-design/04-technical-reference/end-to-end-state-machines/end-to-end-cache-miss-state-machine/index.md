# End-to-end cache miss state machine

```text
BFF request
  ↓
HybridCache L1
  │ hit -> return
  ↓ miss
Dragonfly L2
  │ hit -> populate L1 -> return
  ↓ miss
origin loader
  ↓
service/PostgreSQL/external API
  ↓
populate L2
  ↓
populate L1
  ↓
return
```

## After mutation

```text
invalidate/update relevant L2
invalidate current L1
publish projection invalidation if cross-Pod immediacy required
```

Do not invalidate the entire cache namespace for one changed comment.
