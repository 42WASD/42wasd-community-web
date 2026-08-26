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

## Slice order

Release **one working slice at a time**, in this order — each slice is
deployable and verified before the next starts:

```text
1. Infrastructure   — Dockerfile + GHCR image + K8s Deployment/Service/Ingress
2. Public data      — read-only pages (Home, Games, Servers, Teams, About)
3. Auth             — Account sign-in/out (gate the Members page)
4. Writes           — cross-feature effects (tournament toggle, favourites)
5. Hardening        — data-protection, scaling, logging, monitoring
```

## Safety rules

```text
- security/auth goes live before public data
- keep the container read-only except where a write is the feature
- docs + verify.sh green before each release (releasable after every slice)
```

## Verification

The app is releasable after each vertical slice, not only at the end. Every
slice ends with `bash scripts/docs/verify.sh` → `VERIFY OK` and the app's own
tests passing.