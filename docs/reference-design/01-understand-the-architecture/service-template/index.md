# The 42wasd-service-template

Every new backend service should start with:

```text
42wasd-service-template/
├── src/
│   ├── Service.Domain/
│   ├── Service.Application/
│   ├── Service.Infrastructure/
│   └── Service.Api/
├── proto/
│   └── v1/
├── tests/
│   ├── Unit/
│   └── Integration/
├── deploy/
│   └── k8s/
│       ├── base/
│       └── overlays/
├── Dockerfile
└── README.md
```

## Pre-wired concerns

```text
F#/.NET 10
ASP.NET Core host
gRPC
Dapr sidecar annotations
OpenTelemetry
health/live + health/ready
structured logs
CancellationToken propagation
problem/error mapping
Cilium labels
non-root container
resource requests/limits
PodDisruptionBudget where relevant
```

Build the template once; generate each new service from it (see the service
template phase in the implementation part).
