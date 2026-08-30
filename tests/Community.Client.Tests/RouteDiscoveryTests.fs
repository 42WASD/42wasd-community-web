namespace Community.Client.Tests

open System
open Microsoft.FSharp.Reflection
open Xunit
open Community.Web.Client.App
open Community.Web.Client.Ui

/// Route-discovery proof: the reflection-generated route list must match
/// what the router itself produces. If a Page case is added without an
/// [<EndPoint>], or the DU drifts, these fail.
module RouteDiscoveryTests =

    [<Fact>]
    let ``every parameterless Page case carries an EndPoint attribute`` () =
        // Not a count assertion — a new Page case is automatically covered:
        // reflection walks whatever cases exist and demands the attribute.
        FSharpType.GetUnionCases(typeof<Page>)
        |> Array.filter (fun c -> c.GetFields().Length = 0)
        |> Array.iter (fun c ->
            Assert.True(
                c.GetCustomAttributes(typeof<Bolero.EndPointAttribute>)
                    |> Array.isEmpty |> not,
                $"{c.Name} has no [<EndPoint>] attribute"))

    [<Fact>]
    let ``router.getRoute round-trips every discovered route`` () =
        FSharpType.GetUnionCases(typeof<Page>)
        |> Array.filter (fun c -> c.GetFields().Length = 0)
        |> Array.iter (fun case ->
            let page = FSharpValue.MakeUnion(case, [||]) :?> Page
            let url = router.getRoute page
            // The root endpoint legitimately serializes to "".
            Assert.True(url <> null, $"{case.Name}: getRoute returned null")
            // setRoute must resolve the serialized URL back to the case.
            match router.setRoute url with
            | Some p -> Assert.Equal(page, p)
            | None -> Assert.True(false, $"{case.Name}: '{url}' did not round-trip"))

    [<Fact>]
    let ``navItems covers every routable Page case`` () =
        // Every Page value must be reachable through the layout's nav —
        // guards against a routable-but-orphaned page.
        let navPages =
            Layout.navItems
            |> List.collect (function
                | Layout.NavItem.Leaf (p, _, _) -> [ p ]
                | Layout.NavItem.Group (_, _, leaves) -> leaves |> List.map (fun (p, _, _) -> p))
        let allPages =
            FSharpType.GetUnionCases(typeof<Page>)
            |> Array.choose (fun case ->
                if case.GetFields().Length = 0 then
                    Some (FSharpValue.MakeUnion(case, [||]) :?> Page)
                else None)
        for p in allPages do
            // NotFound is the branded 404 dead-end (Router.withNotFound) —
            // intentionally NOT a nav destination.
            if p <> Page.NotFound then
                Assert.True(
                    navPages |> List.contains p,
                    $"Page case {p} is not reachable from Layout.navItems")
