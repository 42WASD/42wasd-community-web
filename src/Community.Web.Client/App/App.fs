module Community.Web.Client.App

open System
open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Community.Web.Client.State
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

/// Ephemeral, page-local state (not persisted across navigation unless it
/// lives in Shared). For now: the login form input.
type LocalModel =
    {
        username: string
        password: string
    }

/// The root model is the single source of truth: Page (active route),
/// Shared (persistent cross-page state), Local (active page's ephemeral state).
type Model =
    {
        page: Page
        shared: SharedModel
        local: LocalModel
    }

let initModel =
    {
        page = Home
        shared = SharedModel.init
        local = { username = ""; password = "" }
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

let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.batch [ Cmd.ofMsg GetPlayers; Cmd.ofMsg ClearLoginForm ]
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | GetGames ->
        let cmd = Cmd.OfAsync.either remote.getGames () GotGames Error
        { model with shared = { model.shared with games = Loading } }, cmd
    | GotGames games ->
        { model with shared = { model.shared with games = Loaded (SharedModel.indexById games (fun g -> g.id)) } }, Cmd.none

    | GetServers ->
        let cmd = Cmd.OfAsync.either remote.getServers () GotServers Error
        { model with shared = { model.shared with servers = Loading } }, cmd
    | GotServers servers ->
        { model with shared = { model.shared with servers = Loaded (SharedModel.indexById servers (fun s -> s.id)) } }, Cmd.none

    | GetTournaments ->
        let cmd = Cmd.OfAsync.either remote.getTournaments () GotTournaments Error
        { model with shared = { model.shared with tournaments = Loading } }, cmd
    | GotTournaments tournaments ->
        { model with shared = { model.shared with tournaments = Loaded (SharedModel.indexById tournaments (fun t -> t.id)) } }, Cmd.none

    | GetNews ->
        let cmd = Cmd.OfAsync.either remote.getNews () GotNews Error
        { model with shared = { model.shared with news = Loading } }, cmd
    | GotNews news ->
        { model with shared = { model.shared with news = Loaded (SharedModel.indexById news (fun n -> n.id)) } }, Cmd.none

    | GetPlayers ->
        let cmd = Cmd.OfAsync.either remote.getPlayers () GotPlayers Error
        { model with shared = { model.shared with players = Loading } }, cmd
    | GotPlayers players ->
        { model with shared = { model.shared with players = Loaded (SharedModel.indexById players (fun p -> p.id)) } }, Cmd.none

    | SetUsername s ->
        { model with local = { model.local with username = s } }, Cmd.none
    | SetPassword s ->
        { model with local = { model.local with password = s } }, Cmd.none
    | ClearLoginForm ->
        { model with local = { username = ""; password = "" } }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with shared = { model.shared with account = username } }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.local.username, model.local.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with
            shared = { model.shared with account = username; signInFailed = Option.isNone username }
        }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with shared = { model.shared with account = None; signInFailed = false } }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with shared = { model.shared with error = Some "You have been logged out."; account = None } }, Cmd.none
    | Error exn ->
        { model with shared = { model.shared with error = Some exn.Message } }, Cmd.none
    | ClearError ->
        { model with shared = { model.shared with error = None } }, Cmd.none

/// Connects the routing system to the Elmish application.
/// Unknown/wrong URLs fall back predictably to the Home page.
let router =
    Router.infer SetPage (fun model -> model.page)
    |> Router.withNotFound Home