# Application repository

The current repository already contains a `deploy/k8s` directory and a separate
`src` tree.

Evolve toward:

```text
42wasd-community-web/
├── src/
│   ├── Community.Web.Client/
│   ├── Community.Web.Server/
│   └── Community.Web.Shared/
├── tests/
├── deploy/
│   └── k8s/
│       ├── base/
│       │   ├── deployment.yaml
│       │   ├── service.yaml
│       │   ├── configmap.yaml
│       │   ├── cilium-policy.yaml
│       │   └── kustomization.yaml
│       └── overlays/
│           ├── staging/
│           └── production/
└── docs/
```

Each backend service repo follows the same deployment ownership convention.
