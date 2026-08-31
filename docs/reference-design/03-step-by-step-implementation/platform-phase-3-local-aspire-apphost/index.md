# Local Aspire AppHost

Create the local distributed-development host (architecture doc Phase 3):

```text
dev/Community.AppHost/
```

A small C# Aspire AppHost is acceptable even though services are F#.

## Initially model

```text
community-web
postgres-dev
dragonfly-dev
rabbitmq-dev
```

Later add services as they are extracted.

## Goal

```text
one command starts local distributed environment
one dashboard shows logs/traces/resources
```

Do not use Aspire as production deployment truth.

## Acceptance

```text
[ ] new developer can run stack from README
[ ] ports are not manually hard-coded across five terminals
[ ] logs visible in Aspire dashboard
```
