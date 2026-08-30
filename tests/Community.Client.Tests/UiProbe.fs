namespace Community.Client.Tests

open System
open System.IO
open System.Text.Json
open Microsoft.Extensions.DependencyInjection
open Microsoft.FSharp.Reflection
open Bolero
open Bolero.Html
open Bunit
open Community.Web.Client.App
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Client.Ui
open Community.Web.Shared.Domain
open Xunit

/// F#-native discovery of routes + interactive UX paths, without a browser.
///
/// Both generators evaluate the code DYNAMICALLY (no AST parsing — static
/// parsing can't resolve state-driven conditional rendering):
///
/// 1. ROUTES — F# reflection over the Page DU. Every union case with no
///    fields is instantiated and serialized with the router itself
///    (router.getRoute), so URLs come from the same source the app
///    navigates with — they can never drift from the code.
///
/// 2. UX PATHS — dynamic evaluation of the real view functions. App.update
///    (with the stub API from TestData) is driven with each interaction's
///    message; the resulting Model is rendered by Layout.view into a Bolero
///    Node tree, hosted in a minimal Bolero Component and rendered by bUnit.
///    bUnit yields a queryable AngleSharp DOM (FindAll) — every button,
///    link and input that the REAL view code emits in that state is
///    extracted with its text/href/class, i.e. exactly the selectors the
///    Playwright capture engine needs.
///
/// Output: ux-paths.json (routes + per-state interactive element maps),
/// consumed by scripts/visual/discover_capture.py --capture-only.
module UiProbe =

    // ------------------------------------------------------------------
    // Part 1 — route discovery via reflection + the router
    // ------------------------------------------------------------------

    /// Every Page case instantiated via reflection. Parameterized cases
    /// (MembersPage/InboxPage/AccountPage of PageModel<_>) are built with
    /// Router.noModel — exactly what Layout.navItems uses for links.
    let allPages : Page list =
        FSharpType.GetUnionCases(typeof<Page>)
        |> Array.map (fun c ->
            let args: obj[] =
                if c.GetFields().Length = 0 then [||]
                // Each PageModel-carrying case gets ITS OWN noModel type.
                elif c.Name = "AccountPage" then
                    [| box (Router.noModel<Account.Model>) |]
                elif c.Name = "InboxPage" then
                    [| box (Router.noModel<Inbox.Model>) |]
                else [| box (Router.noModel<Members.Model>) |]
            FSharpValue.MakeUnion(c, args) :?> Page)
        |> Array.toList

    /// Route table: { route, page } for every Page case.
    let discoverRoutes () : JsonElement =
        allPages
        |> List.map (fun page ->
            {| route = router.getRoute page
               page =
                   match page with
                   | MembersPage _ -> "MembersPage"
                   | AccountPage _ -> "AccountPage"
                   | p -> string p |})
        |> List.sortBy (fun r -> r.route)
        |> Array.ofList
        |> JsonSerializer.SerializeToElement

    // ------------------------------------------------------------------
    // Part 2 — dynamic view evaluation with bUnit (headless render)
    // ------------------------------------------------------------------

    /// Current node to render. bUnit v1 requires parameterless components,
    /// so the caller stashes the node here and NodeHost picks it up.
    let mutable private currentNode: Node option = None

    /// Host: a minimal parameterless Bolero Component that renders the
    /// stashed Node tree.
    type internal NodeHost() =
        inherit Component()
        override _.Render() =
            match currentNode with
            | Some n -> n
            | None -> failwith "UiProbe.currentNode not set"

    /// Extract every interactive element from a rendered bUnit component.
    let private extractInteractives
        (rendered: IRenderedComponent<NodeHost>) =
        [ for el in rendered.FindAll("button, a[href], input, textarea, [role=tab]") do
            let text =
                if String.IsNullOrWhiteSpace el.TextContent then ""
                else el.TextContent.Trim().Replace("\n", " ")
            {| tag = el.TagName.ToLower()
               text = text
               id = el.Id
               ``class`` = el.ClassName
               href = el.GetAttribute("href")
               ``type`` = el.GetAttribute("type") |} ]

    /// Render one Model via the real Layout.view and extract its
    /// interactive elements. Returns the state record for ux-paths.json.
    let renderModel (model: Model) (label: string) =
        // The context must outlive the query — created here, disposed here.
        currentNode <- Some (Layout.view model ignore)
        use ctx = new TestContext()
        // Radzen components fire JS setup calls (createCarousel, …) in
        // OnAfterRender; Loose mode returns defaults instead of throwing —
        // we only need the rendered DOM, not live JS behaviour.
        ctx.JSInterop.Mode <- JSRuntimeMode.Loose
        // The layout hosts RadzenUI.components (RadzenComponents), whose
        // children inject Radzen's imperative services — register them
        // exactly as the app's Startup does (AddRadzenComponents).
        Radzen.ServiceCollectionExtensions.AddRadzenComponents(
            ctx.Services) |> ignore
        let rendered = ctx.RenderComponent<NodeHost>()
        let result =
            {| label = label
               route = router.getRoute model.page
               page = string model.page
               interactives = extractInteractives rendered |}
        currentNode <- None
        result

    // ------------------------------------------------------------------
    // Part 3 — the state catalogue (dynamically evaluated, never static)
    // ------------------------------------------------------------------

    /// Drive the REAL App.update to reach each interaction state. The stub
    /// API never resolves; every cache slice is fed its DataLoaded response
    /// directly (TestData.loadedShared) so conditional rendering resolves to
    /// real cards/tabs — not skeletons — and rendering is deterministic.
    let discoverStates () =
        let api = TestData.stubApi
        let loadedShared = TestData.loadedShared

        // Base page states — every route with all data caches loaded.
        let baseStates =
            [ Home; Games; Servers; Tournaments
              MembersPage Router.noModel; InboxPage Router.noModel; Teams; About
              AccountPage Router.noModel ]
            |> List.map (fun page ->
                renderModel { initModel with page = page
                                             shared = loadedShared }
                    (string page))

        // Interaction states — one per cross-feature effect. Each starts
        // from the OWNING page (the page the interaction happens on) with
        // the fully-loaded cache.
        let favState =
            let m, _ =
                update api (GamesMsg (Games.ToggleFavorite "game-1"))
                    { initModel with page = Page.Games; shared = loadedShared }
            renderModel m "games-favorited"

        let regState =
            let m, _ =
                update api (TournamentsMsg (Tournaments.ToggleRegistration "t-1"))
                    { initModel with page = Page.Tournaments
                                     shared = loadedShared }
            renderModel m "tournaments-registration-closed"

        // Account: signed-in profile editor + failed sign-in alert state.
        let accountPm : PageModel<Account.Model> = { Model = Account.init }
        let accountPage =
            { initModel with page = Page.AccountPage accountPm
                             shared = loadedShared }

        let signedIn =
            let m, _ =
                update api (SharedMsg (Shared.RecvSignIn (Some "demo")))
                    accountPage
            renderModel m "account-signed-in"

        let failedSignIn =
            let failedShared =
                { accountPage.shared with signInFailed = true }
            let m, _ =
                update api (SharedMsg (Shared.RecvSignIn None))
                    { accountPage with shared = failedShared }
            renderModel m "account-login-failed"

        [ favState; regState; signedIn; failedSignIn ] @ baseStates

    /// Full document: routes + states.
    let buildDocument () =
        {| routes = discoverRoutes ()
           states = discoverStates () |}

    /// Emit ux-paths.json (routes + per-state interactive element maps).
    let emitUxPaths (path: string) : unit =
        File.WriteAllText(path, JsonSerializer.Serialize(buildDocument ()))

    // ------------------------------------------------------------------
    // Tests — the discovery pipeline IS the test suite
    // ------------------------------------------------------------------

    type UiProbeTests() =

        [<Fact>]
        member _.``ux-paths.json is emitted with routes and states``() =
            let out = Path.Combine(AppContext.BaseDirectory, "ux-paths.json")
            emitUxPaths out
            Assert.True(File.Exists(out))
            let doc = JsonDocument.Parse(File.ReadAllText(out))
            Assert.True(doc.RootElement.GetProperty("routes").GetArrayLength() > 0)
            Assert.True(doc.RootElement.GetProperty("states").GetArrayLength() > 0)

        [<Fact>]
        member _.``games state exposes a Favourite button after data load``() =
            let doc = buildDocument ()
            let games = doc.states |> List.find (fun s -> s.label = "games-favorited")
            let texts = games.interactives |> List.map (fun i -> i.text)
            Assert.Contains(texts, fun t -> t.Contains("Favourite"))
