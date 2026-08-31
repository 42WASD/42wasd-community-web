# Install Dapr and make service invocation deny-by-default

Central infra deploys pinned Dapr 1.18.x (architecture doc Phase 31).

Start without converting every existing call.

Verify:

```text
operator
sidecar injector
placement/scheduler as required by used building blocks
Sentry/certificate health
mTLS remains enabled
```

## The warning

Dapr service invocation is **not deny-by-default just because Cilium is
default-deny**. They are separate authorization layers. Dapr's documented
behavior is allow-all when no service-invocation access-control policy exists.

Create a Dapr `Configuration` for each called application (or a carefully
shared configuration) with an explicit global deny and the minimum caller
allow-list.

## Illustrative baseline

```yaml
apiVersion: dapr.io/v1alpha1
kind: Configuration
metadata:
  name: community-service-defaults
  namespace: community
spec:
  mtls:
    enabled: true
  accessControl:
    defaultAction: deny
    trustDomain: public
    policies:
      - appId: community-web
        namespace: community
        trustDomain: public
        defaultAction: deny
        operations:
          # Replace with the exact HTTP/gRPC operation names proved by
          # integration tests.
          - name: "/community.forum.v1.ForumService/**"
            action: allow
```

The exact gRPC operation path must be verified against the generated service
and Dapr invocation behavior before enforcement. Do not copy wildcard rules
without observing the actual calls.

Cilium policies must independently permit the required sidecar/control-plane/
network paths. Think:

```text
Cilium
  = can these workloads establish the network flow?

Dapr ACL + mTLS identity
  = may this authenticated Dapr application invoke this operation?
```

## Acceptance

```text
[ ] sample app with sidecar becomes Ready
[ ] Dapr mTLS/Sentry health verified
[ ] explicitly allowed service invocation succeeds
[ ] unlisted Dapr app invocation is denied
[ ] Cilium still blocks undeclared network paths
[ ] Hubble shows only expected paths
```
