module Community.Web.Client.App

open System
open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Community.Web.Client.State
open Community.Web.Client.Pages
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting

/// Routing endpoints — the six gaming-community pages plus the Account page.
/// The AccountPage case carries a PageModel holding the Account feature's own
/// Model (Phase 8 + Phase 9): transient sign-in draft state excluded from URL.
/// The case name is `AccountPage` (not `Account`) to avoid colliding with the
/// `Account` feature module.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/games">] Games
    | [<EndPoint "/servers">] Servers
    | [<EndPoint "/tournaments">] Tournaments
    | [<EndPoint "/members">] Members
    | [<EndPoint "/about">] About
    | [<EndPoint "/account">] AccountPage of PageModel<Account.Model>

/// Shared messages — data loading plus session/auth. The reference says do
/// NOT split Shared into sub-unions prematurely; it only has a handful of
/// cases, so a single Shared.Msg is right here.
module Shared =

    type Msg =
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
        | GetSignedInAs
        | RecvSignedInAs of option<string>
        | SendSignIn of string * string
        | RecvSignIn of option<string>
        | SendSignOut
        | RecvSignOut
        | Error of exn
        | ClearError

    /// Update shared state for a Shared.Msg. Pure: takes the SharedModel and
    /// returns the new SharedModel + a command of Shared.Msg (the caller lifts
    /// it into the root with Cmd.map SharedMsg).
    let update remote (shared: SharedModel) (msg: Msg) =
        match msg with
        | GetGames ->
            let cmd = Cmd.OfAsync.either remote.getGames () GotGames Error
            { shared with games = Loading }, cmd
        | GotGames games ->
            { shared with games = Loaded (SharedModel.indexById games (fun g -> g.id)) }, Cmd.none

        | GetServers ->
            let cmd = Cmd.OfAsync.either remote.getServers () GotServers Error
            { shared with servers = Loading }, cmd
        | GotServers servers ->
            { shared with servers = Loaded (SharedModel.indexById servers (fun s -> s.id)) }, Cmd.none

        | GetTournaments ->
            let cmd = Cmd.OfAsync.either remote.getTournaments () GotTournaments Error
            { shared with tournaments = Loading }, cmd
        | GotTournaments tournaments ->
            { shared with tournaments = Loaded (SharedModel.indexById tournaments (fun t -> t.id)) }, Cmd.none

        | GetNews ->
            let cmd = Cmd.OfAsync.either remote.getNews () GotNews Error
            { shared with news = Loading }, cmd
        | GotNews news ->
            { shared with news = Loaded (SharedModel.indexById news (fun n -> n.id)) }, Cmd.none

        | GetPlayers ->
            let cmd = Cmd.OfAsync.either remote.getPlayers () GotPlayers Error
            { shared with players = Loading }, cmd
        | GotPlayers players ->
            { shared with players = Loaded (SharedModel.indexById players (fun p -> p.id)) }, Cmd.none

        | GetSignedInAs ->
            let cmd = Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
            shared, cmd
        | RecvSignedInAs username ->
            { shared with account = username }, Cmd.none

        | SendSignIn (username, password) ->
            let cmd = Cmd.OfAsync.either remote.signIn (username, password) RecvSignIn Error
            shared, cmd
        | RecvSignIn username ->
            let shared =
                { shared with
                    account = username
                    signInFailed = Option.isNone username }
            // Refresh the member list on success (Members reflects the new member).
            let cmd =
                match username with
                | Some _ -> Cmd.ofMsg GetPlayers
                | None -> Cmd.none
            shared, cmd

        | SendSignOut ->
            let cmd = Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
            shared, cmd
        | RecvSignOut ->
            { shared with account = None; signInFailed = false }, Cmd.none

        | Error RemoteUnauthorizedException ->
            { shared with error = Some "You have been logged out."; account = None }, Cmd.none
        | Error exn ->
            { shared with error = Some exn.Message }, Cmd.none
        | ClearError ->
            { shared with error = None }, Cmd.none

/// The root model is the single source of truth: Page (active route) and
/// Shared (persistent cross-page state). The active page's transient state
/// lives in the page's own feature Model (carried by the route's PageModel).
type Model =
    {
        page: Page
        shared: SharedModel
    }

let initModel =
    { page = Home
      shared = SharedModel.init }

/// The root message is an orchestration boundary, not an event dump.
/// Nested messages are composed into the root and lifted with Cmd.map:
///   - SharedMsg carries the shared-layer messages (data + session/auth)
///   - AccountMsg carries the Account page's local messages
/// (reference: message-organization + the-root-message).
type Message =
    | SetPage of Page
    | SharedMsg of Shared.Msg
    | AccountMsg of Account.Msg

let update remote message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | SharedMsg msg ->
        let shared, cmd = Shared.update remote model.shared msg
        // The root orchestrates cross-boundary effects: after a successful
        // sign-in, also clear the Account page's transient form.
        let cmd =
            match msg with
            | Shared.RecvSignIn (Some _) ->
                Cmd.batch [ Cmd.map SharedMsg cmd; Cmd.ofMsg (AccountMsg Account.Clear) ]
            | _ -> Cmd.map SharedMsg cmd
        { model with shared = shared }, cmd

    | AccountMsg msg ->
        // The Account feature's local update runs against its own Model, held
        // in the route's PageModel (Phase 8). Transient state persists across
        // in-page updates via the shared PageModel holder. Submit is
        // interpreted by the root: it issues a Shared session message — a
        // cross-feature effect translated by the parent (Phase 14, kept minimal).
        match model.page with
        | AccountPage pm ->
            match msg with
            | Account.Submit ->
                let send =
                    Cmd.ofMsg (SharedMsg (Shared.SendSignIn (pm.Model.username, pm.Model.password)))
                model, send
            | _ ->
                let m, cmd = Account.update msg pm.Model
                Router.definePageModel pm m
                model, Cmd.map AccountMsg cmd
        | _ -> model, Cmd.none

/// Connects the routing system to the Elmish application.
/// Unknown/wrong URLs fall back predictably to the Home page.
///
/// inferWithModel supplies a default PageModel for the Account page: a fresh
/// empty Account.Model each time the route is entered (per the state-lifetime
/// rule, transient page state resets on fresh navigation).
let router =
    let defaultPageModel = function
        | AccountPage pm -> Router.definePageModel pm Account.init
        | _ -> ()
    Router.inferWithModel SetPage (fun model -> model.page) defaultPageModel
    |> Router.withNotFound Home