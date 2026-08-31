# Internal code shape of one F# service

Every F# business service should use approximately:

```text
src/
  Service.Api/
  Service.Application/
  Service.Domain/
  Service.Infrastructure/

proto/
tests/
deploy/
```

Conceptually:

```text
Api
  ↓
Application use case
  ↓
Domain rules
  ↓
ports
  ↓
Infrastructure adapters
```

## Example Forum service

```text
Forum.Api
  gRPC handlers
  Dapr event handlers
  health endpoints

Forum.Application
  CreatePost
  EditPost
  DeletePost
  GetTopicPage
  GetTopicChanges

Forum.Domain
  Post
  Comment
  permissions
  validation
  pure transformations

Forum.Infrastructure
  PostgreSQL
  SQLProvider
  Atlas integration hooks
  cache adapter
  outbox adapter
```

## The domain reference rule

The domain project should not reference:

```text
Npgsql
Dapr
RabbitMQ
Kubernetes
SignalR
Bolero
```

unless a genuine domain concept requires one.
