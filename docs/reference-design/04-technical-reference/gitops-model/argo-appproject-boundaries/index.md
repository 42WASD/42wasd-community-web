# Argo AppProject boundaries

Create AppProjects separating:

```text
platform
community-apps
data
```

## Application project should restrict

```text
allowed source repos
allowed namespaces
allowed cluster-scoped resources
```

## A normal application repo should not be able to create

```text
ClusterRole cluster-admin
CiliumClusterwideNetworkPolicy
StorageClass
CRD
```

unless explicitly approved in the platform project.
