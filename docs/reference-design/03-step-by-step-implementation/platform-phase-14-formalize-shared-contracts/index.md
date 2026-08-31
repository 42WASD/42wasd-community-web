# Formalize Shared browser contracts

Do this before extracting multiple services (architecture doc Phase 14).

Create feature-specific contracts and version semantics.

Add:

```text
ClientVersion
ContractVersion
```

to bootstrap/handshake.

## Rules

```text
additive compatible changes preferred
breaking changes versioned
old client rejected explicitly if unsupported
```

## Acceptance

```text
[ ] client/server compile against same browser contracts
[ ] browser never receives password hash/internal moderation flags
[ ] persistence types do not leak
```
