# Mobile path

## Phase 1

```text
make Bolero client an excellent responsive PWA
```

## Phase 2 if native product requirements appear

```text
Community.Contracts
Community.Domain
Community.Client.Core
```

become reusable libraries.

Native/.NET MAUI client can reuse:

```text
contracts
domain validation
API clients
sync algorithms
state concepts
```

## The rule

Do not assume all Bolero UI DSL rendering code converts automatically into
native MAUI controls.
