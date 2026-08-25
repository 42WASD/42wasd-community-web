module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Community.Web.Client.App
open Community.Web.Shared.Domain

/// The single shared layout template. Keeps the global `Ui/` folder small:
/// only cross-feature UI lives here (per the reference design). Page-specific
/// views will move beside their page in a later phase.
type Layout = Template<"wwwroot/main.html">

let homePage (model: Model) (dispatch: Message -> unit) =
    Layout.Home()
        .Games(cond model.games <| function
            | None -> Layout.EmptyData().Elt()
            | Some games -> forEach games <| fun g -> tr { td { g.name }; td { g.genre } })
        .Servers(cond model.servers <| function
            | None -> Layout.EmptyData().Elt()
            | Some servers -> forEach servers <| fun s -> tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() } })
        .Tournaments(cond model.tournaments <| function
            | None -> Layout.EmptyData().Elt()
            | Some ts -> forEach ts <| fun t -> tr { td { t.name }; td { t.prize } })
        .News(cond model.news <| function
            | None -> Layout.EmptyData().Elt()
            | Some ns -> forEach ns <| fun n -> tr { td { n.title }; td { n.publishedAt.ToString("yyyy-MM-dd") } })
        .Elt()

let gamesPage (model: Model) (dispatch: Message -> unit) =
    Layout.Games()
        .Rows(cond model.games <| function
            | None -> Layout.EmptyData().Elt()
            | Some games -> forEach games <| fun g ->
                tr { td { g.name }; td { g.genre }; td { g.description } })
        .Elt()

let serversPage (model: Model) (dispatch: Message -> unit) =
    Layout.Servers()
        .Rows(cond model.servers <| function
            | None -> Layout.EmptyData().Elt()
            | Some servers -> forEach servers <| fun s ->
                tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() }; td { s.status } })
        .Elt()

let tournamentsPage (model: Model) (dispatch: Message -> unit) =
    Layout.Tournaments()
        .Rows(cond model.tournaments <| function
            | None -> Layout.EmptyData().Elt()
            | Some ts -> forEach ts <| fun t ->
                tr { td { t.name }; td { t.prize }; td { t.startsAt.ToString("yyyy-MM-dd") } })
        .Elt()

let membersPage (model: Model) (dispatch: Message -> unit) =
    Layout.Members()
        .Rows(cond model.players <| function
            | None -> Layout.EmptyData().Elt()
            | Some players -> forEach players <| fun p ->
                tr { td { p.username }; td { defaultArg p.discord "" } })
        .Elt()

let aboutPage (model: Model) (dispatch: Message -> unit) =
    Layout.About().Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Layout.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view (model: Model) (dispatch: Message -> unit) =
    Layout()
        .Menu(concat {
            menuItem model Home "Home"
            menuItem model Games "Games"
            menuItem model Servers "Servers"
            menuItem model Tournaments "Tournaments"
            menuItem model Members "Members"
            menuItem model About "About"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Games -> gamesPage model dispatch
            | Servers -> serversPage model dispatch
            | Tournaments -> tournamentsPage model dispatch
            | Members -> membersPage model dispatch
            | About -> aboutPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty()
            | Some err ->
                Layout.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()