# Recommended source-of-truth model

The architecture has several copies of information.

They are **not equally authoritative**.

Use this ownership table:

```text
Git:
  source code
  .proto contracts
  browser DTO source definitions
  Atlas migration history
  Kubernetes application manifests
  application-specific Cilium policies
  Dapr declarative configuration
  release/version records

Central cluster-infra Git repository:
  Argo CD bootstrap
  AppProjects / ApplicationSets
  Cilium / Hubble platform installation
  cluster-wide/default network policy
  Dapr control plane
  CloudNativePG operator
  RabbitMQ platform deployment
  Dragonfly platform deployment
  observability platform
  storage classes / Local PV declarations
  third-party chart references and pinned versions

PostgreSQL:
  canonical accounts
  personas
  external account links
  forum posts
  comments
  reactions
  durable game/server metadata owned by the platform
  tournaments
  durable notifications
  transactional outbox
  inbox/idempotency records
  durable job/workflow references where the owning service requires them

Dragonfly:
  disposable shared cache
  hot projections
  short-lived coordination where loss is acceptable
  SignalR Redis-protocol backplane only after compatibility testing

ASP.NET / service Pod memory:
  tiny L1 cache
  active request state
  current connections
  temporary computed values

Browser IndexedDB:
  disposable route/query cache
  active-scope cursor metadata
  drafts
  client outbox
  provider upload session metadata

SignalR:
  notification/delta transport only
  never canonical truth

RabbitMQ:
  durable work waiting to be processed
  integration events waiting for consumers
  not the canonical application database

BYOA provider:
  authoritative bytes for user media
  YouTube owns YouTube video bytes
  future image/file provider owns its media bytes
```

## The rule

```text
Do not make the same fact independently authoritative in two places.
```

For example:

```text
PostgreSQL says:
Post 123 body = "hello"

IndexedDB says:
Post 123 body = "old hello"
```

PostgreSQL wins.

IndexedDB is merely an old local representation.

Similarly:

```text
Dragonfly lost all keys
```

must not destroy any forum post.

The application should repopulate it from PostgreSQL.
