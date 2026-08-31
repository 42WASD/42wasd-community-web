# PWA/mobile release

First mobile experience (architecture doc Phase 59):

```text
responsive PWA
installable
service worker
IndexedDB
same Bolero client
```

Only after product demand exists:

```text
Community.Contracts
Community.Domain
Community.Client.Core
```

are shared with a .NET MAUI client.

Do not assume Bolero UI DSL becomes native MAUI UI automatically.

Reuse:

```text
contracts
domain rules
sync engine
API client
validation
state concepts
```

and decide rendering strategy separately.
