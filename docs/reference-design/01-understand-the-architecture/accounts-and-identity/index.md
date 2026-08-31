# Accounts and identity model

Use one canonical authenticated platform account.

Recommended:

```text
AuthAccount
    │
    ├── External login: Google
    ├── External login: Discord
    ├── External login: Steam if used as auth
    └── optional local/passkey credentials

    ├── Persona A
    └── Persona B
```

## The key distinction

External account identity is not the same thing as display persona.

## Merge rules

Do not merge two accounts merely because provider emails match.

A login credential such as `(provider, provider_user_id)` should normally map
unambiguously to one AuthAccount.

If a social identity should be visible on several personas/accounts as
information, model that separately as a **linked external profile**, not as an
ambiguous login credential.

## First implementation

Selected first implementation:

```text
ASP.NET Core Identity
        ↓
EF Core Identity stores
        ↓
Npgsql.EntityFrameworkCore.PostgreSQL
        ↓
PostgreSQL identity schema
```

Keep this EF Core usage confined to the Accounts/Identity bounded context (see
Rejected foundations for the reasoning).
