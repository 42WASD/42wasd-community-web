# Hubble policy-development workflow

Use:

```text
STAGING

default-deny intent
     ↓
Policy Audit Mode temporarily
     ↓
integration/e2e tests
     ↓
Hubble policy verdicts
     ↓
expected flow?
     │
     ├── no -> investigate bug/compromise
     │
     └── yes
           ↓
AI/operator proposes least-privilege rule
           ↓
Git PR
           ↓
CI policy validation
           ↓
review
           ↓
Argo sync
           ↓
disable Audit Mode
           ↓
verify enforcement with Hubble
```

## Never

```text
observe denied flow
    ↓
automatically permit it
```

Observed traffic is evidence, not authorization.
