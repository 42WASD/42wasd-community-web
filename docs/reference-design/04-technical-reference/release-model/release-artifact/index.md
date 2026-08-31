# Release artifact

Every production release should record:

```yaml
platformRelease: "2026-08-r1"

application:
  dotnet: "10"
  bolero: "0.25.x-pinned"
  radzen: "11.2.x-pinned"
  contractVersion: 1
  indexedDbSchemaVersion: 1

platform:
  postgres: "18.x-pinned"
  cloudNativePG: "1.30.x-pinned"
  dapr: "1.18.x-pinned"
  cilium: "1.20.x-pinned"
  argocd: "3.5.x-pinned"
  rabbitmq: "4.2.x-pinned"
  dragonfly: "pinned-tested-release"

database:
  atlasMigration: "20260827..."
```

## The rule

Record exact container image digests in production.

Do not use floating `latest`.
