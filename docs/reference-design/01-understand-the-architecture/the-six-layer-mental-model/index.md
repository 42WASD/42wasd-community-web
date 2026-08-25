# The six-layer mental model

The complete mental model decomposes the application into six layers. Each layer
answers one focused question.

## 2.1 APP — “What composes the entire application?”

Responsibilities:

```text
one ProgramComponent
root Model
root Msg
root update
router attachment
top-level view
dependency wiring
```

Target shape:

```fsharp
type Model =
    {
        Page : Page
        Shared : Shared.Model
    }

type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

The root should stay boring. That is a feature.

## 2.2 ROUTING — “Which routable state is currently active?”

Use a Bolero endpoint DU:

```fsharp
type Page =
    | [<EndPoint "/">]
      Home

    | [<EndPoint "/about">]
      About

    | [<EndPoint "/events">]
      Events of PageModel<Events.Model>
```

Static pages need no local model. Stateful pages can carry `PageModel<'T>`
(for search/filters/pagination) — use route parameters for state that should be
encoded in the URL.

## 2.3 SHARED STATE — “What must persist or be reused across pages?”

```text
authenticated user
community metadata
event/project/member entity caches
feature flags
persistent preferences
```

Example:

```fsharp
module Shared

type Model =
    {
        CurrentUser : User option
        Community : RemoteData<CommunityInfo>
        Events : RemoteData<Map<EventId, Event>>
    }
```

This is the cross-page source of truth.

## 2.4 PAGE / FEATURE STATE — “What only makes sense while this feature is active?”

```fsharp
module Events

type Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
        SelectedEventId : EventId option
    }
```

This state belongs to the Events experience and should not pollute
`App.Model`.

## 2.5 EFFECTS — “What touches the impure world?”

```text
Bolero Remoting, HTTP APIs, browser storage, clipboard, timers, analytics,
GitHub API, Discord/community API
```

Trigger them from Elmish commands:

```text
pure update -> new Model | Cmd -> impure work -> Msg
```

The module that understands the returned result owns the result message.

## 2.6 VIEW — “What renders the current model?”

Views consume data and dispatch messages. They should not become a second
state architecture.

```text
App.view
├── Layout.view
└── Page.view
     ├── Home.view
     ├── Events.view
     │    ├── EventCard.view
     │    └── EventFilters.view
     └── Projects.view
```

Keep Hero, Navbar, Footer, Card, Badge, Button, Stats section, Sponsor grid as
ordinary functions/modules unless they truly own independent behavior.