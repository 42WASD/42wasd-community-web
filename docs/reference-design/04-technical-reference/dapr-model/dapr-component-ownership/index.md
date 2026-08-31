# Dapr component ownership

Dapr configuration belongs to the application when it describes:

```text
which pub/sub component name it uses
which topics it publishes/subscribes
app-level access scopes
resiliency policy
```

## Cluster infra owns

```text
Dapr control plane installation
platform-wide component operators
shared broker deployment
```

Secrets should be referenced rather than embedded.
