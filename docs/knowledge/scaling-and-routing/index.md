# Scaling and Routing — Programs Made of Programs

> Source: *The Elmish Book* (Zaid Ajaj) — part 4 (Composing Larger
> Applications): pages-as-programs, splitting-programs,
> integrating-commands, composition-forms, understanding-data-communication,
> intent, routing, url-type, multi-page-routing.

One giant `State`/`Msg` dies at three features. The scaling move: make
**each page its own Elmish program**, then compose parents from children —
the client-side echo of bounded contexts.

## The monolith State

This repo's root program almost became the ❌ — before the split, every
page's data and messages pile into one root type:

```fsharp
// ❌ the shape that dies at three features: one State, one Msg,
// every page's events and data mixed together
 type RootState =
    { games: RemoteData<Game[]>; tournaments: RemoteData<Tournament[]>
      loginForm: LoginForm; inbox: InboxState; (* ...and growing *) }
type RootMsg =
    | GameToggled of string | TournamentToggled of string
    | LoginFormChanged of string | (* ...one case per page, forever *) }
```

Every page's change touches the root file; every message name fights for
space; nothing can be reasoned about alone. The book's answer: pages are
*logical compartments* of the root program — each with its own
State/Msg/update — and the parent only manages which child is active.

## 1. Pages as programs

- A login page, a dashboard, a settings page — each is its own little TEA
  triplet. The **root program** holds "which page is active" plus the
  children.
- A parent that only forwards to children can be **reduced away** (state
  moves up a level) — don't pay syntactic overhead for a pointless layer.

## 2. Splitting a program (the refactor)

1. Move a feature's state into a `Child.State` field of the parent.
2. Wrap its messages: `ChildMsg of Child.Msg`.
3. Compose dispatch: the parent maps child events into its own loop;
   child-facing dispatch is `ChildMsg >> dispatch` style forwarding.
4. Each program lives in **its own module/file** — `Login.fs`, `App.fs`,
   `Main.fs`.

## 3. Composition forms

| Form | Shape | Trade-off |
| --- | --- | --- |
| **State-field** | child State is a field; child Msg a case | explicit, flexible; wiring at each level |
| **State/Msg pair** | same, but strictly paired | mechanical, easy to generate |
| **Whole program** | `(State * Cmd<Msg>)` + parent wraps `Program` | children fully encapsulate commands |

The book's canonical state-field shape, verbatim:

```fsharp
// ✅ from The Elmish Book (composition-forms.md)
type State =
  { Counter : Counter.State
    InputText : InputText.State
    CurrentPage : Page }

type Msg =
  | CounterMsg of Counter.Msg
  | InputTextMsg of InputText.Msg
  | SwitchPage of Page
```

The wiring that makes children first-class — delegation plus `Cmd.map`,
verbatim from the book:

```fsharp
// ✅ from The Elmish Book — parent update delegating to a child
| Page.Counter counterState, CounterMsg counterMsg ->
    let counterState, counterCmd = Counter.update counterMsg counterState
    let nextState = { state with CurrentPage = Page.Counter counterState }
    let nextCmd = Cmd.map CounterMsg counterCmd
    nextState, nextCmd
```

The alternative — **DU composition**, `Page = Counter of Counter.State | ...` —
*resets* a child's state when you navigate away; state-field keeps all
children alive. Choose deliberately.

## 4. Data communication: proxy and Intent

- Siblings never reference each other — all traffic goes **through the
  parent**, which may inspect, forward, or act. The book's running
  example: a **Login page** hands the authenticated `User` to a **Home
  page**; Login and Home know nothing of each other; `App` relays.
- That interception surprises child developers — so encode it: the
  **Intent pattern** (a.k.a. `ExternalMsg`), verbatim:

```fsharp
// ✅ from The Elmish Book (intent.md) — inside Login.fs
[<RequireQualifiedAccess>]
type Intent =
    | UserLoggedIn of Api.User
    | DoNothing
```

The child's update grows a third return value — `State * Cmd<Msg> *
Intent` — and the parent matches it:

```fsharp
// ✅ from The Elmish Book — parent consumes the intent
| LoginMsg loginMsg, Page.Login loginState ->
    let loginState, loginCmd, intent = Login.update loginMsg loginState
    match intent with
    | Login.Intent.DoNothing ->
        { state with CurrentPage = Page.Login loginState }, Cmd.map LoginMsg loginCmd
    | Login.Intent.UserLoggedIn user ->
        let homeState, homeCmd = Home.init user
        { state with CurrentPage = Page.Home homeState }, Cmd.map HomeMsg homeCmd
```

The child *declares* what the parent must react to; the compiler checks
both sides. The book's caution: don't introduce it when there is no need.

## 5. Routing

- Routing = listening for **URL changes** as just another event:
  `UrlChanged of string list`. Render by matching the current URL.
- **Feliz.Router** owns the history API and parsing: `Router.currentUrl`,
  `Router.onUrlChanged`, and `Router.navigate` (a `Cmd` — navigation after
  login is just a command).
- Turn raw URLs into a **`Url` type** — the book's canonical parser,
  verbatim:

```fsharp
// ✅ from The Elmish Book (introducing-url-type.md)
[<RequireQualifiedAccess>]
type Url =
  | Index
  | About
  | Contact
  | User of userId:int
  | NotFound

let parseUrl = function
    | [ ] -> Url.Index
    | [ "about" ] -> Url.About
    | [ "contact" ] -> Url.Contact
    | [ "user"; Route.Int userId ] -> Url.User userId
    | _ -> Url.NotFound
```

The book's rule: the `Url` is *not* a `Page` — it carries exactly enough
information to **initialize** one.

- **Nested routes compose like state**: the parent parses its segments and
  hands the rest to the child's own `Url` parser — children never see
  siblings' routes.
- Multi-page apps add **accessibility rules** (anonymous-only vs
  authenticated-only pages) and **re-initialization on F5** — decide what
  state must survive a refresh.

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Scaling &amp; routing"])
    n2["Pages as programs"]
    n3["each page: own State/Msg"]
    n4["parent manages children"]
    n5["reducible parents fold away"]
    n6["Splitting: the refactor"]
    n7["extract State field per child"]
    n8["wrap child Msg: ChildMsg of Child.Msg"]
    n9["compose dispatch: forward + map"]
    n10["modules in separate files"]
    n11["Composition forms"]
    n12["state-field composition"]
    n13["State/Msg as pair"]
    n14["whole-program (State * Cmd) * Msg"]
    n15["Data communication"]
    n16["siblings don't know each other"]
    n17["parent proxies messages"]
    n18["parent can intercept events"]
    n19["Intent pattern"]
    n20["child returns Intent (ExternalMsg)"]
    n21["UserLoggedIn of User"]
    n22["DoNothing = propagate as-is"]
    n23["update returns State * Cmd * Intent"]
    n24["Routing"]
    n25["UrlChanged event"]
    n26["match URL → render page"]
    n27["Feliz.Router: parse + history"]
    n28["programmatic navigation"]
    n29["Url type: parse segments"]
    n30["Route.Int, Route.Query"]
    n31["nested routes compose like state"]
    n32["multi-page: auth-gated pages"]
    n33["retaining state on refresh"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n1 --> n6
    n6 --> n7
    n6 --> n8
    n6 --> n9
    n6 --> n10
    n1 --> n11
    n11 --> n12
    n11 --> n13
    n11 --> n14
    n1 --> n15
    n15 --> n16
    n15 --> n17
    n15 --> n18
    n1 --> n19
    n19 --> n20
    n19 --> n21
    n19 --> n22
    n19 --> n23
    n1 --> n24
    n24 --> n25
    n24 --> n26
    n24 --> n27
    n24 --> n28
    n24 --> n29
    n29 --> n30
    n29 --> n31
    n24 --> n32
    n24 --> n33
```

## Cross-links

- Bounded contexts → pages: [domain-driven-design](../domain-driven-design/index.md).
- Commands the children issue: [commands-and-async-data](../commands-and-async-data/index.md).
- Intent = choices in the type system: [domain-modeling-with-types](../domain-modeling-with-types/index.md).
- Components and boundaries (the architecture view): [architecture-characteristics-and-governance](../architecture-characteristics-and-governance/index.md).
- The whole TEA loop these pages plug into: [fable-and-elm-architecture](../fable-and-elm-architecture/index.md).
