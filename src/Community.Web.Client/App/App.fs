module Community.Web.Client.App

open System
open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting

/// Routing endpoints definition — the six gaming-community pages.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/games">] Games
    | [<EndPoint "/servers">] Servers
    | [<EndPoint "/tournaments">] Tournaments
    | [<EndPoint "/members">] Members
    | [<EndPoint "/about">] About

/// The Elmish application's model — the root orchestrates routing and
/// delegates page/session concerns downward (per the reference design).
type Model =
    {
        page: Page
        games: Game[] option
        servers: GameServer[] option
        tournaments: Tournament[] option
        news: News[] option
        players: Player[] option
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

let initModel =
    {
        page = Home
        games = None
        servers = None
        tournaments = None
        news = None
        players = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
    }

/// The Elmish application's messages — a small root namespace, per the
/// reference design. Messages grow where they belong (per page/feature),
/// not as an unbounded flat list at the root.
type Message =
    | SetPage of Page
    | GetGames
    | GotGames of Game[]
    | GetServers
    | GotServers of GameServer[]
    | GetTournaments
    | GotTournaments of Tournament[]
    | GetNews
    | GotNews of News[]
    | GetPlayers
    | GotPlayers of Player[]
    | SetUsername of string
    | SetPassword of string
    | ClearLoginForm
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

/// Load the shared home-page data: games, servers, tournaments, news.
let loadHomeData remote =
    Cmd.batch
        [
            Cmd.OfAsync.either remote.getGames () GotGames Error
            Cmd.OfAsync.either remote.getServers () GotServers Error
            Cmd.OfAsync.either remote.getTournaments () GotTournaments Error
            Cmd.OfAsync.either remote.getNews () GotNews Error
        ]

let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.batch [ Cmd.ofMsg GetPlayers; Cmd.ofMsg ClearLoginForm ]
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | GetGames ->
        let cmd = Cmd.OfAsync.either remote.getGames () GotGames Error
        { model with games = None }, cmd
    | GotGames games ->
        { model with games = Some games }, Cmd.none

    | GetServers ->
        let cmd = Cmd.OfAsync.either remote.getServers () GotServers Error
        { model with servers = None }, cmd
    | GotServers servers ->
        { model with servers = Some servers }, Cmd.none

    | GetTournaments ->
        let cmd = Cmd.OfAsync.either remote.getTournaments () GotTournaments Error
        { model with tournaments = None }, cmd
    | GotTournaments tournaments ->
        { model with tournaments = Some tournaments }, Cmd.none

    | GetNews ->
        let cmd = Cmd.OfAsync.either remote.getNews () GotNews Error
        { model with news = None }, cmd
    | GotNews news ->
        { model with news = Some news }, Cmd.none

    | GetPlayers ->
        let cmd = Cmd.OfAsync.either remote.getPlayers () GotPlayers Error
        { model with players = None }, cmd
    | GotPlayers players ->
        { model with players = Some players }, Cmd.none

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | ClearLoginForm ->
        { model with username = ""; password = "" }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
/// Unknown/wrong URLs fall back predictably to the Home page.
let router =
    Router.infer SetPage (fun model -> model.page)
    |> Router.withNotFound Home