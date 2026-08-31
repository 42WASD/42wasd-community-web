# Dapr service invocation contract

Use Dapr for:

```text
service discovery
secure invocation plumbing
tracing integration
resiliency policies
```

Use gRPC/Protobuf for the actual typed contract.

## Conceptual path

```text
Forum Service
  generated gRPC client
       ↓
local Dapr sidecar
       ↓
remote Dapr sidecar
       ↓
Accounts Service gRPC endpoint
```

Dapr sidecars themselves use gRPC between sidecars.

Do not replace every explicit business API with an untyped generic Dapr
payload.
