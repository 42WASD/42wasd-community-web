# Upgrade the web runtime deliberately

Target (architecture doc Phase 2):

```text
.NET 10
Bolero 0.25.x
Radzen 11.2.x tested patch
```

Do not combine this with the database migration in the same first PR.

## Sequence

```text
upgrade SDK/global.json
upgrade Bolero
upgrade Blazor/ASP.NET target
upgrade compatible Radzen
fix compile errors
run browser tests
publish container
deploy staging
```

Record exact package versions.

## Acceptance

```text
[ ] Client compiles
[ ] Server compiles
[ ] Shared compiles
[ ] Blazor WASM boots
[ ] Bolero Remoting still works
[ ] authentication placeholder/current behavior still works before replacement
[ ] Radzen UI renders
[ ] container starts in Kubernetes
```
