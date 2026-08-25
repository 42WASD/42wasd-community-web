---
phase: 03-step-by-step-implementation/phase-2-repository-structure
---

**Phase 2 complete** — the repository was restructured into the feature-oriented
layout from the reference design, and both client and server now depend on a
shared contracts project.

### New structure

```text
src/
├── Community.Web.Shared/          # shared contracts (no server/client deps)
│   ├── Community.Web.Shared.fsproj
│   └── Contracts/
│       ├── Books.fs               # canonical Book entity
│       └── CommunityApi.fs        # BookService remoting contract
├── Community.Web.Client/
│   ├── Community.Web.Client.fsproj
│   ├── App/
│   │   └── App.fs                 # orchestration: Page, Model, Message, update, router, init
│   ├── Ui/
│   │   └── Layout.fs              # cross-feature UI (shared layout template + views)
│   ├── Main.fs                    # root ProgramComponent (ProgramRouter + program wiring)
│   ├── Startup.fs
│   └── wwwroot/
└── Community.Web.Server/
    ├── Community.Web.Server.fsproj
    ├── Startup.fs
    ├── Index.fs
    ├── BookService.fs             # server-side remoting handler
    └── data/books.json
```

### Design decisions

- **`Community.Web.Shared`**: holds the `Book` entity and the `BookService`
  remoting contract. This is the single contract type both client and server
  compile against — no client<->server circular dependency.
- **Dependency direction**: `Community.Web.Shared` is depended on by both
  Client and Server; Client is referenced by Server (to host the WASM app);
  Server does NOT reference Client's internal logic.
- **App/Ui split**: the root `Model`/`Msg`/`update`/router live in `App/`;
  cross-feature UI (the shared template + view composition) lives in `Ui/`.
  Page-specific UI will move beside its page in a later phase (feature-owned
  UI), keeping the global `Ui/` folder deliberately small per the reference.
- **No top-level `Model/`/`Msg/`/`Update/` split** — state, messages, and views
  are organized by concern/feature, not by technical type.
- **Start shallow**: no empty placeholder directories (`State/`, `Pages/`,
  `Infrastructure/`) were created for hypothetical features. They will be added
  in later phases when they have real content.

### Remoting contract moved to Shared

The template's `BookService` + `Book` were lifted out of `Client.Main` into
`Community.Web.Shared/Contracts`. The server's `RemoteHandler` now inherits
`RemoteHandler<Community.Web.Shared.Contracts.BookService>` (the shared
contract), so it no longer depends on the client's internal `Main` type.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live checks against `http://localhost:5023` (Development):

| Route | Result |
|---|---|
| `/` | 200 |
| `/counter` | 200 |
| `/data` | 200 |
| `/books/getBooks` (remoting) | 200 |

Both client and server reference the shared project successfully.

### Acceptance (from reference design)

- [x] Shared has no server-only or client-only dependencies
- [x] Both client and server depend on `Community.Web.Shared` contracts
- [x] Feature folders, not top-level `Model/View/Update` split
- [x] Each directory has a clear ownership rule
- [x] No global `Models/Msgs/Updates` directories
- [x] No feature-specific UI dumped into `Ui/`

### Next

Phase 3 will define the shared domain types (`Community.Web.Shared/Domain` +
remoting service interfaces) needed for the first community slice.