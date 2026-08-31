# Prepare GitOps ownership

Central infra (architecture doc Phase 4):

```text
cluster-infra/
```

Application repo keeps:

```text
deploy/k8s/
```

Create an Argo `AppProject` for community workloads with:

```text
allowed source repos
allowed destination namespaces
restricted cluster resources
```

Register application repo through a central `Application`/`ApplicationSet`.

## Acceptance

```text
[ ] Argo reads application repo
[ ] application repo does not need cluster-admin
[ ] AppProject prevents deployment to arbitrary namespaces
[ ] manual cluster drift is visible
```
