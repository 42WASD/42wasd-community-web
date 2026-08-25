# Nested page messages

Compose page-local `Msg` into the root message with `Cmd.map`.

## Goal

```fsharp
type Msg =
    | PageChanged of Page
    | Shared of Shared.Msg
    | Local of LocalMsg
```

## How

- Each page owns its local `Model`/`Msg`/`update`.
- The root wraps the active page's `Msg`.
- Child effects are lifted with `Cmd.map`.

## Rule

> A local MVU boundary is for genuine feature state, not for every visual
> component.