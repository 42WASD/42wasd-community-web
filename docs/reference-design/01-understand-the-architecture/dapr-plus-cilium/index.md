# Dapr plus Cilium

Dapr and Cilium solve different concerns.

```text
Dapr:
    app identity / mTLS / service invocation / distributed APIs

Cilium:
    which network flows are reachable at all
```

## What a service may reach

A service should be allowed to reach only:

```text
its own Dapr sidecar locally
specific backend services through expected paths
DNS
required platform control-plane endpoints
its database/cache/broker dependencies where direct connection is intended
```

## Two authorization layers

```text
Cilium
  = can these workloads establish the network flow?

Dapr ACL + mTLS identity
  = may this authenticated Dapr application invoke this operation?
```

## Configuration requirement

Dapr service invocation is **not deny-by-default just because Cilium is
default-deny**. They are separate authorization layers. Dapr's documented
behavior is allow-all when no service-invocation access-control policy exists.

Create a Dapr `Configuration` with an explicit global deny and the minimum
caller allow-list.

## Testing requirement

Policy tests must include Dapr control-plane and sidecar communication so
default deny does not break the runtime.
