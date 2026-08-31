# Difference from strict config-repo GitOps guidance

Argo CD often recommends separate source and deployment configuration
repositories.

Your selected ownership rule intentionally differs:

```text
application manifests stay with application source
```

because you want:

```text
code + deployment dependency policy changed/reviewed together
```

## Mitigate risks with

```text
protected deploy/ paths
CODEOWNERS
required reviews
CI manifest validation
Argo Project restrictions
no application repo access to cluster-wide resources
```

The central infra repo remains the authority for cluster/platform-level
configuration.
