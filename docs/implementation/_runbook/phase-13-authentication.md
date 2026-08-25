---
phase: 03-step-by-step-implementation/phase-13-authentication
---

**Phase 13 complete — authentication with a single auth-gated `/account`
page.** The session contract (`signIn`, `getUsername`, `signOut`) and cookie
persistence already existed from earlier phases. This phase added the auth
gate UI: the `/account` page shows a **sign-in form when signed out** and the
**current user's profile editor when signed in**.

### Reference

The reference asks for sign in / sign out, account state that survives refresh
(persistent lifetime), and auth-gated pages that show a sign-in prompt when
logged out.

Design decision (user choice, mid-phase refactor): we initially built a
separate `/profile` page, but recognized that **Profile duplicated Account**
(both showed "Signed in as X" + sign out when signed in). We merged the gated
profile editor into `/account` and **removed `/profile`** — one page that logs
you in when signed out and shows your profile when signed in.

### Session contract + service (unchanged)

`CommunityApi.Shared` already has `signIn`, `getUsername`, `signOut`.
`CommunityApiService` implements them (signIn returns `Some username` for
`password = "password"`; `getUsername` is wrapped in `ctx.Authorize`, 401 when
signed out). The session persists via an auth cookie, so a full page refresh
keeps you signed in. **No contract/service change was needed.**

### Auth gate (single feature-owned view)

`Pages/Account.fs` — the Account feature's `Model` holds the sign-in form draft
**and** the profile-edit draft:

```fsharp
type Model =
    { username: string; password: string   // sign-in form draft
      handle: string; bio: string }         // profile-edit draft

type Msg =
    | SetUsername of string
    | SetPassword of string
    | SetHandle of string
    | SetBio of string
    | Clear
    | Submit
```

`init` seeds all fields empty. `update` is pure (edits the drafts; `Submit` is
interpreted by the root). `view` is the auth gate — it reads the root session
state (`username: option<string>`, selected from `SharedModel.account`):

```fsharp
let view (form: Model) (username: option<string>) (signInFailed: bool)
         (localDispatch: Msg -> unit) (signOut: unit -> unit) =
    match username with
    | Some name ->
        // Signed in: profile editor.
        Layout.AccountSignedIn()
            .Username(name)
            .Handle(form.handle, fun h -> localDispatch (SetHandle h))
            .Bio(form.bio, fun b -> localDispatch (SetBio b))
            .Save(fun _ -> localDispatch Submit)
            .SignOut(fun _ -> signOut ())
            .Elt()
    | None ->
        // Signed out: sign-in form.
        Layout.SignIn()...
```

### Templates

`wwwroot/main.html` — the `SignIn` template is unchanged. The shallow
`AccountSignedIn` plus the now-removed `Profile`/`ProfileSignedOut` templates
were consolidated into a single richer `AccountSignedIn` template: "Signed in
as **Username**" plus a form with `Handle` (bound input) and `Bio` (bound
textarea), a **Save** button (`onsubmit="${Save}"`) and a **Sign out** button
(`onclick="${SignOut}"`).

### Root orchestration

- `App.fs` `Page` keeps `| [<EndPoint "/account">] AccountPage of
  PageModel<Account.Model>`. The `ProfilePage`/`ProfileMsg` additions were
  **removed**.
- `App.fs` `Message` = `SetPage | SharedMsg | AccountMsg`.
- `App.fs` root `update` already translates `Account.Submit` into
  `SharedMsg (Shared.SendSignIn (...))` (cross-feature effect), and on
  `Shared.RecvSignIn (Some _)` clears the Account form (`AccountMsg
  Account.Clear`).
- `App.fs` `defaultPageModel` supplies a fresh `Account.init` on each entry
  (state-lifetime rule — transient drafts reset on fresh navigation).
- `Ui/Layout.fs` has a single **Account** menu item + body case; the Profile
  menu/body were removed.
- `.fsproj` no longer references `Pages/Profile.fs` (file deleted).

The `/profile` URL now falls back to Home (`DefaultNotFound Home`).

### MVU verification (browser console)

Signed out (app start): `GetSignedInAs` → `RecvSignedInAs` (401 → `account =
 None`) → `/account` renders the sign-in form.

Sign in (username + `password`, submit): `SendSignIn ("bob","password")` →
`RecvSignIn (Some "bob")` → `AccountMsg Clear` → `/account` re-renders to the
profile editor.

Refresh: the Http cookie persists → `getUsername` returns "bob" → still signed
in. Sign out: `SendSignOut` → session cleared → `/account` returns to the
sign-in form.

Removed route: `/profile` → `withNotFound Home` redirects to `/`.

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/account # 200 (SPA route)
curl -I http://localhost:5023/profile # 200 (SPA fallback -> Home)
```

Browser: `/account` shows the sign-in form when logged out, the profile editor
when signed in; refresh persists the session; `/profile` no longer exists
(falls back to Home). `verify.sh` reports `VERIFY OK`.

### Files changed (relative to the initial Phase 13 commit)

```text
src/Community.Web.Client/Pages/Profile.fs              (deleted)
src/Community.Web.Client/Pages/Account.fs               (+ handle/bio state, gate view)
src/Community.Web.Client/wwwroot/main.html              (merged AccountSignedIn; -Profile/-ProfileSignedOut)
src/Community.Web.Client/App/App.fs                    (-ProfilePage/-ProfileMsg)
src/Community.Web.Client/Ui/Layout.fs                  (-Profile menu/body)
src/Community.Web.Client/Community.Web.Client.fsproj   (-Pages/Profile.fs)
docs/implementation/_runbook/phase-13-authentication.md  (this file, rewritten)
```