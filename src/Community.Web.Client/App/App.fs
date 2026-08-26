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
    | [<EndPoint "/members">] MembersPage of PageModel<Members.Model>
    | [<EndPoint "/teams">] Teams
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
        | GetTeams
        | GotTeams of Team[]
        | ToggleTournament of string
        | ToggleFavoriteGame of string
        | GetSignedInAs
        | RecvSignedInAs of option<string>
        | SendSignIn of string * string
        | RecvSignIn of option<string>
        | SendSignOut
        | RecvSignOut
        | SaveProfile of string option * string option
        | RecvSaveProfile
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

        | GetTeams ->
            let cmd = Cmd.OfAsync.either remote.getTeams () GotTeams Error
            { shared with teams = Loading }, cmd
        | GotTeams teams ->
            { shared with teams = Loaded (SharedModel.indexById teams (fun t -> t.id)) }, Cmd.none

        | ToggleTournament tournamentId ->
            // A shared effect: flip registrationOpen in the canonical
            // tournaments cache. Other pages reading that cache (e.g. Home's
            // "open tournaments" stat) reflect the change immediately — the
            // cross-feature verification.
            let tournaments =
                match shared.tournaments with
                | Loaded m ->
                    match m.TryFind tournamentId with
                    | Some t ->
                        let t' = { t with registrationOpen = not t.registrationOpen }
                        Loaded (m.Add(tournamentId, t'))
                    | None -> Loaded m
                | other -> other
            { shared with tournaments = tournaments }, Cmd.none

        | ToggleFavoriteGame gameId ->
            // A shared effect: add/remove the game id in the favourite set.
            // Home's "favourite games" stat reads the same set, so it reflects
            // the change immediately (cross-feature verification).
            let favorites =
                if shared.favoriteGames.Contains gameId then
                    shared.favoriteGames.Remove gameId
                else
                    shared.favoriteGames.Add gameId
            { shared with favoriteGames = favorites }, Cmd.none

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

        | SaveProfile (handle, bio) ->
            // Cross-feature effect: save the signed-in player's profile and
            // refresh the canonical Players cache so Members reflects the
            // updated handle/bio.
            let cmd =
                Cmd.batch [
                    Cmd.OfAsync.either remote.saveProfile (handle, bio) (fun () -> RecvSaveProfile) Error
                    Cmd.ofMsg GetPlayers
                ]
            shared, cmd
        | RecvSaveProfile ->
            { shared with profileSaved = true }, Cmd.none

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
        /// Whether the mobile navigation drawer (RadzenSidebar) is open. Only
        /// used on small screens; desktop uses the horizontal header menu.
        sidebarOpen: bool
    }

let initModel =
    { page = Home
      shared = SharedModel.init
      sidebarOpen = false }

/// The root message is an orchestration boundary, not an event dump.
/// Nested messages are composed into the root and lifted with Cmd.map:
///   - SharedMsg carries the shared-layer messages (data + session/auth)
///   - AccountMsg carries the Account page's local messages
/// (reference: message-organization + the-root-message).
type Message =
    | SetPage of Page
    | SetSidebarOpen of bool
    | SharedMsg of Shared.Msg
    | AccountMsg of Account.Msg
    | MembersMsg of Members.Msg
    | TournamentsMsg of Tournaments.Msg
    | GamesMsg of Games.Msg

/// The startup command: load the shared, cross-page caches in parallel and
/// resolve the initial session. Kept as a single named binding (rather than an
/// inline list in Main) so the program composition stays declarative and the
/// full startup intent is visible in one place.
let initCmd =
    Cmd.batch [
        Cmd.ofMsg (SharedMsg Shared.GetSignedInAs)
        Cmd.ofMsg (SharedMsg Shared.GetGames)
        Cmd.ofMsg (SharedMsg Shared.GetServers)
        Cmd.ofMsg (SharedMsg Shared.GetTournaments)
        Cmd.ofMsg (SharedMsg Shared.GetNews)
        Cmd.ofMsg (SharedMsg Shared.GetPlayers)
        Cmd.ofMsg (SharedMsg Shared.GetTeams)
    ]

let update remote message model =
    match message with
    | SetPage page ->
        // Closing the nav drawer on navigation is intuitive: picking a link
        // dismisses the drawer on mobile. The transient "Profile saved"
        // confirmation also resets on navigation (state-lifetime rule).
        { model with page = page; sidebarOpen = false; shared = { model.shared with profileSaved = false } }, Cmd.none

    | SetSidebarOpen open' ->
        { model with sidebarOpen = open' }, Cmd.none

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
            | Account.SaveProfile ->
                // Cross-feature effect translated by the root: issue a Shared
                // profile-save (owned by the Shared layer) with the handle/bio
                // drafted on this page. `Cmd.map` is not needed — the shared
                // message is lifted as SharedMsg.
                let send =
                    Cmd.ofMsg (SharedMsg (Shared.SaveProfile (Some pm.Model.handle, Some pm.Model.bio)))
                model, send
            | _ ->
                let m, cmd = Account.update msg pm.Model
                Router.definePageModel pm m
                model, Cmd.map AccountMsg cmd
        | _ -> model, Cmd.none

    | MembersMsg msg ->
        // The Members feature's local update runs against its own Model, held
        // in the route's PageModel (same transient-state pattern as Account).
        match model.page with
        | MembersPage pm ->
            let m, cmd = Members.update msg pm.Model
            Router.definePageModel pm m
            model, Cmd.map MembersMsg cmd
        | _ -> model, Cmd.none

    | TournamentsMsg msg ->
        // A cross-feature effect: the Tournaments feature does not mutate
        // shared state directly. It emits its own local message, and the root
        // translates it into a shared effect message that the Shared layer
        // owns (reference: "a shared update is dispatched, not reached into").
        // Tournaments owns no local Model, so every message maps straight to a
        // Shared.ToggleTournament.
        match msg with
        | Tournaments.ToggleRegistration tournamentId ->
            model, Cmd.ofMsg (SharedMsg (Shared.ToggleTournament tournamentId))

    | GamesMsg msg ->
        // Same cross-feature pattern as Tournaments: the Games feature emits
        // its own local message and the root translates it into a shared
        // effect (favourite games), never mutating Shared directly.
        match msg with
        | Games.ToggleFavorite gameId ->
            model, Cmd.ofMsg (SharedMsg (Shared.ToggleFavoriteGame gameId))

/// Connects the routing system to the Elmish application.
/// Unknown/wrong URLs fall back predictably to the Home page.
///
/// inferWithModel supplies a default PageModel for the Account page: a fresh
/// empty Account.Model each time the route is entered (per the state-lifetime
/// rule, transient page state resets on fresh navigation).
let router =
    let defaultPageModel = function
        | AccountPage pm -> Router.definePageModel pm Account.init
        | MembersPage pm -> Router.definePageModel pm Members.init
        | _ -> ()
    Router.inferWithModel SetPage (fun model -> model.page) defaultPageModel
    |> Router.withNotFound Home