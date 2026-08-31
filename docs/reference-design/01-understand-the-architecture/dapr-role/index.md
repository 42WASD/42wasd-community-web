# Dapr role

Dapr provides reusable distributed-system building blocks:

```text
service invocation
pub/sub
workflow
actors if later needed
secrets/config access where useful
jobs
bindings if useful
tracing integration
mTLS/service identity
```

## Dapr does not provide

```text
forum rules
account rules
tournament rules
moderation policy
```

## The rule

Dapr owns reusable distributed plumbing. Your services own domain behavior.
Dapr is not your business logic, and Dapr service invocation is not an
authorization system by itself — configure explicit accessControl policies
(see Dapr plus Cilium).
