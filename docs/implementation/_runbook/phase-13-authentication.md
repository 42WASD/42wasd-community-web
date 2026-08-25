---
phase: 03-step-by-step-implementation/phase-13-authentication
---

**Phase 13 complete — added authentication + an auth-gated `/profile` page.**
The session contract (`signIn`, `getUsername`, `signOut`) and the Account
(sign-in) page already existed from earlier phases. This phase made auth
meaningful by adding a second, **auth-gated** page: `/profile`, which shows a
profile editor to signed-in users and a sign-in prompt to everyone else.

### Reference

The reference asks for pages that are only reachable by signed-in users — a
gate that renders different content based on session state. The obvious fit
for the gaming community was a personal **profile** page: you must be signed
in to view or edit your own profile.

Design decision (user choice): **New `/profile` page (gated)** rather than
gating an existing page, so we demonstrate the gate on a purpose-built page.

### Session contract (unchanged)

`Community.Web.Shared/Remoting/CommunityApi.fs` already exposed:

```fsharp
signIn: string * string -> Async<option<string>>
getUsername: unit -> Async<string>
signOut: unit -> Async<unit>
```

`Community.Web.Server/CommunityApiService.fs` implements them (sign-in
returns `Some username` for `password = "password"`; `getUsername` is wrapped
in `ctx.Authorize`, returning the identity name or 401). The session persists
via an auth cookie, so a full page refresh keeps you signed in. **No change to
the contract or service was needed this phase** — auth already existed.

### Auth gate templates

`wwwroot/main.html` gained two templates:

- `Profile` — "Your profile" title, "Signed in as **${Username}**", a form with
  `${Handle}` (input) and `${Bio}` (textarea) bound fields, a Save submit
  (`onsubmit="${Save}"`), and a Sign out button (`onclick="${SignOut}"`).
- `ProfileSignedOut` — "Profile" title, "Sign in to view and edit your
  profile.", and a "Sign in" link (`href="/account"`).

### Feature-owned page

`Pages/Profile.fs` — the feature-owned, auth-gated page:

```fsharp
module Profile =
    type Model = { handle: string; bio: string }
    type Msg =
        | SetHandle of string
        | SetBio of string
        | Save

    let init = { handle = ""; bio = "" }

    let update msg model =
        match msg with
        | SetHandle h -> { model with handle = h }, Cmd.none
        | SetBio b -> { model with bio = b }, Cmd.none
        | Save -> model, Cmd.none

    let view (form: Model) (username: option<string>)
             (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        cond username <| function
        | None ->
            Layout.ProfileSignedOut().Elt()   // auth gate
        | Some name ->
            Layout.Profile().Username(name).Handle(...).Bio(...)
                .Save(...).SignOut(...).Elt()
```

The page owns only the profile-edit draft (transient, carried by the route's
PageModel). Session state (who is signed in) lives on the root
`SharedModel.account`; sign-out is passed in as a callback — a cross-feature
session effect owned by the root/Shared.

### Wiring (same shape as the Account PageModel page)

- `.fsproj` — registered `Pages/Profile.fs` before `App/App.fs`.
- `App/App.fs` `Page` union — added `| [<EndPoint "/profile">] ProfilePage of
  PageModel<Profile.Model>`.
- `App/App.fs` `Message` — added `| ProfileMsg of Profile.Msg`, with an
  `update` case that matches `model.page` on `ProfilePage pm`, runs
  `Profile.update msg pm.Model`, calls `Router.definePageModel pm m`, and lifts
  with `Cmd.map ProfileMsg` (mirroring `AccountMsg`).
- `App/App.fs` router `defaultPageModel` — added `ProfilePage pm ->
  Router.definePageModel pm Profile.init` so a fresh empty draft is supplied on
  route entry (state-lifetime rule).
- `Ui/Layout.fs` — added the "Profile" menu item and the
  `| ProfilePage pm -> Profile.view pm.Model model.shared.account ...` body
  case. The gate reads `model.shared.account` (the root's session state).

The `/profile` route shares the `PageModel` pattern established for the
Account page, so the profile-edit draft is transient and resets on fresh
navigation.

### MVU verification (browser console trace)

Signed out:

```
New message:: SharedMsg GetSignedInAs        // on app start
New message:: SharedMsg (RecvSignedInAs ...) // 401 -> None -> gate renders
```

Navigate to `/profile` → renders "Sign in to view and edit your profile." with
a Sign in link.

Sign in (on `/account`, username + password, press Enter to submit):

```
New message:: SharedMsg (SendSignIn ("alice", "password"))
New message:: SharedMsg (RecvSignIn (Some "alice"))
New message:: AccountMsg Clear               // root clears the form on success
```

Navigate to `/profile`: renders the editor — "Signed in as **alice**", Handle +
Bio fields, Save + Sign out buttons.

Full page refresh on `/profile`: session cookie persists, so the editor still
renders (still signed in as alice). A logged-out refresh stays on the gate.

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/profile # 200 (SPA route)
```

Browser: `/profile` gated when signed out, editor when signed in, session
survives refresh. `verify.sh` reports `VERIFY OK`.

### Files changed

```
src/Community.Web.Client/Pages/Profile.fs        (new)
src/Community.Web.Client/wwwroot/main.html        (+ Profile, ProfileSignedOut)
src/Community.Web.Client/App/App.fs               (+ ProfilePage, ProfileMsg)
src/Community.Web.Client/Ui/Layout.fs             (+ Profile menu + body case)
src/Community.Web.Client/Community.Web.Client.fsproj
docs/implementation/progress.yaml                 (phase-13: done)
docs/implementation/index.md                     (regenerated)
docs/implementation/_runbook/phase-13-authentication.md  (this file)
```