# Add rate limiting

Policies (architecture doc Phase 30):

```text
login
post/create
comment/create
search
sync
external integration
job submit
SignalR connect
```

Partition by account where authenticated.

Use IP conservatively for unauthenticated abuse controls.

Start with ASP.NET Core rate limiting as **per-Pod protective admission
control**. If BFF replicas are later increased above one, explicitly decide
which limits must remain per-Pod and which are product-wide quotas.
Product-wide quotas must use a shared enforcement point/counter; they are not
automatically coordinated by the built-in limiter across Pods.

## Acceptance

```text
[ ] abusive user cannot exhaust expensive query endpoint
[ ] normal bursty page load is not accidentally throttled
[ ] multi-replica test proves whether each policy is intentionally per-Pod or globally enforced
```
