---
phase: 03-step-by-step-implementation/phase-5-root-app-orchestration
---

**Phase 5 complete** — the single root Elmish program is wired up with
dev-only tracing hooks.

### What this phase required

From the reference spec:

- One `ProgramComponent`.
- Root `Model` with `Page` + shared state.
- Root `Msg` with page-change, shared, and local messages.
- Dev-only Elmish tracing hooks: `withConsoleTrace`, `withErrorHandler`,
  `withTermination`.
- The Elmish message trace runs in the **browser console**, not the server
  terminal (template disables these hooks by default; add them manually).

### Implementation

The root `ProgramComponent` (`MyApp` in `Main.fs`) already existed from the
template/Phase 1, holding `ProgramComponent<Model, Message>` with the root
`Model` (containing `Page`) and root `Message`. The `#if DEBUG` block now adds
the three dev hooks plus hot-reload:

```fsharp
let program =
    Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
    |> Program.withRouter router
#if DEBUG
program
|> Program.withConsoleTrace
|> Program.withErrorHandler (fun (msg, exn) ->
    printfn $"Elmish error after %A{msg}: {exn}")
|> Program.withTermination
    (fun _ -> false)
    (fun _ -> printfn "Program terminated.")
|> Program.withHotReload
#else
program
#endif
```

Notes:

- `withConsoleTrace` logs every message + state change to the browser console.
- `withErrorHandler` surfaces any `(msg, exn)` raised by the update/view.
- `withTermination` takes a predicate `'model -> bool` (not an option). The
  predicate is a pass-through here; the hook is installed for later use.
- All hooks are `#if DEBUG`-gated so release builds stay lean.
- Verified against `thirdparty/Bolero` + the restored `Elmish.dll` (4.0.1):
  the hooks are Elmish `ProgramModule` members (not Bolero's), and the exact
  signatures are `withConsoleTrace : Program`, `withErrorHandler : (('msg*exn)
  -> unit) -> Program`, `withTermination : ('model -> bool) -> ('model -> unit)
  -> Program`.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live check (Development, browser console capture via Playwright):

```
log: Initial state:: { page = Home
log:   games = None ... }
log: New message:: GetSignedInAs
log: Updated state:: { page = Home ... }
log: New message:: RecvSignedInAs None
log: Updated state:: { ... }
```

The `withConsoleTrace` output appears in the browser console exactly as the
reference describes. (The 401 on `/api/getUsername` is expected: `Cmd.OfAuthor
ized` returns 401 when signed out, correctly yielding `RecvSignedInAs None`.)

### Acceptance (from reference spec)

- [x] Single root `ProgramComponent`
- [x] Root `Model` with `Page` + shared state
- [x] Root `Msg` with page-change / shared / local messages
- [x] `withConsoleTrace` active (browser console trace confirmed)
- [x] `withErrorHandler` + `withTermination` added (dev-only)
- [x] Hooks disabled in release (`#if DEBUG`)

### Next

Phase 6 — shared application state.