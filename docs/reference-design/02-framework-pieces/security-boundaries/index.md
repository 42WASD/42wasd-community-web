# Security boundaries

## Browser boundary

Browser is untrusted.

Never place:

```text
PostgreSQL credentials
RabbitMQ credentials
Dapr control credentials
server-side OAuth client secrets
internal admin tokens
```

in WebAssembly.

## BFF boundary

BFF authenticates browser identity.

It is allowed to shape browser DTOs.

It does not automatically become authorized for every backend action.

Propagate explicit user identity/claims/context to domain services.

## Service authorization

Network allow ≠ user authorization.

Even if Cilium permits:

```text
BFF -> Forum Service
```

Forum Service still verifies:

```text
who is acting
what operation
resource ownership
moderator/admin policy
```

## Database boundary

Each service should ideally have the least database permissions needed.

As service ownership matures, consider:

```text
separate DB roles
separate schemas
eventual separate databases only where useful
```

Avoid one superuser connection string shared by every workload.

CloudNativePG 1.30 `DatabaseRole` is useful for GitOps-friendly role ownership.

## Secrets

Production secrets:

```text
Kubernetes Secret at minimum
external secret manager if/when available
```

Never Git plaintext secrets.

Do not put OAuth refresh tokens in logs.

## Media

Only allow provider adapters/domains you trust.

Sanitize user-supplied metadata.

Never render arbitrary user-provided raw HTML/JavaScript embed code.

Use provider's standard embed/player endpoint with a validated resource ID.
