# Argo CD application generation

Central infra owns the Argo objects that point to application repositories.

## Generators

For a small fixed set:

```text
ApplicationSet List generator
```

For many standardized repos:

```text
ApplicationSet Git/SCM generator
```

## Permissions

Keep `AppProject` permissions restrictive.

Do not template arbitrary project/namespace/repo values from untrusted
developer-controlled sources.
