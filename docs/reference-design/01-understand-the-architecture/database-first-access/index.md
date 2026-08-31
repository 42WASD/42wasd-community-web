# Database-first F# access

Selected path:

```text
Atlas migration files
       ↓
PostgreSQL schema
       ↓
SQLProvider.PostgreSql / Npgsql
       ↓
typed F# persistence access
```

This means the **database schema is the persistence source of truth**.

It does **not** remove explicit browser DTOs.

## Credentials rule

Production code should never use build-time production credentials. Use a safe
schema/dev database or SQLProvider schema/offline mechanisms for type
generation/build workflows, and runtime connection strings/secrets for live
access.
