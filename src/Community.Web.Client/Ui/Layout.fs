module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Community.Web.Client.App
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Shared.Domain

/// The single shared layout template. Keeps the global `Ui/` folder small:
/// only cross-feature UI lives here (per the reference design). Page-specific
/// views will move beside their page in a later phase.
type Layout = Template<"wwwroot/main.html">

/// Render a RemoteData<Map<string,'T>> as table rows via a row renderer.
let dataRows (rd: RemoteData<Map<string, 'T>>) (render: 'T -> Node) =
    cond rd <| function
        | NotAsked | Loading -> Layout.EmptyData().Elt()
        | Loaded m -> forEach (Map.toArray m) (fun (_, t) -> render t)
        | Failed _ -> Layout.EmptyData().Elt()

/// Compute community stats from shared state (0 when not loaded yet).
let stats (shared: SharedModel) =
    let gameCount =
        match shared.games with
        | Loaded m -> m.Count
        | _ -> 0
    let onlineNow =
        match shared.servers with
        | Loaded m -> m.Values |> Seq.sumBy (fun s -> s.onlinePlayers)
        | _ -> 0
    let openTournaments =
        match shared.tournaments with
        | Loaded m -> m.Values |> Seq.filter (fun t -> t.registrationOpen) |> Seq.length
        | _ -> 0
    let memberCount =
        match shared.players with
        | Loaded m -> m.Count
        | _ -> 0
    gameCount, onlineNow, openTournaments, memberCount

let homePage (model: Model) (dispatch: Message -> unit) =
    let gamesCount, onlineNow, openTournaments, memberCount = stats model.shared
    Layout.Home()
        .GamesCount(gamesCount.ToString())
        .OnlineNow(onlineNow.ToString())
        .OpenTournaments(openTournaments.ToString())
        .MembersCount(memberCount.ToString())
        .Games(dataRows model.shared.games <| fun g -> tr { td { g.name }; td { g.genre } })
        .Servers(dataRows model.shared.servers <| fun s -> tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() } })
        .Tournaments(dataRows model.shared.tournaments <| fun t -> tr { td { t.name }; td { t.prize } })
        .News(dataRows model.shared.news <| fun n -> tr { td { n.title }; td { n.publishedAt.ToString("yyyy-MM-dd") } })
        .Elt()

let gamesPage (model: Model) (dispatch: Message -> unit) =
    Layout.Games()
        .Rows(dataRows model.shared.games <| fun g ->
            tr { td { g.name }; td { g.genre }; td { g.description } })
        .Elt()

let serversPage (model: Model) (dispatch: Message -> unit) =
    Layout.Servers()
        .Rows(dataRows model.shared.servers <| fun s ->
            tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() }; td { s.status } })
        .Elt()

let tournamentsPage (model: Model) (dispatch: Message -> unit) =
    Layout.Tournaments()
        .Rows(dataRows model.shared.tournaments <| fun t ->
            tr { td { t.name }; td { t.prize }; td { t.startsAt.ToString("yyyy-MM-dd") } })
        .Elt()

let membersPage (model: Model) (dispatch: Message -> unit) =
    Layout.Members()
        .Rows(dataRows model.shared.players <| fun p ->
            tr { td { p.username }; td { defaultArg p.discord "" } })
        .Elt()

let aboutPage (model: Model) (dispatch: Message -> unit) =
    Layout.About().Elt()

/// The Account page renders either the sign-in form (transient form state held
/// in the page's own Account feature Model via PageModel) or the signed-in
/// account banner with a Sign out button. The form's messages are nested
/// (Account ...) and the session messages are nested (Shared ...).
let accountPage (model: Model) (dispatch: Message -> unit) =
    cond model.shared.account <| function
    | Some username ->
        Layout.AccountSignedIn()
            .Username(username)
            .SignOut(fun _ -> dispatch (SharedMsg Shared.SendSignOut))
            .Elt()
    | None ->
        match model.page with
        | AccountPage pm ->
            Layout.SignIn()
                .Username(pm.Model.username, fun s -> dispatch (AccountMsg (Account.SetUsername s)))
                .Password(pm.Model.password, fun s -> dispatch (AccountMsg (Account.SetPassword s)))
                .SignIn(fun _ -> dispatch (AccountMsg Account.Submit))
                .ErrorNotification(
                    cond model.shared.signInFailed <| function
                    | false -> empty()
                    | true ->
                        Layout.ErrorNotification()
                            .HideClass("is-hidden")
                            .Text("Sign in failed. Use any username and the password \"password\".")
                            .Elt()
                )
                .Elt()
        | _ -> Layout.SignIn().Elt()

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
            menuItem model (AccountPage Router.noModel) "Account"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Games -> gamesPage model dispatch
            | Servers -> serversPage model dispatch
            | Tournaments -> tournamentsPage model dispatch
            | Members -> membersPage model dispatch
            | About -> aboutPage model dispatch
            | AccountPage _ -> accountPage model dispatch
        )
        .Error(
            cond model.shared.error <| function
            | None -> empty()
            | Some err ->
                Layout.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch (SharedMsg Shared.ClearError))
                    .Elt()
        )
        .Elt()