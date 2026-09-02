# Web Forms → Blazor Migration

> Source: *Blazor for ASP.NET Web Forms Developers* — ch 1 (why migrate),
> ch 2 (architecture comparison), ch 11 (modules/handlers/middleware),
> ch 13 (security migration), ch 14 (the migration walkthrough).

The end-to-end "move a Web Forms app to Blazor" guide, with the concept
mapping that explains *why* each step exists.

```mermaid
flowchart LR
    n1(["Migration"])
    n2["Why (or why not)"]
    n3["Web Forms still supported"]
    n4["New features need .NET Core\/Blazor"]
    n5["Span&lt;T&gt;, WebAssembly, cross-platform"]
    n6["Decision is business-driven"]
    n7["Architecture shift"]
    n8["Page-centric request\/reply"]
    n9[".aspx pages, lifecycle events"]
    n10["ViewState postback"]
    n11["Component-centric event-driven"]
    n12["RenderTree diffing"]
    n13["SynchronizationContext"]
    n14["no request-reply"]
    n15["Pipeline mapping"]
    n16["HTTP modules \/ handlers"]
    n17["Global.asax lifecycle"]
    n18["IIS modules"]
    n19["to ASP.NET Core middleware"]
    n20["Security migration"]
    n21["universal providers"]
    n22["aspnet_ tables"]
    n23["ASP.NET Core Identity"]
    n24["claims &amp; policies"]
    n25["schema &amp; data migration"]
    n26["web.config authorization"]
    n27["to attributes &amp; AuthorizeView"]
    n28["Data &amp; config"]
    n29["EF6 on .NET Core"]
    n30["async data access"]
    n31["web.config to appsettings"]
    n32["Page conversion"]
    n33[".aspx + code-behind + designer"]
    n34["to .razor single file"]
    n35["validators to EditForm"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n4 --> n5
    n2 --> n6
    n1 --> n7
    n7 --> n8
    n8 --> n9
    n8 --> n10
    n7 --> n11
    n11 --> n12
    n11 --> n13
    n11 --> n14
    n1 --> n15
    n15 --> n16
    n15 --> n17
    n15 --> n18
    n18 --> n19
    n1 --> n20
    n20 --> n21
    n21 --> n22
    n20 --> n23
    n23 --> n24
    n23 --> n25
    n20 --> n26
    n26 --> n27
    n1 --> n28
    n28 --> n29
    n29 --> n30
    n28 --> n31
    n1 --> n32
    n32 --> n33
    n33 --> n34
    n32 --> n35
```

## Should you migrate at all?

- Web Forms is supported as long as .NET Framework ships with Windows —
  **not mandatory**.
- Migrate when the new platform matters: `Span<T>` performance,
  WebAssembly, Linux/macOS hosting, app-local deployment.
- It's a **business decision**; whole-app or endpoint-by-endpoint.

Two trends behind the book: .NET went **open-source, cross-platform** (Web
Forms is Windows-only, will not be ported) and app logic moved **to the
client** (WebAssembly made .NET-in-browser real in 2020).

## The two architectures

| | **Web Forms** | **Blazor** |
| --- | --- | --- |
| Model | page-centric, HTTP request-reply | component-based, event-driven, stateful |
| UI unit | `.aspx` + `.ascx` controls | `.razor` components (.NET types) |
| Events | postbacks; page lifecycle | DOM event → state → RenderTree diff → patch |
| State | ViewState round-trips (can hit MBs) | in-memory (circuit on Server; JS heap on WASM) |
| Compile | `.aspx` recompiled at runtime | compile-time only; Hot Reload |

Shared DNA (why the mapping works): reusable controls, event-driven,
stateful UI, data-binding ambitions.

## The concept map

| Web Forms | Becomes |
| --- | --- |
| `.aspx` page | `@page` component |
| code-behind + `.designer.cs` | one `.razor` (+ optional `.razor.cs`) |
| Master Page + `ContentPlaceHolder` | layout component + `@Body` |
| User control `.ascx` | Razor component |
| Server controls (`asp:Label`) | plain HTML / input components |
| Validation controls | `EditForm` + `DataAnnotationsValidator` (no custom JS) |
| `Global.asax Application_Start` | `Program.cs` builder + services |
| `Application_BeginRequest` | `app.Use(...)` middleware |
| HTTP modules/handlers | middleware pipeline (order = execution) |
| IIS modules | ASP.NET Core middleware (Status Code Pages, Static Files, Rewriting…) |
| `web.config` appSettings | `appsettings.json` + sources, `IConfiguration` |
| Forms auth + universal providers | ASP.NET Core Identity |
| `<location>` authorization | `[Authorize]`, policies, `AuthorizeView` |
| `Session` / `Application` | circuit state / singleton services (+ backing store) |
| `packages.config` | `PackageReference` (transitive) |
| runtime bundling | build-time tools |
| files anywhere | only `wwwroot/` is addressable |
| `Response.Redirect` | `NavigationManager.NavigateTo` |

## The walkthrough (eShop example)

1. **New SDK-style project**; libraries as `PackageReference`s. Windows-only
   APIs (Registry, WMI) via the **Windows Compatibility Pack** (~20k APIs);
   missing ones = *runtime* errors — test.
2. **Port startup** to `Program.cs`: IoC setup, DB initializer, per-request
   logging → DI registrations + `app.Use` middleware. ⚠️ **Session state is
   not supported** with Blazor Server (connections ≠ HTTP context) — session
   apps need rearchitecting.
3. **Modules/handlers → middleware** (mapping above; Katana experience
   carries over).
4. **Static files & bundling**: `app.UseStaticFiles()`; bundling to build
   time.
5. **Convert pages** — three files (`Details.aspx`, `.aspx.cs`,
   `.aspx.designer.cs`) become one `Details.razor`:
   - `Page_Load` → `OnInitialized` (+ `[Parameter] public int Id` bound from
     `@page "/Catalog/Details/{id:int}"`);
   - `<%# Bind(...) %>` → `@item.Prop`;
   - property injection → `@inject`;
   - `Page.RouteData` → route template.
6. **Validation transfers almost unchanged**: annotated model +
   `<EditForm Model="_item" OnValidSubmit=…><DataAnnotationsValidator/>`
   replaces `RequiredFieldValidator`s
   ([blazor-components](../blazor-components/index.md)).
7. **Configuration**: web.config XML → `appsettings.json` + environments +
   secrets + env vars + CLI. Secrets leave source control.
8. **Data access**: **EF6 is supported** on .NET Core. Required changes:
   connection strings passed explicitly (`name=…` convention gone); sync DB
   access → async (never `Task.Wait()`/`.Result` — thread-pool exhaustion).
9. **.NET Core architectural changes**: no AppDomains, Remoting, Code Access
   Security; **no synchronization context** (no `HttpContext.Current`,
   `Thread.CurrentPrincipal` static accessors); no shadow copying; embrace
   async.

## Security: universal providers → Identity

Universal providers (since ASP.NET 2.0) store users in `aspnet_Users`,
credentials in `aspnet_Membership` (password + salt + lockout), roles in
`aspnet_Roles` — incompatible with Identity. Migration required.

**Identity adds beyond roles**:

- **claims** — name/value pairs describing *what the subject is*;
- **policies** — named requirement sets
  (`RequireClaim(ClaimTypes.Country, "Canada")`) replacing scattered role
  checks;
- `[Authorize(Roles="administrators")]` / `[Authorize(Policy="CanadiansOnly")]`
  via `@attribute` (routable `@page` components only — child components use
  `AuthorizeView`);
- `AuthorizeView` (`<Authorized>` / `<NotAuthorized>`) for declarative UI;
- auth state as `[CascadingParameter] Task<AuthenticationState>`;
  `UserManager<T>`/`RoleManager<T>` (Server only — WebAssembly calls APIs).

**Four steps**:

1. Create the Identity schema (`dotnet new webapp -au Individual` +
   migrations, or `dotnet ef migrations script -o auth.sql`). Map:
   `aspnet_Membership` → `AspNetUsers`, `aspnet_Roles` → `AspNetRoles`.
2. Migrate users/roles. **Passwords are not migrated** — users reset at
   next login (safer anyway).
3. web.config auth → `Program.cs` (`AddDefaultIdentity<IdentityUser>`,
   `UseAuthentication` **then** `UseAuthorization`); `<location>` rules →
   `[Authorize]` per page.
4. `User.IsInRole(...)` conditionals → `AuthorizeView` / policy checks /
   `IAuthorizationService.AuthorizeAsync`.

## Cross-links

- Startup/DI/config details: [blazor-app-services](../blazor-app-services/index.md).
- Component authoring for page conversions: [blazor-components](../blazor-components/index.md).
- Identity claims/policies ↔ MAUI bearer tokens: [remote-data-and-security](../remote-data-and-security/index.md).
- The boundary discipline (explicit connection strings, async, no ambient
  context) is DMMF's: [persistence-and-evolution](../persistence-and-evolution/index.md).
