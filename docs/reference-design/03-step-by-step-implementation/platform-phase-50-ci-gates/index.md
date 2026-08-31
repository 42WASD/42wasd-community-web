# CI gates

Every service (architecture doc Phase 50):

```text
format/lint
unit tests
integration tests
proto compatibility
container build
SBOM/security scan if available
Kubernetes render validation
Cilium policy validation
```

## Database

```text
Atlas migration lint
migration apply to disposable Postgres
upgrade test from previous schema
```

## Web

```text
F# tests
browser integration/e2e
PWA update test
IndexedDB migration test
contract compatibility test
```
