# Third-party component ownership

For:

```text
Cilium
Dapr
CloudNativePG
RabbitMQ operator/chart
Dragonfly chart
Argo CD
```

prefer:

```text
central Argo Application references upstream chart/repository
pinned version
small values overlay
```

rather than copying thousands of vendor YAML lines into the infra repository.

## Record

```text
source URL
version
digest if applicable
values
upgrade notes
```
