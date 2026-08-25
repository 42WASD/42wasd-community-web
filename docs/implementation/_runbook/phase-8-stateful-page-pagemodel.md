---
phase: 03-step-by-step-implementation/phase-8-stateful-page-pagemodel
---

**Phase 8 complete** — transient page state now lives in a **`PageModel<'T>`**
that is excluded from the URL, supplied by **`Router.inferWithModel`**, and
reset per the state-lifetime rule. The Account page is the first such page,
holding the sign-in form draft.

### Goal met

The reference spec asks for:

```text
a page with form/draft state that must not appear in the URL
state lives in PageModel<'T>, excluded from the route
```

The Account page (`/account`) carries the sign-in form draft (`username`,
`password`) in `Account of PageModel<AccountForm>`. Typing in the form updates
the PageModel in place; the draft never appears in the URL.

### Implementation

```fsharp
// Page union: the Account case carries a PageModel<AccountForm>
type Page =
    | ...
    | [<EndPoint "/account">] Account of PageModel<AccountForm>

// Transient page state — lives in the PageModel, not the root model
and AccountForm = { username: string; password: string }

// Root model: Page (route) + Shared (persistent). No more `local` field.
type Model =
    {
        page: Page
        shared: SharedModel
    }
```

```fsharp
// Router supplies a fresh default PageModel when entering the route
let router =
    let defaultPageModel = function
        | Account pm -> Router.definePageModel pm { username = ""; password = "" }
        | _ -> ()
    Router.inferWithModel SetPage (fun m -> m.page) defaultPageModel
    |> Router.withNotFound Home
```

### How updates mutate the PageModel

The `PageModel<'a>` is a mutable holder shared with the view. Typing a
character dispatches `SetUsername`/`SetPassword`, which update the instance in
place via `Router.definePageModel`:

```fsharp
| SetUsername s ->
    match model.page with
    | Account pm -> Router.definePageModel pm { pm.Model with username = s }
    | _ -> ()
    model, Cmd.none
```

`SendSignIn` reads the draft straight from the active PageModel, and
`ClearLoginForm` resets it after a successful sign-in. Because the same
instance is shared between the router and the view, the text the user types
survives re-renders without ever being encoded into the URL.

### State-lifetime rule (verified)

Transient page state resets when the page is navigated to fresh:

- Sign in → `Signed in as testuser` banner.
- Sign out → back to a blank Sign in form.
- Navigate away and back to `/account` → **fresh empty form** (the router
  re-supplies a new default PageModel). The draft does not leak across
  sessions of the page.

### Changes

- `App/App.fs` — `Page` gained `Account of PageModel<AccountForm>`; deleted the
  root `LocalModel` field; `Model` is now just `{ page; shared }`; `update`
  mutates the PageModel in place for `SetUsername`/`SetPassword`/
  `ClearLoginForm`; `router` switched from `Router.infer` to
  `Router.inferWithModel` with a `defaultPageModel` supplying the empty form.
- `Ui/Layout.fs` — new `accountPage` (renders the SignIn form or the
  signed-in banner); `view` gained the `Account` menu item and the `Account _`
  body case.
- `wwwroot/main.html` — new `SignIn` and `AccountSignedIn` templates.

### MVU verification (browser console)

```
New message:: SetUsername "testuser"
New message:: SetPassword "password"
New message:: SendSignIn
New message:: RecvSignIn (Some "testuser")
New message:: GetPlayers
New message:: ClearLoginForm
New message:: GotPlayers
```

Sign-out trace: `SendSignOut` → `RecvSignOut`; fresh `/account` navigation
dispatched the full data batch and rendered a blank form. Only the intentional
auth-protected `getUsername` 401 remains.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

### Acceptance (from reference spec)

- [x] A page with form/draft state that must not appear in the URL (Account)
- [x] State lives in `PageModel<'T>`, excluded from the route
- [x] `Router.inferWithModel` supplies the default PageModel
- [x] Page keeps transient state across in-page updates
- [x] Transient state resets per the state-lifetime rule (fresh navigation)

### Next

Phase 9 — Nested page messages (page/feature-scoped message namespaces).