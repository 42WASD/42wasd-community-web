# Policy discovery

In staging/non-production:

```text
deploy workload
    ↓
policy audit/discovery mode where appropriate
    ↓
exercise integration/e2e tests
    ↓
Hubble records required flows
    ↓
AI/operator proposes least-privilege policy PR
    ↓
review/CI
    ↓
Argo deploys enforced policy
```

## The rule

Cilium Policy Audit Mode must not become the production security posture
because audit mode allows traffic that policy would otherwise deny.

Observed traffic is **evidence**, not authorization:

```text
never:
observe denied flow
    ↓
automatically permit it
```
