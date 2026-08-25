---
phase: 03-step-by-step-implementation/phase-11-feature-owned-ui
---

**Phase 11 complete** — every page now renders itself via its owning feature
module. `Ui/` holds only cross-feature UI (layout shell, menu, shared error
notification). Views select shared state; they do not duplicate it.

### Goal met

The reference spec asks for:

```text
each page/feature owns its model, message, update, view
Shared state is selected, not duplicated
render isolation (ElmishComponent) used only where measured
```

### The refactor

Before Phase 11, every page view lived in `Ui/Layout.fs` next to the layout
shell. That conflated the global `Ui/` folder (cross-feature UI) with
feature-specific views. Phase 11 moved each page view beside its owning
feature:

```
Ui/
  Templates.fs      # the shared Layout template + dataRows helper (compiles early)
  Layout.fs         # shell only: menu, body dispatch, shared error notification
Pages/
  Home.fs           # owns Home.view + its stats selection
  Games.fs          # owns Games.view
  Servers.fs        # owns Servers.view
  Tournaments.fs    # owns Tournaments.view
  Members.fs        # owns Members.view
  About.fs          # owns About.view (static, no Msg)
  Account.fs        # owns Account.Model/Msg/update + Account.view
```

### Why `Ui/Templates.fs`

The page views need the shared `Layout` template and `dataRows` helper, but the
root `view` (in `Ui/Layout.fs`) composes the pages and depends on `App`.
That is a dependency cycle. Splitting the *shared rendering primitives* into a
early-compiled `Ui/Templates.fs` breaks it:

```
State  ->  Ui/Templates  ->  Pages/*  ->  App  ->  Ui/Layout  ->  Main
```

### Feature views own themselves

Each page view is a plain function that *selects* the canonical shared data it
needs (per the state-ownership model — "reuse, not duplicate") and returns a
node. Static pages (`About`) take no data and declare no message case
(message-organization: "Static pages do not need a message case").

```fsharp
module Home =
    let stats (shared: SharedModel) = ...            // select slices
    let view (shared: SharedModel) =
        Layout.Home()
            .GamesCount(...).OnlineNow(...)...
            .Games(dataRows shared.games <| fun g -> ...)
            .Elt()

module About =
    let view () = Layout.About().Elt()               // no state, no Msg
```

The Account view takes the live transient form (from the active PageModel) plus
the Shared slices it owns, and takes a `signOut` callback for the cross-feature
session effect:

```fsharp
module Account =
    let view (form: Model) (username: option<string>) (signInFailed: bool)
             (localDispatch: Msg -> unit) (signOut: unit -> unit) = ...
```

The root `view` (`Ui/Layout.fs`) is now a thin dispatcher that routes each route
to its feature module:

```fsharp
| Home         -> Home.view model.shared
| Games        -> Games.view model.shared
...
| AccountPage pm ->
    Account.view pm.Model model.shared.account model.shared.signInFailed
        (fun msg -> dispatch (AccountMsg msg))
        (fun () -> dispatch (SharedMsg Shared.SendSignOut))
```

### MVU verification (browser console)

Home dashboard rendered all stats + tables; Members rendered all 3 players;
Account sign-in round-trip through the moved view:

```
New message:: AccountMsg (SetUsername "player99")
New message:: AccountMsg (SetPassword "password")
New message:: AccountMsg Submit
New message:: SharedMsg (SendSignIn ("player99", "password"))
New message:: SharedMsg (RecvSignIn (Some "player99"))
New message:: AccountMsg Clear
```

Page → "Signed in as player99" with a Sign out button. The only console error
is the intentional auth-protected `getUsername` 401.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] Each page/feature owns its view (`Pages/*.fs`)
- [x] Views depend only on the feature's own state; they never reach into
      another owner's state
- [x] Shared state is selected (passed as the slice), not duplicated
- [x] `Ui/` reduced to cross-feature UI only (layout shell, menu, error)
- [x] Rendering boundary separated from the state-ownership boundary

### Next

Phase 12 — More features.