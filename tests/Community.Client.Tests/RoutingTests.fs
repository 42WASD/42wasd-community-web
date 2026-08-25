namespace Community.Client.Tests

open Xunit
open Community.Web.Client.App

/// Tests for the routing round-trips and the not-found fallback. The router in
/// App is a Router<Page, Model, Message> exposing:
///   router.setRoute : string -> option<Page>
///   router.getRoute : Page -> string
module RoutingTests =

    [<Fact>]
    let ``root path resolves to Home`` () =
        // Bolero returns base-relative paths (no leading slash).
        match router.setRoute "" with
        | Some Page.Home -> ()
        | _ -> Assert.True(false, "expected Home")

    [<Fact>]
    let ``games path resolves to Games`` () =
        match router.setRoute "games" with
        | Some Page.Games -> ()
        | _ -> Assert.True(false, "expected Games")

    [<Fact>]
    let ``account path resolves to AccountPage`` () =
        match router.setRoute "account" with
        | Some (Page.AccountPage _) -> ()
        | _ -> Assert.True(false, "expected AccountPage")

    [<Fact>]
    let ``unknown path does not crash`` () =
        match router.setRoute "definitely-not-a-route" with
        | _ -> ()

    [<Fact>]
    let ``getRoute serializes the Games endpoint back to games`` () =
        Assert.Equal("games", router.getRoute Page.Games)

    [<Fact>]
    let ``setRoute and getRoute round-trip`` () =
        match router.setRoute "games" with
        | Some page -> Assert.Equal("games", router.getRoute page)
        | None -> Assert.True(false, "expected a route")
