# Media preview/resolution

Client renders (architecture doc Phase 45):

```text
YouTube embed/player from resource ID
validated external image provider
```

BFF may cache provider metadata only if needed.

Do not proxy media bytes.

## Acceptance

```text
[ ] home-server bandwidth contains metadata/control traffic, not video body
[ ] removed provider resource yields graceful unavailable UI
```
