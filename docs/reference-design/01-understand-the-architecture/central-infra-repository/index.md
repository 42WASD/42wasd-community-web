# Central infrastructure repository

Recommended layout:

```text
cluster-infra/
├── bootstrap/
│   └── argocd/
├── clusters/
│   └── homelab/
│       ├── projects/
│       ├── applicationsets/
│       └── cluster-policies/
├── platform/
│   ├── cilium/
│   ├── hubble/
│   ├── dapr/
│   ├── cloudnative-pg/
│   ├── rabbitmq/
│   ├── dragonfly/
│   └── observability/
├── storage/
│   ├── local-pv/
│   └── docs/
│       └── lvm-dm-cache.md
└── thirdparty/
    └── app-definitions/
```

## Chart hygiene

Do not copy full upstream third-party charts into this repository unless patch
ownership requires it.

Pin:

```text
chart version
Git tag
container digest where practical
```

and let Argo source the upstream chart/repository.
