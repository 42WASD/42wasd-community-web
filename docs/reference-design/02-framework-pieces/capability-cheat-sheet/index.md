# Capability cheat sheet

| Concern | Recommended owner/tool |
|---|---|
| Root MVU program | Bolero `ProgramComponent<Model, Msg>` |
| Routing | Bolero `Router.infer` / `Router.inferWithModel` |
| Route representation | F# `Page` DU |
| Temporary route/page state | `PageModel<'T>` |
| Persistent cross-page state | `Shared.Model` |
| Local feature logic | feature/page `Model`, `Msg`, `update`, `view` |
| Async effects | Elmish `Cmd` |
| Child command lifting | `Cmd.map` |
| Server calls | Bolero Remoting or explicit HTTP client |
| Shared client/server contracts | `Community.Shared` |
| Rendering optimization | Bolero `ElmishComponent` where justified |
| Pure visual components | normal F# view functions/modules |
| Canonical entity cache | normalized maps keyed by IDs |
| Tests | pure `update` tests + routing + integration tests |

## How to read this

The cheat sheet is the **reuse-first** map: before writing new F# functions,
check whether Bolero/Elmish already provides the tool on this table.