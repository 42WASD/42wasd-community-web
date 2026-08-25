# Verified Bolero / Elmish Community Web Architecture — Explained Edition

**Audit date:** 2026-08-25  
**Purpose:** define a clean, maintainable, feature-oriented architecture for a developer community or gaming community website built with **Bolero + Elmish**, with a small root MVU loop, explicit state ownership, type-safe routing, page-local state, reusable UI, server remoting, and a phased implementation path that does not collapse into one giant `Model`, `Msg`, or `update`.

> **Important scope note:** This is an application-architecture and implementation guide. It deliberately separates **routing**, **persistent application state**, **page-local state**, **domain data**, **effects/remoting**, and **rendering**. Treating all of these as one global `Model`/`Msg` file creates a monolith; creating an MVU component for every visual element creates the opposite problem: excessive nesting and boilerplate.

---

# Part I — Understand the architecture before writing pages

## 0. The one-sentence idea

Build one **Bolero `ProgramComponent` / Elmish program** whose root model stays small, keep cross-page data in a persistent `Shared.Model`, keep temporary state close to the page or feature that owns it, use Bolero routing and `PageModel<'T>` for route/page state, use `Cmd` for effects, and keep ordinary visual elements as pure view functions.

The mental model is:

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

The crucial rule is:

```text
route state != shared domain state
page-local state != persistent application state
visual component != independent MVU program
effect != direct mutation
framework shell != application architecture
```

---

# 1. Why the tempting architectures are wrong

## Problem 1 — One giant root `Model` does not scale

This starts innocently:

```fsharp
type Model =
    {
        Page : Page
        MobileMenuOpen : bool
        Events : Event list
        Projects : Project list
    }
```

Then grows into:

```text
App.Model
├── current page
├── menu state
├── events
├── event filters
├── event pagination
├── event selection
├── projects
├── project filters
├── members
├── login form
├── account form
├── settings
├── dialogs
├── loading flags
├── errors
└── ...
```

The issue is not that Elmish cannot handle a large record.

The issue is **ownership**.

When every state field is placed at the root, developers eventually stop knowing:

```text
who owns this field?
which update is allowed to change it?
should it survive navigation?
is this canonical data or temporary UI state?
```

The better separation is:

```text
App.Model
├── Page
└── Shared

Page
└── current page + page-local state

Shared.Model
└── persistent cross-page state
```

---

## Problem 2 — One giant root `Msg` becomes an event dump

Avoid this:

```fsharp
type Msg =
    | PageChanged of Page
    | OpenMenu
    | CloseMenu
    | LoadEvents
    | EventsLoaded of Event list
    | SearchEvents of string
    | FilterEvents of Category
    | NextEventPage
    | SelectEvent of EventId
    | LoadProjects
    | ProjectsLoaded of Project list
    | SearchProjects of string
    | SelectProject of ProjectId
    | LoginEmailChanged of string
    | LoginPasswordChanged of string
    | Login
    | Logout
    // ...
```

A flat union is fine for a small application.

It is not the desired long-term shape.

Prefer a small root namespace of messages:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

Then complexity grows where it belongs:

```text
App.Msg
│
├── PageChanged
├── SharedMsg
│     ├── SessionMsg
│     ├── EventsDataMsg
│     └── ProjectsDataMsg
│
└── PageMsg
      ├── EventsMsg
      ├── ProjectsMsg
      └── AccountMsg
```

The root remains orchestration.

---

## Problem 3 — Mirroring the visual tree with MVU creates boilerplate

Do not assume:

```text
there is a Navbar
→ Navbar.Model
→ Navbar.Msg
→ Navbar.update

there is a Hero
→ Hero.Model
→ Hero.Msg
→ Hero.update

there is an EventCard
→ EventCard.Model
→ EventCard.Msg
→ EventCard.update
```

Most landing-page UI does not own independent state machines.

A reusable card should normally be:

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

The page or feature owns the behavior.

The card renders it.

---

## Problem 4 — Splitting globally by technical type creates scattered features

Avoid a large repository shaped like:

```text
src/
├── Models/
├── Messages/
├── Updates/
├── Views/
├── Services/
└── Components/
```

Adding an Events feature then touches:

```text
Models/Events.fs
Messages/Events.fs
Updates/Events.fs
Views/Events.fs
Services/Events.fs
Components/EventCard.fs
```

The feature is spread across the repository.

Prefer feature/page colocation:

```text
Pages/
└── Events/
    ├── Events.fs
    ├── EventCard.fs
    └── EventFilters.fs
```

or, while the feature is still small:

```text
Pages/
└── Events.fs
```

Grow the folder only when the feature earns it.

---

## Problem 5 — Shared entities should not be copied into page models

Bad:

```text
Shared.Events
EventsPage.Events
HomePage.UpcomingEvents
EventDetails.Event
```

Now the same event can exist in four versions.

Prefer:

```text
Shared.Events : Map<EventId, Event>
```

and page-local references:

```fsharp
type Events.Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
        SelectedEventId : EventId option
    }
```

Then:

```text
                 Shared.Events
                    /      \
                   /        \
                  ▼          ▼
               Home       Events
                            │
                            └── SelectedEventId
```

The canonical entity exists once.

---

## Problem 6 — Calling HTTP/remoting directly from views breaks the MVU boundary

Bad mental model:

```text
button click
   ↓
call remote API directly
   ↓
mutate local state
```

Preferred:

```text
button click
   ↓
Msg
   ↓
update
   ↓
Cmd
   ↓
remote call
   ↓
result Msg
   ↓
update
```

This keeps state transitions inspectable and testable.

---

# 2. The complete mental model: six layers

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

The root should stay boring.

That is a feature.

---

## 2.2 ROUTING — “Which routable state is currently active?”

Use a Bolero endpoint DU.

Example:

```fsharp
type Page =
    | [<EndPoint "/">]
      Home

    | [<EndPoint "/about">]
      About

    | [<EndPoint "/events">]
      Events of PageModel<Events.Model>

    | [<EndPoint "/projects">]
      Projects of PageModel<Projects.Model>

    | [<EndPoint "/members">]
      Members of PageModel<Members.Model>
```

Static pages need no local model:

```text
Home
About
```

Stateful pages can carry `PageModel<'T>`:

```text
Events   -> search / filters / pagination
Projects -> filters / selected tag
Account  -> temporary form state
```

Use route parameters for state that should be encoded in the URL.

Use `PageModel<'T>` for state that belongs to the active page but should not be encoded into the URL.

---

## 2.3 SHARED STATE — “What must persist or be reused across pages?”

Examples:

```text
authenticated user
community metadata
event entity cache
project entity cache
member entity cache
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
        Projects : RemoteData<Map<ProjectId, Project>>
    }
```

This is the cross-page source of truth.

Do not put temporary search boxes or page numbers here unless they genuinely need to survive page changes.

---

## 2.4 PAGE / FEATURE STATE — “What only makes sense while this feature is active?”

Example:

```fsharp
module Events

type Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
        SelectedEventId : EventId option
    }

type Msg =
    | SearchChanged of string
    | CategoryChanged of Category option
    | NextPage
    | PreviousPage
    | EventSelected of EventId
```

This state belongs to the Events experience.

It should not pollute `App.Model`.

---

## 2.5 EFFECTS — “What touches the impure world?”

Examples:

```text
Bolero Remoting
HTTP APIs
browser storage
clipboard
timers
analytics
GitHub API
Discord/community API
```

Trigger them from Elmish commands.

Conceptually:

```text
pure update
   │
   ├── new Model
   │
   └── Cmd
         ↓
      impure work
         ↓
       Msg
```

The module that understands the returned result should own the result message.

---

## 2.6 VIEW — “What renders the current model?”

Views consume data and dispatch messages.

They should not become a second state architecture.

Typical hierarchy:

```text
App.view
│
├── Layout.view
│
└── Page.view
     │
     ├── Home.view
     ├── Events.view
     │    ├── EventCard.view
     │    └── EventFilters.view
     └── Projects.view
```

Keep:

```text
Hero
Navbar
Footer
Card
Badge
Button
Stats section
Sponsor grid
```

as ordinary functions/modules unless they truly own meaningful independent behavior.

---

# 3. The selected state ownership model

Use this decision table.

| State | Owner |
|---|---|
| Current route | `Page` |
| Route parameters | `Page` DU |
| Temporary page state | `PageModel<'T>` / page `Model` |
| Canonical entities reused by many pages | `Shared.Model` |
| Authenticated user | `Shared.Model` |
| Community configuration | `Shared.Model` |
| Search text used only on Events page | `Events.Model` |
| Event filter | `Events.Model` |
| Selected event already present in shared cache | store `EventId`, not another `Event` |
| Static hero text | view/module constant, not Elmish state |
| Pure card props | function arguments |
| Remote call in progress | model owned by the feature/data slice that requested it |
| Server-returned canonical data | shared state if reused across routes |
| Login/password fields | page-local model; discard after leaving page |
| Dark theme persisted app-wide | shared/app UI state |
| Modal that only exists in one page | page-local model |

The rule is:

```text
put state at the lowest level that fully owns it
but no lower than the level at which it must be shared
```

---

# 4. Message organization

## 4.1 Root messages

Keep the root small:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

The root update is a dispatcher/orchestrator.

---

## 4.2 Shared messages

Example:

```fsharp
module Shared

type Msg =
    | SessionMsg of Session.Msg
    | EventsDataMsg of EventsData.Msg
    | ProjectsDataMsg of ProjectsData.Msg
```

Do not introduce these sub-unions immediately if `Shared.Msg` only has five cases.

Split when the ownership boundary is real.

---

## 4.3 Page messages

Example:

```fsharp
module Page

type Msg =
    | EventsMsg of Events.Msg
    | ProjectsMsg of Projects.Msg
    | MembersMsg of Members.Msg
```

Static pages do not need a message case.

---

## 4.4 Feature messages

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

---

## 4.5 Cross-boundary actions

For a small app, the parent can translate page behavior directly.

When cross-feature coordination grows, prefer explicit outputs/effects over making a child depend on the entire parent.

Example concept:

```fsharp
type Events.Effect =
    | NavigateToEvent of EventId
    | RefreshEvents
```

Then:

```text
Events.update
   ↓
local model + local Cmd + Effect
   ↓
Page/App interprets Effect
   ↓
navigation or Shared.Msg
```

Do not start here unless needed.

Use it when parent/child coupling becomes visible.

---

# 5. Page organization: route is not always feature

For a small site:

```text
Pages/
├── Home.fs
├── Events.fs
├── Projects.fs
├── Members.fs
└── About.fs
```

is enough.

If Events grows into:

```text
/events
/events/upcoming
/events/past
/events/{id}
/events/{id}/register
```

do not automatically create five unrelated top-level features.

Promote Events into a feature folder:

```text
Features/
└── Events/
    ├── Events.fs
    ├── ListPage.fs
    ├── DetailPage.fs
    ├── RegistrationPage.fs
    ├── EventCard.fs
    └── EventFilters.fs
```

The rule is:

```text
route != feature
```

Many routes may belong to one coherent domain feature.

---

# 6. Recommended repository structure

For a full-stack Bolero application:

```text
community-platform/
├── README.md
├── global.json
├── src/
│   │
│   ├── Community.Shared/
│   │   ├── Domain/
│   │   │   ├── Common.fs
│   │   │   ├── Community.fs
│   │   │   ├── Event.fs
│   │   │   ├── Project.fs
│   │   │   └── Member.fs
│   │   │
│   │   └── Contracts/
│   │       └── CommunityApi.fs
│   │
│   ├── Community.Client/
│   │   ├── App/
│   │   │   ├── Routing.fs
│   │   │   └── App.fs
│   │   │
│   │   ├── State/
│   │   │   └── Shared.fs
│   │   │
│   │   ├── Pages/
│   │   │   ├── Page.fs
│   │   │   ├── Home.fs
│   │   │   ├── About.fs
│   │   │   ├── Events.fs
│   │   │   ├── Projects.fs
│   │   │   └── Members.fs
│   │   │
│   │   ├── Ui/
│   │   │   ├── Layout.fs
│   │   │   ├── Navbar.fs
│   │   │   ├── Footer.fs
│   │   │   └── Primitives.fs
│   │   │
│   │   ├── Infrastructure/
│   │   │   ├── CommunityApi.fs
│   │   │   └── Browser.fs
│   │   │
│   │   ├── Main.fs
│   │   └── wwwroot/
│   │
│   └── Community.Server/
│       ├── Program.fs
│       ├── Services/
│       │   ├── CommunityService.fs
│       │   └── EventService.fs
│       └── Persistence/
│
└── tests/
    ├── Community.Client.Tests/
    └── Community.Server.Tests/
```

The dependency direction is:

```text
Community.Shared
      ↑
      │
Client + Server

Client:
Domain/contracts
      ↑
Infrastructure / State
      ↑
Pages / features
      ↑
App composition
```

Do not create circular feature dependencies.

---

# 7. Why `Ui/` is deliberately small

A global `Components/` directory easily becomes:

```text
Components/
├── EventCard
├── EventFilter
├── ProjectEditor
├── MemberProfile
├── LoginForm
├── ...
```

and silently recreates folder-by-type organization.

Use global `Ui/` only for truly cross-feature UI:

```text
Layout
Navbar
Footer
Button/primitives
Modal shell
typography helpers
design tokens
```

Keep feature-specific UI beside the feature:

```text
Pages/Events/
├── Events.fs
├── EventCard.fs
└── EventFilters.fs
```

This is the cleanest compromise between feature colocation and reuse.

---

# 8. Developer community vs gaming community

The architecture does not change.

## Developer community domain

```text
Community.Shared/Domain/
├── Event.fs
├── Project.fs
├── Member.fs
├── Organization.fs
└── Sponsor.fs
```

Pages/features:

```text
Home
Events
Projects
Members
About
```

Typical home sections:

```text
Hero
Stats
Upcoming events
Featured projects
Community members
Sponsors
Join Discord / GitHub
```

---

## Gaming community domain

```text
Community.Shared/Domain/
├── Game.fs
├── GameServer.fs
├── Tournament.fs
├── Player.fs
├── Team.fs
└── News.fs
```

Pages/features:

```text
Home
Servers
Tournaments
Games
Members
About
```

Typical home sections:

```text
Hero
Online/community stats
Games we play
Active servers
Upcoming tournaments
Latest news
Join Discord
```

Same architecture.

Different vocabulary and theme.

---

# 9. Design language is independent from MVU architecture

Recommended developer-community direction:

```text
minimal
clean typography
strong spacing
neutral surfaces
subtle borders
dark/light theme
small accent palette
accessible contrast
simple cards
```

Gaming-community direction:

```text
dark-first
stronger artwork
larger hero imagery
accent lighting
server-status indicators
tournament cards
controlled glass/overlay surfaces
```

Do not let the visual theme dictate state ownership.

---

# Part II — How to interpret the framework pieces

# 10. Capability cheat sheet

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

---

# 11. Maturity tiers

## Tier A — foundation

Implement immediately:

```text
official Bolero project
one ProgramComponent
Page DU
root Model / Msg
root update
Home + About
shared layout
```

---

## Tier B — state architecture

Add next:

```text
Shared.Model
RemoteData
page-local Models
nested Page.Msg
Cmd.map
```

---

## Tier C — real community data

Add after the shell is stable:

```text
Events
Projects / Servers
Members
server remoting
loading/error states
normalized entity caches
```

---

## Tier D — product polish

Add later:

```text
authentication
account state
theme persistence
analytics
SEO/static rendering decisions
render optimization
```

Do not optimize the architecture for features that do not exist yet.

---

# 12. Current verification notes — 2026-08-25

## Bolero

Current Bolero repository documentation states:

```text
Bolero integrates Elmish
routing binds URL to a field in the Elmish model
remoting exposes asynchronous server functions
current main-repository getting-started requirement: .NET SDK 10.0
```

Important documentation mismatch:

```text
Bolero main GitHub README -> .NET SDK 10.0
some older/template/docs surfaces -> still mention .NET 8
```

Decision:

```text
follow the current main Bolero repository / tested template output
pin the SDK in global.json
do not copy an old SDK number from an outdated page blindly
```

---

## Bolero routing

Current routing documentation verifies:

```text
endpoint type commonly represented by F# union
endpoint stored in Elmish model
PageChanged message updates it
Router.infer binds route <-> model
PageModel<'T> is excluded from URL
Router.inferWithModel supplies defaults for PageModel
```

This directly supports the architecture in this guide.

---

## Bolero `ProgramComponent`

Current documentation describes:

```text
ProgramComponent<'model, 'msg>
```

as the main Bolero component whose content is defined by an Elmish program.

Selected rule:

```text
one primary ProgramComponent for the application
```

Do not create independent program stores for ordinary page fragments.

---

## Bolero `ElmishComponent`

Current documentation says `ElmishComponent<'model, 'msg>` can receive a sub-model and only render when that model changes.

Interpretation:

```text
rendering boundary != state ownership boundary
```

Use it for rendering isolation/optimization when needed.

Do not use it as justification to create local MVU state for every visual component.

---

## Elmish parent-child composition

Elmish documentation verifies the standard pattern:

```text
child Model
child Msg
child update
parent wraps child Msg
parent uses Cmd.map
```

This is the mechanism used when a feature genuinely deserves its own MVU boundary.

It is not a requirement to make every component a child application.

---

## Elm application structure guidance

Elm's official guide recommends page-centered modules containing:

```text
Model
init
update
view
helpers
```

and explicitly warns against globally splitting code into:

```text
Model/
View/
Update/
```

It also warns against translating the visual component tree directly into architectural components.

This strongly supports feature/page colocation.

---

## Framework-agnostic feature organization

Current Angular guidance recommends organizing by feature areas and avoiding top-level directories based purely on code type.

Redux guidance likewise recommends feature folders and notes that state should be organized around data/functionality rather than UI components.

These are not Bolero rules.

They independently support the same feature-oriented repository principle.

---

# 13. Failure modes you must design for

## Root `Msg` starts growing again

Bad:

```text
App.Msg gets one new case for every button and feature event
```

Better:

```text
App.Msg
├── PageChanged
├── SharedMsg
└── PageMsg
```

Push detail down to the owner.

---

## `Shared.Model` becomes another giant dumping ground

Bad:

```text
"it is used somewhere, put it in Shared"
```

Better question:

```text
does this state need to survive navigation
or be consumed by multiple independent routes?
```

If no:

```text
keep it page-local
```

---

## The same entity exists in several models

Bad:

```text
HomeFeaturedEvent : Event
EventsList : Event list
SelectedEvent : Event
```

Better:

```text
Shared.Events : Map<EventId, Event>

Home stores/derives IDs
Events page stores filters
SelectedEventId : EventId option
```

---

## `PageModel` is used for canonical shared data

Bad:

```text
Events PageModel owns the only copy of events
```

Then navigation away discards data unnecessarily.

Better:

```text
Shared.Events -> canonical data
Events.PageModel -> search/filter/page selection
```

---

## Everything becomes a child MVU

Symptom:

```text
dozens of Model/Msg/update triplets
for cards, sections and layout pieces
```

Fix:

```text
collapse stateless pieces back into view functions
```

---

## Effects leak into views

Bad:

```text
view calls server directly
```

Fix:

```text
view -> dispatch
update -> Cmd
Cmd -> server
result -> Msg
```

---

## Page modules directly mutate shared state

Avoid hidden coupling.

Preferred direction:

```text
page emits local Msg
parent/shared updater owns shared state changes
```

For complex cross-boundary workflows, add explicit feature effects/out-messages.

---

## `Ui/` becomes a second global feature folder

If `Ui/` contains:

```text
EventCard
ProjectEditor
MemberSettings
TournamentRegistration
```

those are probably feature-owned.

Move them beside the feature.

---

## Premature abstraction

Do not start with:

```text
50 interfaces
generic feature framework
generic repository abstraction
generic page factory
generic message router
```

Start concrete.

Refactor when repetition is real.

---

# 14. Testing boundaries

The most valuable tests are around pure updates and state ownership.

Test:

```text
message
+
initial model
↓
update
↓
expected model + expected command shape
```

Examples:

```text
Events.SearchChanged
-> search changes
-> page number resets

Events.NextPage
-> page increments

PageChanged
-> correct Page selected

EventsLoaded
-> canonical Shared.Events updated

leaving Login PageModel
-> credentials no longer retained in root state
```

Also test router round-trips:

```text
Page -> URL -> Page
```

for every public route.

---

# Part III — Step-by-step implementation

# 15. Phase 0 — Decide the ownership rules before coding

Write these rules into `docs/architecture.md`:

```text
1. one main Elmish program
2. root Model is orchestration
3. root Msg is orchestration
4. canonical cross-page data lives in Shared.Model
5. page-only transient state lives with the page
6. PageModel is used only for state that should disappear with the page
7. effects run through Cmd
8. ordinary visual pieces are pure views
9. feature code stays colocated
10. split only after a real boundary appears
```

Acceptance criteria:

```text
[ ] team can explain what belongs in Shared
[ ] team can explain what belongs in PageModel
[ ] team can explain when a component does NOT need MVU
```

Do this before adding many pages.

---

# 16. Phase 1 — Create the Bolero solution

Create from the current official template.

Conceptually:

```bash
dotnet new install Bolero.Templates
dotnet new bolero-app -o Community
```

Then pin the tested SDK in `global.json`.

Do not leave the project dependent on whichever SDK happens to be newest on a developer machine.

Acceptance criteria:

```text
[ ] solution restores
[ ] server/client run locally
[ ] default route renders
[ ] refresh on a routed page works
```

---

# 17. Phase 2 — Establish repository structure

Refactor the starter into:

```text
src/
├── Community.Shared/
├── Community.Client/
└── Community.Server/
```

Inside Client:

```text
App/
State/
Pages/
Ui/
Infrastructure/
Main.fs
```

Start shallow.

Do not create empty directories for hypothetical features.

Acceptance criteria:

```text
[ ] each directory has a clear ownership rule
[ ] no global Models/Msgs/Updates directories
[ ] feature-specific UI is not dumped into Ui/
```

---

# 18. Phase 3 — Define shared domain types

Create only the domain types needed for the first slice.

Developer example:

```fsharp
type EventId = EventId of Guid

type Event =
    {
        Id : EventId
        Title : string
        StartsAt : DateTime
        Location : string
    }

type CommunityInfo =
    {
        Name : string
        MemberCount : int
    }
```

Gaming example:

```fsharp
type ServerId = ServerId of Guid

type GameServer =
    {
        Id : ServerId
        Name : string
        Players : int
        Capacity : int
    }
```

Domain files should not depend on Bolero HTML rendering.

Acceptance criteria:

```text
[ ] domain types compile in Community.Shared
[ ] server can reference them
[ ] client can reference them
[ ] domain layer does not import UI modules
```

---

# 19. Phase 4 — Build routing first

Define:

```fsharp
type Page =
    | [<EndPoint "/">]
      Home

    | [<EndPoint "/about">]
      About
```

Root model:

```fsharp
type Model =
    {
        Page : Page
        Shared : Shared.Model
    }
```

Root message:

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

Attach the router.

Acceptance criteria:

```text
[ ] / renders Home
[ ] /about renders About
[ ] browser back/forward updates model
[ ] router owns URL parsing
```

---

# 20. Phase 5 — Build the root App as orchestration only

`App.update` should initially look conceptually like:

```fsharp
let update msg model =
    match msg with
    | PageChanged page ->
        { model with Page = page }, Cmd.none

    | SharedMsg sharedMsg ->
        let shared, cmd =
            Shared.update sharedMsg model.Shared

        { model with Shared = shared },
        Cmd.map SharedMsg cmd

    | PageMsg pageMsg ->
        let page, cmd =
            Page.update pageMsg model.Page

        { model with Page = page },
        Cmd.map PageMsg cmd
```

Exact details may vary with how `PageModel` is represented.

The important property is:

```text
App.update does not contain Events business logic.
App.update does not contain Projects business logic.
```

Acceptance criteria:

```text
[ ] root update is small
[ ] root update delegates
[ ] no page-specific filters/pagination logic appears here
```

---

# 21. Phase 6 — Create the shared application state

Start with only what is genuinely cross-page.

Example:

```fsharp
module Shared

type Model =
    {
        Community : RemoteData<CommunityInfo>
        Events : RemoteData<Map<EventId, Event>>
    }

type Msg =
    | LoadCommunity
    | CommunityLoaded of Result<CommunityInfo, exn>
    | LoadEvents
    | EventsLoaded of Result<Event list, exn>
```

Normalize loaded entities:

```fsharp
events
|> Seq.map (fun event -> event.Id, event)
|> Map.ofSeq
```

Acceptance criteria:

```text
[ ] Home and Events can read the same event cache
[ ] canonical events are stored once
[ ] loading/error state is explicit
```

---

# 22. Phase 7 — Build the Home page as a mostly pure page

Home usually needs little or no local state.

Example composition:

```text
Home.view
├── Hero
├── CommunityStats
├── UpcomingEvents
├── FeaturedProjects
├── Sponsors
└── JoinCommunity
```

Most sections can be normal functions.

Do not create `Home.Model` merely because Home exists.

Acceptance criteria:

```text
[ ] static sections render without local MVU
[ ] shared data is passed/read cleanly
[ ] no server calls originate directly from view
```

---

# 23. Phase 8 — Add the first stateful page with `PageModel`

Use Events as the first real local-state example.

```fsharp
module Events

type Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
    }

type Msg =
    | SearchChanged of string
    | CategoryChanged of Category option
    | NextPage
    | PreviousPage
```

Route:

```fsharp
| [<EndPoint "/events">]
  Events of PageModel<Events.Model>
```

Define the default model through `Router.inferWithModel`.

Acceptance criteria:

```text
[ ] navigating to /events initializes Events.Model
[ ] typing search changes only Events.Model
[ ] Shared.Events is not copied into Events.Model
[ ] leaving /events discards local page state if that is the chosen policy
```

---

# 24. Phase 9 — Add nested page message composition

Create:

```fsharp
module Page

type Msg =
    | EventsMsg of Events.Msg
```

When Projects becomes stateful:

```fsharp
type Msg =
    | EventsMsg of Events.Msg
    | ProjectsMsg of Projects.Msg
```

Map child commands with `Cmd.map`.

Acceptance criteria:

```text
[ ] App.Msg remains unchanged
[ ] Events.Msg can grow without touching App.Msg
[ ] child commands are lifted safely
```

---

# 25. Phase 10 — Add server remoting through commands

Define a shared contract:

```fsharp
type CommunityApi =
    {
        GetCommunity : unit -> Async<CommunityInfo>
        GetEvents : unit -> Async<Event list>
    }
```

Client flow:

```text
Shared.LoadEvents
   ↓
Shared.update
   ↓
Cmd.ofAsync
   ↓
CommunityApi.GetEvents
   ↓
Shared.EventsLoaded
   ↓
Shared.update
```

Server implements the contract through Bolero Remoting or an equivalent explicit API.

Acceptance criteria:

```text
[ ] no view directly invokes remote function
[ ] loading state appears
[ ] success updates canonical cache
[ ] failure becomes explicit state/message
```

---

# 26. Phase 11 — Add feature-owned UI beside the feature

If Events now needs:

```text
EventCard
EventFilters
EventEmptyState
```

place them beside Events:

```text
Pages/
└── Events/
    ├── Events.fs
    ├── EventCard.fs
    ├── EventFilters.fs
    └── EventEmptyState.fs
```

Do not move them into global `Ui/` unless multiple unrelated features genuinely reuse them.

Acceptance criteria:

```text
[ ] Events feature can be understood by reading one directory
[ ] Ui/ remains small and generic
```

---

# 27. Phase 12 — Add Projects / Servers using the same rules

Developer community:

```text
Projects
```

Gaming community:

```text
Servers
Tournaments
```

For each new feature ask:

```text
Does it need local state?
Does its canonical data already exist in Shared?
Does local state need to survive navigation?
Does it need a child Msg?
Does it need remote effects?
```

Do not copy-paste a full MVU template blindly.

Acceptance criteria:

```text
[ ] stateless feature stays a view
[ ] stateful feature owns its Model/Msg/update
[ ] shared canonical data remains normalized
```

---

# 28. Phase 13 — Add authentication only after public pages are stable

Persistent auth:

```text
Shared.Model.CurrentUser
```

Temporary credentials:

```text
Login PageModel<Login.Model>
```

This gives a clean lifecycle:

```text
Login page active
    ↓
credentials exist
    ↓
login succeeds
    ↓
Shared.CurrentUser updated
    ↓
navigate away
    ↓
LoginModel discarded
```

Acceptance criteria:

```text
[ ] credentials are not stored in persistent Shared.Model
[ ] authenticated user is available across pages
[ ] logout clears shared session state
```

---

# 29. Phase 14 — Add cross-feature effects only when needed

Do not make Events depend directly on `App.Model`.

If Events needs to request:

```text
navigate to event
refresh global cache
open auth flow
```

introduce an explicit output/effect boundary.

Concept:

```fsharp
type Events.Effect =
    | NavigateTo of EventId
    | ReloadEvents
```

Parent interprets it.

Acceptance criteria:

```text
[ ] child remains reusable/testable
[ ] child does not know root Msg constructors
[ ] cross-boundary behavior is explicit
```

Skip this phase if the app does not need it.

---

# 30. Phase 15 — Add rendering optimization only after measurement

Bolero `ElmishComponent` is useful when a sub-view is expensive and only depends on a sub-model.

Use:

```text
large lists
expensive derived views
independent heavy visual regions
```

Do not use as default architecture.

Acceptance criteria:

```text
[ ] optimization is based on observed rendering cost
[ ] state architecture remains unchanged
[ ] no duplicate store/model introduced
```

---

# 31. Phase 16 — Add tests around ownership boundaries

Minimum client tests:

```text
Shared.update
Events.update
Page.update
App.update
routing
```

Test examples:

```text
SearchChanged resets Events page number
EventsLoaded updates Shared.Events
PageChanged preserves Shared.Model
leaving page removes PageModel state
selected ID resolves against Shared cache
```

Server tests:

```text
remoting handler
validation
persistence
authorization
```

Acceptance criteria:

```text
[ ] pure update logic is testable without browser
[ ] routing round-trips are covered
[ ] async integration path is covered
```

---

# 32. Phase 17 — Add styling and community-specific design system

Keep design tokens independent from domain state.

Example:

```text
Ui/
├── DesignTokens.fs
├── Layout.fs
├── Navbar.fs
└── Primitives.fs
```

Or keep CSS/Tailwind tokens in `wwwroot`.

For a developer community:

```text
neutral
minimal
technical
```

For a gaming community:

```text
dark-first
media-heavy
strong accents
```

Architecture stays the same.

---

# 33. Phase 18 — Production hardening

Add:

```text
error boundaries
404 route
loading skeletons
retry policy
server validation
logging
cache policy
analytics
accessibility checks
SEO/render-mode review
security headers
```

Do not add these by mutating the architecture into framework-specific global state.

Acceptance criteria:

```text
[ ] unknown URL has intentional behavior
[ ] remote errors produce user-visible state
[ ] server validates all client input
[ ] no secret is shipped to WebAssembly
```

---

# 34. Phase 19 — Rollout order

Recommended order:

```text
1. Bolero starter runs
2. App + routing
3. Shared.Model
4. Home
5. Events PageModel
6. Page.Msg + Cmd.map
7. remoting
8. canonical event cache
9. Projects / Servers
10. Members
11. auth if needed
12. feature effects if needed
13. tests
14. optimization
15. production hardening
```

Do not begin by building an abstract architecture framework.

Ship one vertical slice first.

---

# Part IV — Technical reference

# 35. Recommended root source-of-truth model

```fsharp
type Model =
    {
        Page : Page
        Shared : Shared.Model
    }
```

This is intentionally small.

Do not add fields here simply because they are convenient to reach.

---

# 36. Recommended root message

```fsharp
type Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

This is the anti-monolith boundary.

---

# 37. Recommended Page shape

```fsharp
type Page =
    | [<EndPoint "/">]
      Home

    | [<EndPoint "/about">]
      About

    | [<EndPoint "/events">]
      Events of PageModel<Events.Model>

    | [<EndPoint "/projects">]
      Projects of PageModel<Projects.Model>
```

If a page does not need state:

```text
do not invent a Model for it
```

---

# 38. Recommended `RemoteData`

A simple representation:

```fsharp
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Loaded of 'T
    | Failed of string
```

For richer apps you may add stale/reloading states.

Start simple.

---

# 39. Recommended normalized entity state

```fsharp
type Model =
    {
        Events : RemoteData<Map<EventId, Event>>
        Projects : RemoteData<Map<ProjectId, Project>>
    }
```

Page models should hold IDs and view/filter state, not duplicate canonical objects.

---

# 40. Recommended Events state

```fsharp
module Events

type Model =
    {
        Search : string
        Category : Category option
        PageNumber : int
        SelectedEventId : EventId option
    }

type Msg =
    | SearchChanged of string
    | CategoryChanged of Category option
    | NextPage
    | PreviousPage
    | EventSelected of EventId
```

This is a legitimate child MVU because it owns coherent behavior.

---

# 41. Recommended dependency rule

```text
Shared domain/contracts
        ↓
Infrastructure + state
        ↓
Pages/features
        ↓
App composition
```

Never:

```text
Shared domain -> Page
Server domain -> Client view
Ui primitive -> Events state
Feature A -> Feature B internals
```

For cross-feature behavior, go through shared domain/state or an explicit parent effect.

---

# 42. Routing state rule

Encode in URL when users should be able to:

```text
bookmark it
share it
refresh it
use back/forward meaningfully
```

Examples:

```text
/events/42
/projects?page=2
/members/ahmad
```

Keep out of URL when it is temporary internal state:

```text
password field
open hover state
temporary modal draft
internal loading flag
```

Choose deliberately.

---

# 43. State lifetime rule

```text
Application lifetime
    -> Shared.Model

Route lifetime
    -> PageModel / page Model

Render call lifetime
    -> local function values / derived values

Server lifetime
    -> server persistence/cache, not client Model
```

This is the simplest way to reason about ownership.

---

# 44. Functional acceptance test

## Architecture

```text
[ ] exactly one primary Elmish program
[ ] root Model contains only orchestration/persistent roots
[ ] root Msg has only top-level message namespaces
[ ] root update delegates feature logic
```

## Routing

```text
[ ] every route round-trips
[ ] PageChanged is driven by router
[ ] PageModel defaults are defined
[ ] URL-worthy state is encoded in route
```

## Shared state

```text
[ ] canonical cross-page entities stored once
[ ] IDs are used for references
[ ] temporary form/filter state is not in Shared without reason
```

## Page state

```text
[ ] stateful page owns Model/Msg/update
[ ] stateless page is not forced into MVU
[ ] leaving disposable page removes its state
```

## Effects

```text
[ ] remote work originates from Cmd
[ ] result returns as Msg
[ ] errors are represented explicitly
```

## UI

```text
[ ] generic Ui folder remains small
[ ] feature-specific UI is colocated
[ ] cards/sections do not get fake MVU state
```

---

# 45. Performance principles

Start with correctness.

Then measure.

Preferred order:

```text
1. keep model normalized
2. derive view data instead of duplicating it
3. avoid unnecessary remote requests
4. paginate large datasets
5. optimize expensive view regions
6. use ElmishComponent when measurements justify it
```

Do not distort the state model merely to avoid a theoretical render.

---

# 46. Upgrade policy

Pin:

```text
.NET SDK
Bolero packages
Elmish package
critical CSS/build tooling
```

Use `global.json`.

Before upgrades:

```text
build
run routing tests
run update tests
run remoting tests
run WebAssembly/server render smoke test
```

Important current note:

```text
Bolero main repository currently documents .NET 10
some older template/documentation text still references .NET 8
```

Trust the tested current dependency set, not a stale prose page.

---

# 47. Why this architecture is intentionally not “maximum abstraction” on day one

The goal is not to predict the final application.

The goal is to preserve good boundaries while allowing cheap refactoring.

Start:

```text
Pages/Events.fs
```

Then, if needed:

```text
Pages/Events/
├── Events.fs
├── EventCard.fs
└── EventFilters.fs
```

Then, only if Events becomes a multi-route domain:

```text
Features/Events/
├── State.fs
├── ListPage.fs
├── DetailPage.fs
├── RegistrationPage.fs
└── ...
```

The architecture grows from evidence.

Not imagination.

---

# Part V — Current verification references

# 48. Primary references

## Bolero — main repository

Verified points:

```text
Elmish integration
routing
remoting
current main-repository getting started: .NET SDK 10.0
```

Source:

https://github.com/fsbolero/Bolero

---

## Bolero — Using Elmish

Verified points:

```text
ProgramComponent<'model,'msg> hosts the Elmish program
ElmishComponent can render from a sub-model
view is recomputed on updates and component boundaries can optimize rendering
```

Source:

https://fsbolero.io/docs/Elmish

---

## Bolero — Routing

Verified points:

```text
endpoint DU stored in model
PageChanged message
Router.infer
PageModel<'T>
Router.inferWithModel
```

Source:

https://fsbolero.io/docs/Routing

---

## Bolero — Remoting

Verified points:

```text
remote service represented as async functions
client normally invokes them from Elmish update
Cmd.ofAsync used for request/result/error flow
```

Source:

https://fsbolero.io/docs/Remoting

---

## Elmish — parent-child composition

Verified points:

```text
child Model/Msg/update
parent wrapper messages
Cmd.map
compiler-checked composition
```

Source:

https://elmish.github.io/elmish/docs/parent-child.html

---

## Elm — application structure

Verified points:

```text
page-centered modules
Model/init/update/view kept together
do not globally split Model/View/Update
do not make architecture mirror visual components
refactor when real types/boundaries emerge
```

Source:

https://guide.elm-lang.org/webapps/structure.html

---

## Redux style guide

Used only as framework-agnostic supporting evidence.

Verified points:

```text
feature-folder organization
root state split into owned slices
state organized by data/functionality rather than UI components
```

Source:

https://redux.js.org/style-guide/

---

## Angular style guide

Used only as framework-agnostic supporting evidence.

Verified points:

```text
organize by feature areas
group related files
avoid top-level folders based only on code type
```

Source:

https://angular.dev/style-guide

---

# 49. Final architecture recommendation

Use this:

```text
community-platform/
│
├── Community.Shared/
│   ├── Domain
│   └── Contracts
│
├── Community.Client/
│   │
│   ├── App
│   │   ├── Routing
│   │   └── root MVU orchestration
│   │
│   ├── State
│   │   └── persistent Shared.Model
│   │
│   ├── Pages / Features
│   │   ├── Home
│   │   ├── Events
│   │   ├── Projects / Servers
│   │   └── Members
│   │
│   ├── Ui
│   │   └── truly reusable layout/primitives
│   │
│   ├── Infrastructure
│   │   └── remote/browser effects
│   │
│   └── Main.fs
│
└── Community.Server/
    ├── Program
    ├── Services
    └── Persistence
```

With:

```fsharp
type App.Model =
    {
        Page : Page
        Shared : Shared.Model
    }

type App.Msg =
    | PageChanged of Page
    | SharedMsg of Shared.Msg
    | PageMsg of Page.Msg
```

And the operating rules:

```text
ONE ProgramComponent
ONE authoritative application state tree

persistent cross-page data
    -> Shared.Model

temporary page behavior
    -> PageModel / page Model

feature complexity
    -> feature-owned Msg/update/view

side effects
    -> Cmd

visual composition
    -> pure view functions by default

shared entities
    -> normalized and stored once

repository growth
    -> by feature/domain, not global technical type
```

This is the selected default because it gives the community site:

```text
small root types
clear ownership
type-safe routing
predictable state lifetime
testable updates
controlled effects
feature-local growth
minimal boilerplate
clean path from simple landing page to larger application
```

It is intentionally simple enough for the first public landing page and structured enough to grow into:

```text
events
projects
servers
tournaments
members
authentication
admin tools
community APIs
real-time status
```

without replacing the foundation.
