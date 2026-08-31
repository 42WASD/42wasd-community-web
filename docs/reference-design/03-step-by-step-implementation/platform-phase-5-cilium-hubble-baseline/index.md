# Cilium/Hubble platform baseline

Central infra owns (architecture doc Phase 5):

```text
Cilium version/config
Hubble enabled
Hubble Relay
Hubble UI access policy
cluster-wide baseline policies
DNS baseline policy
```

Verify:

```text
cilium status
hubble status
```

Create default-deny policy strategy consistent with your existing cluster.

## Important

```text
DNS must remain allowed where workloads need service discovery
health/control-plane paths must be tested
```

## Acceptance

```text
[ ] Hubble sees flows across cluster through Relay
[ ] denied test flow appears as denied/drop
[ ] same-namespace unapproved flow is not implicitly trusted
[ ] intentional DNS works
```
