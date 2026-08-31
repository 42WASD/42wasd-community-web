# Security invariants

Never violate:

```text
browser never receives database credentials
browser never receives server OAuth secrets
browser never decides authorization
backend services are not publicly exposed by default
Cilium default deny is enforced
Hubble observation does not automatically grant policy
business Pods do not receive cluster-admin
Dapr is not treated as authorization replacement
external media embed HTML is not trusted
queue messages are authenticated/authorized at producer boundary
client version numbers are not authorization claims
```
