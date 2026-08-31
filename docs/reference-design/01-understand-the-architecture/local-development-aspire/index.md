# Local development with Aspire

Production remains Kubernetes/Argo.

Local developer experience:

```text
Community.AppHost  (small C# or TypeScript Aspire AppHost)
    ├── Community.Web
    ├── Accounts Service
    ├── Forum Service
    ├── Server Service
    ├── Tournament Service
    ├── Dragonfly
    ├── RabbitMQ
    └── development PostgreSQL
```

## What Aspire provides

```text
one-command startup
local service discovery
central logs/traces/dashboard
dependency visualization
AI/agent-friendly app model
```

## The rule

It is **not** a production replacement for Kubernetes.
