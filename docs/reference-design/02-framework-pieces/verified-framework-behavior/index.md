# Verified framework behavior

These are the current, verified facts about Bolero, Elmish, and supporting
frameworks as of 2026-08-25. They directly support the architecture in this
guide.

## Bolero — main repository

```text
Bolero integrates Elmish
routing binds URL to a field in the Elmish model
remoting exposes asynchronous server functions
current main-repository getting-started requirement: .NET SDK 10.0
```

> **Documentation mismatch:** the Bolero main README says .NET 10, but some
> older/template/docs surfaces still mention .NET 8.
>
> **Decision:** follow the current main Bolero repository and tested template
> output; pin the SDK in `global.json`; do not copy an old SDK number from an
> outdated page blindly.

## Bolero routing

```text
endpoint type commonly represented by F# union
endpoint stored in Elmish model
PageChanged message updates it
Router.infer binds route <-> model
PageModel<'T> is excluded from URL
Router.inferWithModel supplies defaults for PageModel
```

## Bolero `ProgramComponent`

```text
ProgramComponent<'model, 'msg>
```

is the main Bolero component whose content is defined by an Elmish program.

Selected rule:

> One primary `ProgramComponent` for the application. Do not create independent
> program stores for ordinary page fragments.

## Bolero `ElmishComponent`

```text
ElmishComponent<'model, 'msg>
```

receives a sub-model and only re-renders when that model changes.

Interpretation:

> rendering boundary != state ownership boundary. Use it for rendering
> isolation/optimization when needed, not as justification for local MVU state
> on every visual component.

## Elmish parent-child composition

```text
child Model
child Msg
child update
parent wraps child Msg
parent uses Cmd.map
```

This is the mechanism used when a feature genuinely deserves its own MVU
boundary. It is not a requirement to make every component a child application.

## Elm application structure guidance

Elm's official guide recommends page-centered modules containing `Model`,
`init`, `update`, `view`, helpers — and warns against globally splitting code
into `Model/`, `View/`, `Update/`. It also warns against translating the visual
component tree directly into architectural components.

## Framework-agnostic feature organization

Angular guidance recommends organizing by feature areas and avoiding top-level
directories based purely on code type. Redux guidance likewise recommends
feature folders and organizing state around data/functionality rather than UI
components. These are not Bolero rules but independently support the same
feature-oriented repository principle.