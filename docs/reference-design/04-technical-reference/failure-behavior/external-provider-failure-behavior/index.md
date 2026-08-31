# External provider failure behavior

## If YouTube is unavailable during upload

```text
post media remains Pending/Failed
forum text post remains valid if product permits
upload can retry/resume
```

## If external server-status API is slow

```text
render page without status enrichment
show stale/unknown status
do not block core page
```

This is why multi-source page state is separated.
