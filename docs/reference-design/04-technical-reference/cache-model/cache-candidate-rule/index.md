# Cache candidate rule

## Good cache candidates

```text
public profile summary
forum category page projection
server metadata that changes infrequently
permission-independent lookup tables
expensive external API projection
```

## Poor cache candidates

```text
security decision without correct user keying
rapidly changing per-user draft
one-time command response
data whose invalidation semantics are unknown
```

## Every cache entry must have

```text
key
owner
TTL/freshness policy
invalidation rule
maximum size/budget
```

## If you cannot explain invalidation

```text
do not cache it yet
```
