# The one-sentence idea

Build one **Bolero `ProgramComponent` / Elmish program** whose root model stays
small, keep cross-page data in a persistent `Shared.Model`, keep temporary
state close to the page or feature that owns it, use Bolero routing and
`PageModel<'T>` for route/page state, use `Cmd` for effects, and keep ordinary
visual elements as pure view functions.

## The mental model

```text
                         ┌───────────────────────────┐
                         │        BROWSER / URL      │
                         └─────────────┬─────────────┘
                                       │
                                       ▼
                              ┌─────────────────┐
                              │ Bolero Router   │
                              │ Page DU         │
                              └────────┬────────┘
                                       │
                                       ▼
                         ┌───────────────────────────┐
                         │    ONE ELMISH PROGRAM     │
                         │ ProgramComponent<M, Msg>  │
                         └─────────────┬─────────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
                App.Model          Shared.Model        Page state
                orchestration      persistent          temporary /
                                    cross-page          route-owned
                    │                  │                  │
                    └──────────────────┼──────────────────┘
                                       │
                                       ▼
                                    update
                                       │
                           ┌───────────┴───────────┐
                           │                       │
                           ▼                       ▼
                         Model                    Cmd
                                                   │
                                                   ▼
                                        remoting / browser /
                                        API / other effects
                                                   │
                                                   ▼
                                                  Msg
                                       │
                                       ▼
                                     view
                                       │
                         ┌─────────────┼─────────────┐
                         │             │             │
                         ▼             ▼             ▼
                       page          feature       shared UI
                       views         views         functions
```

## The crucial rule

```text
route state != shared domain state
page-local state != persistent application state
visual component != independent MVU program
effect != direct mutation
framework shell != application architecture
```

Everything else in this design is an unpacking of this one idea.