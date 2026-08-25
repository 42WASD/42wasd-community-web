---
phase: 03-step-by-step-implementation/phase-1-create-the-solution
---

**Phase 1 complete** — the Bolero solution was scaffolded from the official
template, built cleanly, and verified to run locally with the default route
rendering.

### What was created

```text
Community.Web.sln
global.json                          # pins .NET SDK 10.0.111 (rollForward: disable)
src/
├── Community.Web.Client/            # classic Bolero Elmish WASM client
│   ├── Community.Web.Client.fsproj
│   ├── Main.fs                      # ProgramComponent + Router.infer (template baseline)
│   ├── Startup.fs
│   ├── MyApp.bolero.css
│   └── wwwroot/ (main.html, css, favicon)
└── Community.Web.Server/            # ASP.NET host + remoting
    ├── Community.Web.Server.fsproj
    ├── Startup.fs
    ├── Index.fs
    ├── BookService.fs
    └── data/books.json
```

### How it was scaffolded

```bash
dotnet new bolero-app -n Community.Web -o . --render LegacyWebAssembly
```

- **Render mode `LegacyWebAssembly`** — the classic Bolero hosting path
  (`ProgramComponent`, `Router.infer`, `PageModel<'T>`, `Program.withRouter`,
  remoting) that the reference design's canonical patterns are written for.
  Chosen after confirming in `thirdparty/Bolero-Template` source that the
  classic API is wired only for the non-`isInteractive` modes, and that the
  `Interactive*` modes use the new .NET Blazor renderer pipeline instead.
- **`global.json`** pins `10.0.111` with `rollForward: disable` so builds are
  reproducible regardless of the SDK installed on a developer machine.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
# run locally (Development):
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Community.Web.Server
```

HTTP checks against `http://localhost:5023`:

| Check | Result |
|---|---|
| `/` (default route) | 200 — `Bolero Application` |
| `_framework/blazor.webassembly.js` | 200 — client loader served |
| client WASM payload (hashed) | 200 — client boots |
| `/counter` (routed page fallback) | 200 — refresh on a routed page works |

Note: in .NET 8+ Blazor WASM the framework assets are served under **hashed**
names; raw `_framework/blazor.boot.json` / `Community.Web.Client.dll` return
404 by design. The client boots from the runtime manifest.

### Acceptance (from verified design)

- [x] solution restores
- [x] server/client run locally
- [x] default route renders
- [x] refresh on a routed page works

### Next

Phase 2 will reshape the client into `App/ State/ Pages/ Ui/ Infrastructure/`
folders. The template baseline is left intact for now; later phases reshape it
without rewriting the framework machinery.