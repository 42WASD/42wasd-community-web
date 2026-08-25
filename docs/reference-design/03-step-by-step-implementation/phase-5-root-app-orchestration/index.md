# Root app orchestration

Wire up the single root Elmish program.

## Goal

```text
one ProgramComponent
root Model with Page + Shared
root Msg with PageChanged, Shared, Local
```

## Implementation notes

- Enable Elmish tracing hooks for development:
  `withConsoleTrace`, `withErrorHandler`, `withTermination`.
- The Elmish message trace runs in the **browser console**, not the server
  terminal.
- These hooks are disabled by default in the template; add them manually.

## Verification

Open the app, act, and watch the trace in the browser console.