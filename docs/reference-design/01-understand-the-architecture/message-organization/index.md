# Message organization

The root message is an orchestration boundary, not an event dump.

## Root messages

Keep the root small:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

The root update is a dispatcher/orchestrator.

## Shared messages

Example:

```fsharp
module Shared

type Msg =
    | SessionMsg of Session.Msg
    | EventsDataMsg of EventsData.Msg
    | ProjectsDataMsg of ProjectsData.Msg
```

Do **not** introduce these sub-unions immediately if `Shared.Msg` only has a few
cases. Split when the ownership boundary is real.

## Page messages

Example:

```fsharp
module Page

type Msg =
    | EventsMsg of Events.Msg
    | ProjectsMsg of Projects.Msg
    | MembersMsg of Members.Msg
```

Static pages do not need a message case.

## Feature messages

Example:

```fsharp
module Events

type Msg =
    | SearchChanged of string
    | CategoryChanged of Category option
    | NextPage
    | PreviousPage
    | EventSelected of EventId
```

These messages are meaningful inside the feature.

## Cross-boundary actions

For a small app, the parent can translate page behavior directly. When
cross-feature coordination grows, prefer explicit outputs/effects over making a
child depend on the entire parent:

```fsharp
type Events.Effect =
    | NavigateToEvent of EventId
    | RefreshEvents
```

Then the parent/App interprets the `Effect`. Do not start here unless needed;
use it when parent/child coupling becomes visible.