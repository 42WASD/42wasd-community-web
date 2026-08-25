# Repository structure

Establish the feature-oriented repository layout.

## Goal

```text
src/
  Community.Web.Client/
  Community.Web.Server/
  Community.Shared/
tests/
scripts/
docs/
```

## Rules

- Shared has no server-only or client-only dependencies.
- Both client and server depend on `Community.Shared` contracts.
- Feature folders, not top-level `Model/`/`View/`/`Update/` split.

## Verification

```bash
dotnet build
```

Both sides reference the shared project successfully.