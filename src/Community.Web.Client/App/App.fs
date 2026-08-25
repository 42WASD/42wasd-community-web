module Community.Web.Client.App

open System
open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Community.Web.Client.State
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting

/// Routing endpoints — the six gaming-community pages plus the Account page.
/// The Account case carries a PageModel: transient sign-in draft state that
/// must NOT appear in the URL (Phase 8: stateful page — PageModel).
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/games">] Games
    | [<EndPoint "/servers">] Servers
    | [<EndPoint "/tournaments">] Tournaments
    | [<EndPoint "/members">] Members
    | [<EndPoint "/about">] About
    | [<EndPoint "/account">] Account of PageModel<AccountForm>

/// Transient, page-local state for the Account page (the sign-in form draft).
/// Lives in a PageModel so it is excluded from the URL, persists across
/// in-page updates, and resets when the page is navigated to fresh
/// (per the state-lifetime rule).
and AccountForm =
    {
        username: string
        password: string
    }

/// The root model is the single source of truth: Page (active route) and
/// Shared (persistent cross-page state). Transient page state lives in the
/// active Page's PageModel, not in the root model.
type Model =
    {
        page: Page
        shared: SharedModel
    }

let initModel =
    {
        page = Home
        shared = SharedModel.init
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
    // Sign-in success refreshes the member list (players) so the Members page
    // reflects the newly authenticated member, then clears the sign-in form.
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

    // --- Account page: transient state lives in PageModel<AccountForm>. ---
    // The PageModel is a mutable holder shared with the view; we update it in
    // place with Router.definePageModel so in-page edits persist across
    // re-renders but are never written to the URL.
    | SetUsername s ->
        match model.page with
        | Account pm -> Router.definePageModel pm { pm.Model with username = s }
        | _ -> ()
        model, Cmd.none
    | SetPassword s ->
        match model.page with
        | Account pm -> Router.definePageModel pm { pm.Model with password = s }
        | _ -> ()
        model, Cmd.none
    | ClearLoginForm ->
        match model.page with
        | Account pm -> Router.definePageModel pm { pm.Model with username = ""; password = "" }
        | _ -> ()
        model, Cmd.none

    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with shared = { model.shared with account = username } }, onSignIn username
    | SendSignIn ->
        match model.page with
        | Account pm ->
            model, Cmd.OfAsync.either remote.signIn (pm.Model.username, pm.Model.password) RecvSignIn Error
        | _ -> model, Cmd.none
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
///
/// inferWithModel supplies a default PageModel for the Account page: a fresh
/// empty AccountForm each time the route is entered (per the state-lifetime
/// rule, transient page state resets on fresh navigation).
let router =
    let defaultPageModel = function
        | Account pm -> Router.definePageModel pm { username = ""; password = "" }
        | _ -> ()
    Router.inferWithModel SetPage (fun model -> model.page) defaultPageModel
    |> Router.withNotFound Home