# Replace placeholder authentication

Create Accounts/Identity persistence (architecture doc Phase 13).

## Selected first implementation

```text
ASP.NET Core Identity
        ↓
EF Core Identity stores
        ↓
Npgsql.EntityFrameworkCore.PostgreSQL
        ↓
PostgreSQL identity schema
```

Keep this EF Core usage confined to the Accounts/Identity bounded context. Do
**not** hand-write a SQLProvider-backed custom Identity store for the first
production version; Microsoft's custom-store path requires implementing the
relevant `IUserStore` plus optional login/token/claims/role/lockout/
security-stamp interfaces, which is unnecessary security-sensitive code when a
mature Npgsql EF Core provider already exists.

## Initial login providers

```text
Google
Discord
```

Then linked integrations:

```text
Steam
FACEIT
YouTube
```

can be added according to product requirements.

## Model

```text
AuthAccount
Persona
ExternalLoginCredential
LinkedExternalProfile
```

## Acceptance

```text
[ ] external provider login maps to one canonical account
[ ] same email from another provider does not auto-merge without proof/policy
[ ] account can link additional provider while already authenticated
[ ] provider removal does not orphan content ownership
[ ] session cookie/security works behind proxy/ingress
```
