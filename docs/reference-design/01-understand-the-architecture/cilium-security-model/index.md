# Cilium security model

Selected baseline:

```text
default deny
explicit allow dependencies
DNS allow
no "same namespace means trusted"
```

Cilium policy is per-direction.

## Standard vs Cilium policies

Use standard Kubernetes NetworkPolicy when L3/L4 selectors are enough.

Use CiliumNetworkPolicy when you need Cilium-specific L7/FQDN/identity
features.

Avoid mixing policy dialects unnecessarily.

## Failure handling

If default deny breaks a dependency, use Hubble to inspect source, destination,
port, verdict and propose a narrow policy update. Do not disable default deny
as the permanent fix.

Policy Audit Mode is for discovery in staging/non-production — it must never
become the production security posture because audit mode allows traffic that
policy would otherwise deny.
