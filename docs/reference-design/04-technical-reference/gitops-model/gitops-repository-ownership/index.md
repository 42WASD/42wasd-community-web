# GitOps repository ownership

Central repository:

```text
cluster-infra/
  bootstrap/
  clusters/
    homelab/
      argocd/
        projects/
        applicationsets/

      networking/
        cilium/
        hubble/
        defaults/
        cluster-policies/

      data/
        cloudnative-pg/
        postgres-storage/

      messaging/
        rabbitmq/

      cache/
        dragonfly/

      runtime/
        dapr/

      observability/
```

Application repository:

```text
42wasd-community-web/
  src/
  proto/
  database/
  tests/

  deploy/
    k8s/
      base/
        community-web/
        accounts-service/
        forum-service/
        game-service/
        tournament-service/
        notification-worker/

      overlays/
        staging/
        production/
```

Application-specific Cilium policies live next to the corresponding
application deployment.
