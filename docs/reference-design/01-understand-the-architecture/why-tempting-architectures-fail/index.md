# Why tempting architectures fail

Before writing pages, it is worth understanding why the obvious approaches do
not scale. Each problem below motivates a specific design decision in the
rest of this reference design.

## Problem 1 — One giant root `Model` does not scale

It starts innocently, then grows without bound:

```fsharp
type Model =
    {
        Page : Page
        MobileMenuOpen : bool
        Events : Event list
        Projects : Project list
        // ...
    }
```

The issue is not that Elmish cannot handle a large record — it is
**ownership**. When every field sits at the root, developers stop knowing:

```text
who owns this field?
which update is allowed to change it?
should it survive navigation?
is this canonical data or temporary UI state?
```

The better separation:

```text
App.Model
├── Page
└── Shared

Shared.Model
└── persistent cross-page state
```

## Problem 2 — One giant root `Msg` becomes an event dump

A flat union is fine for a small application, but it is not the long-term
shape. Prefer a small root namespace:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

Complexity then grows where it belongs — inside the page or feature, not at the
root.

## Problem 3 — Mirroring the visual tree with MVU creates boilerplate

Do not assume every visual element needs `Model`/`Msg`/`update`. Most
landing-page UI does not own independent state machines. A reusable card should
be a pure function:

```fsharp
module EventCard =

    let view (event : Event) (onOpen : EventId -> unit) =
        article {
            h3 { event.Title }
            button {
                on.click (fun _ -> onOpen event.Id)
                "View event"
            }
        }
```

The page owns the behavior; the card renders it.

## Problem 4 — Splitting globally by technical type scatters features

Avoid a repo shaped like `Models/`, `Messages/`, `Updates/`, `Views/`,
`Services/`, `Components/`. Adding an Events feature then touches six folders.
Prefer feature/page colocation:

```text
Pages/
└── Events/
    ├── Events.fs
    ├── EventCard.fs
    └── EventFilters.fs
```

## Problem 5 — Shared entities should not be copied into page models

Avoid having the same entity exist in several versions. Prefer a canonical,
normalized cache in `Shared`, and page-local references by ID:

```text
Shared.Events : Map<EventId, Event>
```

## Problem 6 — Calling HTTP/remoting directly from views breaks the MVU boundary

Always route effects through the message loop:

```text
button click -> Msg -> update -> Cmd -> remote call -> result Msg -> update
```

## Summary

These six failure modes are the reason the design separates **routing**,
**persistent state**, **page-local state**, **domain data**, **effects**, and
**rendering** — rather than collapsing them into one monolith or, conversely,
splitting them into excessive MVU components.