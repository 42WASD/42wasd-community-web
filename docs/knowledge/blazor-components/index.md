# Blazor Components

> Source: *Blazor for ASP.NET Web Forms Developers* — ch 4 (project structure),
> ch 6 (build reusable UI components), ch 7 (pages, routing, layouts),
> ch 9 (forms and validation).

A Blazor app is a tree of **components**. A component is a self-contained
chunk of UI that owns its state, its rendering logic, its event handlers, and
its lifecycle. This is the direct analogue of Web Forms user controls
(`.ascx`) — but components are plain .NET types, run on the client *or* the
server, and render through a diffable tree rather than the DOM directly.

**Concept map of this page**

```mermaid
mindmap
  root((Blazor components))
    Razor syntax
      @expression / @() explicit
      @code members
      Directives
        @page @layout @inject
        @inherits @implements @typeparam
      Directive attributes
        @onclick @bind @key @ref
    Composition
      Parameters
      ChildContent RenderFragment
      Templated RenderFragment<T>
      EventCallback
    Lifecycle
      OnInitialized
      OnParametersSet
      OnAfterRender
      IDisposable
    State change
      RenderTree diffing
      StateHasChanged
      ErrorBoundary
    Pages & routing
      @page route templates
      Router / RouteView
      NavigationManager
      Layouts (nested)
    Forms & validation
      EditForm / EditContext
      Input* components
      DataAnnotationsValidator
```

## Razor: the component authoring language

Razor is a lightweight templating language based on HTML + C#. Each `.razor`
file compiles into a .NET class (named from the file, namespaced from the
project + folder path or `@namespace`). The class captures the component's
state, rendering logic, lifecycle methods, and event handlers.

- `@` transitions from markup into C#; `@(expr)` delimits an explicit
  expression.
- Control flow is just C#: `@if (…) { … }`, `@foreach (var item in items) { … }`.
- `@code { … }` adds class members (fields, methods, properties) — the
  analogue of Web Forms' `<script runat="server">` block.

### Directives (compile-time controls)

| Directive | Purpose | Web Forms equivalent |
| --- | --- | --- |
| `@page "/product/{id}"` | route for the component | `<%@ Page %>` |
| `@layout MainLayout` | layout component | `MasterPageFile="~/Site.Master"` |
| `@inject IJSRuntime JS` | inject a service | — (via DI) |
| `@inherits Base` | base class | `Inherits=` on Control directive |
| `@implements IDisposable` | implement interface | code-behind |
| `@typeparam TItem` | generic type parameter | code-behind |
| `@using NS` / `@attribute [X]` / `@namespace NS` | namespace / attribute | `@import`, class attributes |

Common `@using` directives live in `_Imports.razor`, which applies to all
`.razor` files in its folder and subfolders.

### Directive attributes (runtime wiring)

| Attribute | Purpose |
| --- | --- |
| `@onclick="Handler"` | DOM event handler (`@on{event}` family; optional `MouseEventArgs e` arg; lambda or method group; sync or `async Task`) |
| `@bind="username"` | two-way data binding (defaults to `onchange`; `@bind:event="oninput"` to change) |
| `@key="person"` | hint for the diffing algorithm to preserve elements in collections |
| `@ref="field"` | capture a component or element reference |
| `@attributes="dict"` | splat a dictionary of attributes |

## Using and composing components

Components are used like HTML tags matching the type name: `<Counter />`. No
element prefix and no registration needed — if the assembly is referenced and
the namespace is in scope (`@using` or fully-qualified
`<MyComponentLib.Counter />`), the component is usable. Think of components
exactly as .NET types.

### Parameters

Flow data *into* a component via public properties marked `[Parameter]`,
set as attributes: `<Counter IncrementAmount="10" />`. Route and query-string
values can also feed parameters:

- route: `@page "/product/{id:int}"` binds `{id}` to a matching
  `[Parameter]` (constraints like `:int` supported; case-insensitive match).
- query: `[Parameter] [SupplyParameterFromQuery(Name = "IncBy")] public int IncrementAmount …`.

### Child content and template parameters

A `[Parameter] RenderFragment ChildContent` captures the component's child
markup; `RenderFragment<T>` is a *templated* fragment invoked per item (the
modern equivalent of Web Forms `Repeater`/`DataList` templated controls).
Generic components declare `@typeparam TItem` and receive the type argument
via an attribute (`TItem="string"`); the item context variable can be renamed
with `Context="message"`.

### Component-defined events

A component exposes events to its parent with a
`[Parameter] EventCallback<T>` property. EventCallbacks support the same
handler shapes as DOM events (sync/async, lambdas, method groups).

## State change and the render tree

Components do **not** render to the DOM. They render to an in-memory
**RenderTree**; Blazor diffs the new output against the previous output and
applies the minimal DOM update. (Elm's equivalent is its virtual-DOM diffing —
see [elm-architecture](../elm-architecture/index.md).)

- After a UI event handler runs, the component re-renders automatically.
- **Async handlers render twice**: immediately after the synchronous part
  (so `Loading…` UI can appear while the task runs) and again when the
  `Task` completes.
- State changed *outside* a normal event (e.g. a service pushes an update)
  requires a manual `StateHasChanged()` call — commonly wired to a service's
  `OnChange` event in `OnInitialized`.
- Blazor runs every component on a `SynchronizationContext` — a single logical
  thread of execution for lifecycle methods and event callbacks.

### Error boundaries

Wrap components in `<ErrorBoundary>` to contain unhandled exceptions to that
subtree instead of failing the whole page, optionally with custom
`<ErrorContent>`.

## Component lifecycle

All lifecycle methods have sync and async variants. **Rendering itself is
synchronous** — async work belongs in lifecycle methods, never in rendering.

| Method | When | Web Forms analogue |
| --- | --- | --- |
| `OnInitialized(Async)` | once, when the component is first created | `Page_Load` |
| `OnParametersSet(Async)` | after parameters are assigned, on init and every render | — |
| `OnAfterRender(Async firstRender)` | after render completes; element refs populated; JS interop safe here | — |
| `Dispose()` via `@implements IDisposable` | removed from UI | `Page_UnLoad` |

Notes:
- `OnAfterRender` is *not* called during server prerendering.
- Component references (`@ref`) are populated after render; mutating child
  state directly via refs is discouraged because it bypasses automatic
  re-rendering.
- Element references are opaque — they exist to pass into JavaScript interop,
  not for direct DOM manipulation (Blazor owns the DOM via diffing).

## Code-behind

A `.razor` file can be split: `Counter.razor` + `Counter.razor.cs`, where the
`.cs` file defines `public class CounterBase : ComponentBase` and the markup
declares `@inherits CounterBase`. Members must be `protected` or `public`.
The designer-generated `.designer.cs` of Web Forms disappears — Razor
compilation output lives in `obj/`.

## Pages, routing, and layouts

There is no `.aspx`-style page file. **A page is a component with a route.**

- `@page "/counter"` assigns the route explicitly (never inferred from file
  location; `Pages/` is only a convention).
- Route templates use `{param}` placeholders with optional constraints:
  `@page "/product/{id:int}"`.
- Routing is **client-side**: the root `<Router AppAssembly=…>` component
  discovers routable components, intercepts navigation, and renders
  `<RouteView>` (with `DefaultLayout`) on a match or the `<NotFound>` content
  otherwise. Deep links hit the server first, get routed to the app's host
  page (`MapFallbackToPage("/_Host")`), then route client-side.
- `NavigationManager` (injectable) gets the current/base address, triggers
  `NavigateTo`, and notifies on address changes — there is no
  `Response.Redirect` because Blazor is not request-reply.

### Layouts

A layout component inherits `LayoutComponentBase` and renders `@Body` where
pages appear — the analogue of Master Pages (`Site.Master` +
`ContentPlaceHolder`). Applied with `@layout MainLayout`, per folder via
`_Imports.razor`, or as the Router's `DefaultLayout`. Layouts can be *nested*
(a layout may itself use `@layout`), giving the multi-placeholder effect.
Root `<html>`/`<body>` elements live in the **host page**
(`_Host.cshtml` for Server, `wwwroot/index.html` for WebAssembly), not in
layouts. Components cannot render `<script>` tags — scripts belong in the
host page because scripts load once and can't be dynamically re-rendered.

## Forms and validation

Blazor shares one validation model between client and server: **the model
type + data annotations**. (DMMF makes the deeper point that the best
validation is making invalid states unrepresentable in the type itself — see
[functional-design-and-types](../functional-design-and-types/index.md); here
the same instinct appears as declarative rules attached to the model rather
than scattered code.)

- Input components bind to model properties and render typed HTML:
  `InputText`, `InputNumber`, `InputDate`, `InputSelect`, `InputCheckbox`,
  `InputTextArea`.
- `<EditForm Model="@model">` orchestrates validation through an
  `EditContext`; add `<DataAnnotationsValidator />` for annotation-based
  rules; `<ValidationSummary />` shows all errors,
  `<ValidationMessage For="() => model.Prop" />` shows one field's error.
- Submit events: `OnValidSubmit`, `OnInvalidSubmit`, or a manual `OnSubmit`.
- Annotations used: `[Required]`, `[StringLength(16, ErrorMessage=…)]`,
  `[Range(1, 100000, …)]`, `[DataType]`, custom boolean-range rules, etc.

A submitted form does not persist anything by itself: a WebAssembly app must
POST the data to a server API, a Server app must call a persistence path —
see [remote-data-and-security](../remote-data-and-security/index.md) and
[persistence-and-evolution](../persistence-and-evolution/index.md).

## Project structure essentials (context for components)

- `.razor` files compile into one assembly; **no runtime UI compilation**.
- Only `wwwroot/` is web-addressable (static files elsewhere are not exposed).
- Server app entry: `Program.cs` builds the host, registers services
  (`builder.Services.AddServerSideBlazor()`), maps endpoints
  (`MapBlazorHub`, `MapFallbackToPage`). WebAssembly entry: registers root
  components (`builder.RootComponents.Add<App>("#app")`) and DI services,
  no HTTP server.
- Host page render modes: `RenderMode.Server` (interactive over connection),
  `ServerPrerendered` (static HTML first, then interactive), `Static`.
- Hot Reload updates running apps live, retaining page state — the inner loop
  Web Forms developers had with the designer.

## Cross-links

- Rendering events → state → re-render is the same unidirectional loop as
  The Elm Architecture: [elm-architecture](../elm-architecture/index.md).
- The ViewModel analogue (bindable state + change notification) is covered in
  [mvvm-patterns](../mvvm-patterns/index.md).
- Injected services, configuration, and where components live relative to
  hosting: [blazor-app-services](../blazor-app-services/index.md).
- Route parameters as *typed* data (`{id:int}`) is a small example of
  parsing input at the boundary: [workflows-and-error-handling](../workflows-and-error-handling/index.md).
