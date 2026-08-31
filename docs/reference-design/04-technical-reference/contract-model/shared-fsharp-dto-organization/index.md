# Shared F# DTO organization

Recommended:

```text
Community.Web.Shared/
  Common/
    Identifiers.fs
    Errors.fs
    Pagination.fs
    Versioning.fs

  Accounts/
    Contracts.fs

  Forum/
    Requests.fs
    Responses.fs
    Events.fs

  Servers/
    Contracts.fs

  Tournaments/
    Contracts.fs

  Realtime/
    Contracts.fs

  Media/
    Contracts.fs
```

## Do not create

```text
Models.fs
```

with 4,000 unrelated lines.

Prefer feature ownership.
