# GitOps ownership model

The user-selected ownership rule is:

```text
central infra repo:
    cluster/platform components
    cluster-wide guardrails
    Argo bootstrap/application registration

application repo:
    that application's own Kubernetes deployment contract
    Service
    Dapr annotations
    resource limits
    app-specific Cilium policy
```

## The deliberate tradeoff

This intentionally differs from Argo CD's strongest recommendation to keep
application configuration in a separate config repository.

The tradeoff is accepted because ownership locality and AI-agent
maintainability are prioritized.

## Mitigations

```text
CODEOWNERS on deploy/
separate CI paths
GitHub Actions path filters
Argo read-only source access
protected production branches
manifest validation
```
