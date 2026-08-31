# Upgrade order

## For ordinary compatible release

```text
1. backups verified
2. migration compatibility checked
3. deploy additive DB migration if required
4. deploy backend services
5. deploy BFF
6. publish new PWA/static client
7. monitor old-client compatibility
8. remove old schema only in later release
```

## For Dapr/Cilium/Argo/CNPG upgrades

```text
staging
  ↓
vendor upgrade notes
  ↓
compatibility tests
  ↓
backup/recovery check
  ↓
one platform component at a time
  ↓
production
```

## Never combine

```text
Cilium major upgrade
PostgreSQL major upgrade
Dapr major upgrade
whole app schema rewrite
```

into one production change.
