# The source-of-truth model

Use this as a **hard architecture invariant**. Several stores hold copies of
information; they are not equally authoritative.

```text
Git:
    source code
    protobuf contracts
    DB migration history
    Kubernetes manifests
    Cilium policies
    Dapr component definitions
    environment overlays
    tested component versions

Browser CacheStorage:
    published application/runtime static snapshot

Browser IndexedDB:
    disposable local projections/cache
    user drafts
    local outbox
    resumable upload session metadata
    active-scope sync cursors

Elmish:
    current interactive UI state

PostgreSQL:
    authoritative community business data
    accounts/profile ownership
    forum posts/comments/reactions
    tournaments
    durable job/business metadata
    transactional outbox/inbox records

Dragonfly:
    disposable hot cache
    optional SignalR backplane after compatibility testing
    ephemeral distributed coordination only where appropriate

RabbitMQ:
    accepted but unfinished durable asynchronous messages/work

Dapr Workflow:
    durable orchestration state for selected multi-step processes

External media provider:
    authoritative media bytes for BYOA attachment

Kubernetes:
    actual running workload/resource state

Cilium:
    enforced network reachability policy

Hubble:
    observed network/security flow evidence

Argo CD:
    reconciliation status between Git desired state and cluster
```

## The rule

Do not copy every fact into every store.

PostgreSQL says `Post 123 body = "hello"` and IndexedDB says
`"old hello"` → **PostgreSQL wins**; IndexedDB is merely an old local
representation. Losing the Dragonfly cache must never destroy a forum post.
