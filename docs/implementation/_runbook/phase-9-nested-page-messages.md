---
phase: 03-step-by-step-implementation/phase-9-nested-page-messages
---

**Phase 9 complete** — each page/feature now scopes its messages in its own
module, and those nested messages are composed into the root `Message` and
lifted with **`Cmd.map`**. The Account page's local messages no longer leak
into the root event dump; the root is a pure orchestration boundary.

### Goal met

The reference spec asks for:

```text
page-scoped message namespaces
nested messages composed into the root and lifted with Cmd.map
the root translates cross-feature effects (Submit -> session sign-in)
```

The Account feature (`Pages/Account.fs`) owns its form messages
(`SetUsername`, `SetPassword`, `Clear`, `Submit`). Session/auth stays on the
root `Shared` dispatcher, which owns `SharedModel.account` and reads the form
draft from the Account page only when needed.

### Ownership split (as chosen)

| Concern | Owned by |
| --- | --- |
| Sign-in form draft (`username`, `password`) | `Account` page module |
| Form edit / clear / submit intent | `Account.Msg` |
| Session data (`SharedModel.account`), sign-in/out, data loads | `Shared.Msg` |
| Translating `Submit` → `SendSignIn` effect | root `Message` (`AccountMsg`) |

### Implementation

The `Shared` module is declared **before** the root `Message` union so its
`Msg` type is in scope for the union's cases. The union cases are named
`SharedMsg` / `AccountMsg` (and the route case `AccountPage`) to avoid
colliding with the `Shared` / `Account` modules.

```fsharp
// Pages/Account.fs — the feature owns its local messages
module Account =
    type Model = { username: string; password: string }
    type Msg =
        | SetUsername of string
        | SetPassword of string
        | Clear
        | Submit
    let init = { username = ""; password = "" }
    let update msg model = ...   // pure, local only
```

```fsharp
// App/App.fs — nested messages composed into the root
module Shared =
    type Msg = GetGames | GotGames of Game[] | ... | SendSignIn of string*string
              | RecvSignIn of option<string> | SendSignOut | RecvSignOut
              | Error of exn | ClearError
    let update remote (shared: SharedModel) (msg: Msg) = ...   // returns lifted cmd

type Message =
    | SetPage of Page
    | SharedMsg of Shared.Msg
    | AccountMsg of Account.Msg
```

### The root dispatcher

The root interprets `Submit` as a **cross-feature effect**: the Account page
owns the form, but the session belongs to `Shared`, so the root translates the
child's submit intent into a shared session message:

```fsharp
| AccountMsg msg ->
    match model.page with
    | AccountPage pm ->
        match msg with
        | Account.Submit ->
            let send = Cmd.ofMsg (SharedMsg (Shared.SendSignIn (pm.Model.username, pm.Model.password)))
            model, send
        | _ ->
            let m, cmd = Account.update msg pm.Model
            Router.definePageModel pm m
            model, Cmd.map AccountMsg cmd
    | _ -> model, Cmd.none
```

And the shared layer is lifted back up with `Cmd.map SharedMsg`:

```fsharp
| SharedMsg msg ->
    let shared, cmd = Shared.update remote model.shared msg
    // cross-boundary effect: clear the Account form after a successful sign-in
    let cmd =
        match msg with
        | Shared.RecvSignIn (Some _) ->
            Cmd.batch [ Cmd.map SharedMsg cmd; Cmd.ofMsg (AccountMsg Account.Clear) ]
        | _ -> Cmd.map SharedMsg cmd
    { model with shared = shared }, cmd
```

`Main.fs` now dispatches the initial data batch as nested messages:
`Cmd.ofMsg (SharedMsg Shared.GetGames)` and friends.

### Changes

- `Pages/Account.fs` — **new** module holding `Account.Model`, `Account.Msg`,
  `init`, and `update`. Owns the sign-in form draft and its local messages.
- `App/App.fs` — extracted the shared-layer messages into `module Shared`
  (declared before `type Message`); root `Message` is now
  `SetPage | SharedMsg of Shared.Msg | AccountMsg of Account.Msg`; route case
  renamed `Account` → `AccountPage`; root `update` composes nested messages
  with `Cmd.map` and translates `Account.Submit` into a `Shared` session
  effect.
- `Ui/Layout.fs` — message dispatch now qualifies nested modules
  (`SharedMsg Shared.SendSignOut`, `AccountMsg (Account.SetUsername s)`,
  `AccountMsg Account.Submit`, `SharedMsg Shared.ClearError`); `view` and
  `menuItem` use `AccountPage`.
- `Main.fs` — initial data loads wrapped as `Cmd.ofMsg (SharedMsg Shared.*)`.

### MVU verification (browser console)

```
New message:: SharedMsg GetSignedInAs
New message:: SharedMsg GetGames
New message:: SharedMsg GetServers
New message:: SharedMsg GetTournaments
New message:: SharedMsg GetNews
New message:: SharedMsg GetPlayers
New message:: SharedMsg (RecvSignedInAs None)        // authed probe -> signed out
New message:: SharedMsg (GotGames ...)
```

Sign-in flow (typed player1 / password, clicked Sign in):

```
New message:: AccountMsg (SetUsername "player1")
New message:: AccountMsg (SetPassword "password")
New message:: AccountMsg Submit                    // root translates to a Shared effect
New message:: SharedMsg SendSignIn
New message:: SharedMsg (RecvSignIn (Some "player1"))
New message:: AccountMsg Clear                     // root clears the form post-success
New message:: SharedMsg GetPlayers                 // refresh Members
```

Sign-out flow:

```
New message:: SharedMsg SendSignOut
New message:: SharedMsg RecvSignOut
```

The page went from the sign-in form → **"Signed in as player1"** banner → back
to a blank form on sign-out. Only the intentional auth-protected `getUsername`
401 remains in the console.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] Page/feature scopes its messages in its own module (`Account.Msg`)
- [x] Nested messages composed into the root and lifted with `Cmd.map`
- [x] Root is an orchestration boundary, not an event dump
- [x] Cross-feature effect (Submit → session) translated by the parent
- [x] Session/auth stays on the Shared dispatcher; page owns transient form

### Next

Phase 10 — (see `_sequence.yaml`).