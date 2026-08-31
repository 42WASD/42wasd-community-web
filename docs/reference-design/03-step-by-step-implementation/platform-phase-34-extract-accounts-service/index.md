# Extract the Accounts Service

Owns (architecture doc Phase 34):

```text
platform account
personas
external identity mappings
profile core
linked external profiles
account policy
```

Decide whether ASP.NET Identity executes in BFF or Accounts Service.

## Recommended early architecture

```text
browser OAuth callback/session -> BFF
canonical account persistence/policy -> Accounts Service
```

unless centralizing all auth protocol handling in Accounts produces a cleaner
ingress.

Avoid spreading authentication cookie keys across services.

## Acceptance

```text
[ ] one session model
[ ] canonical account ID propagated
[ ] services do not trust arbitrary user ID headers from public clients
```
