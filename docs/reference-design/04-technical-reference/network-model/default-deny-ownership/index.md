# Default-deny ownership

Central cluster-infra repository owns:

```text
namespace baseline
cluster-wide default posture
DNS baseline policy
platform component policy
```

## Application repository owns

```text
"this app may call that dependency"
```

## Example app policy intent

```text
community-web
  -> forum-service gRPC
  -> accounts-service gRPC
  -> dragonfly
  -> kube-dns
```

That dependency belongs with `community-web`.
