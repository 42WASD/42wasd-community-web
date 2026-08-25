# Shared domain types

Define the domain contract types in `Community.Shared`.

## Goal

```text
Domain/
  Event.fs
  Project.fs
  Member.fs
  ...
Remoting/
  service interfaces
```

## Rules

- These types are the single contract between client and server.
- They carry no server or client framework dependencies.
- IDs, entity records, and request/response types live here.

## Verification

```bash
dotnet build
```

Client and server compile against the same shared domain types.