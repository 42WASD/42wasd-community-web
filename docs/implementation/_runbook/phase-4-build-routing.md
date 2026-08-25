---
phase: 03-step-by-step-implementation/phase-4-build-routing
---

**Phase 4 complete** — routing is established with a `Page` union bound to the
URL, and unknown URLs fall back predictably to the Home page.

### What this phase required

From the reference design: a `Page` DU bound to routes, `Page` stored in the
root model, a page-changing message, `Router.infer` binding route `<->` Page,
and predictable fallback for wrong URLs.

### Already in place (from Phase 3)

The routing skeleton was largely established while building the shared-domain
slice:

```fsharp
// App/App.fs
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/games">] Games
    | [<EndPoint "/servers">] Servers
    | [<EndPoint "/tournaments">] Tournaments
    | [<EndPoint "/members">] Members
    | [<EndPoint "/about">] About
```

- `Page` lives in the root `Model`.
- `SetPage` is the only message that changes the route.
- `Router.infer SetPage (fun model -> model.page)` maps route `<-> Page`.

### Added in this phase

**Explicit unknown-route fallback.** `Router.infer` sets `notFound = None` by
default; the server `MapFallbackToBolero` shell already served a predictable
HTTP 200 for unknown paths. To make the *client-side* intent explicit and
consistent with the reference ("Wrong URLs fall back predictably"), the router
now declares:

```fsharp
let router =
    Router.infer SetPage (fun model -> model.page)
    |> Router.withNotFound Home
```

So any route that isn't one of the six pages resolves to the Home page instead
of a blank/undefined endpoint.

### Verification

```bash
dotnet build Community.Web.sln      # Build succeeded, 0 warnings, 0 errors
```

Live checks against `http://localhost:5023` (Development):

| Route | Result |
|---|---|
| `/` | 200 |
| `/games` | 200 |
| `/servers` | 200 |
| `/nonexistent-route` | 200 (falls back predictably) |
| `/totally-bogus` | 200 (falls back predictably) |

### Acceptance (from reference spec)

- [x] `Page` is a union whose cases map to routes
- [x] The route lives in the root model
- [x] A single message (`SetPage`) changes the route
- [x] `Router.infer` binds route `<-> Page`
- [x] Wrong URLs fall back predictably (`Router.withNotFound Home` + shell)

### Next

Phase 5 will add root-app orchestration on top of this routing spine.