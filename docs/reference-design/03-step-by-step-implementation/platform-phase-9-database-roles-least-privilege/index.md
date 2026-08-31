# Database roles with least privilege

Use CloudNativePG managed role/DatabaseRole capabilities where appropriate
(architecture doc Phase 9).

At minimum separate:

```text
migration role
application runtime role
read-only/operator/debug role if needed
```

Do not give application runtime:

```text
SUPERUSER
CREATEDB
arbitrary role creation
```

Create schemas/ownership intentionally.

## Acceptance

```text
[ ] BFF/service credential cannot create superuser
[ ] migration role is used only by migration job
[ ] secrets are not committed
```
