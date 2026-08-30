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
    | [<EndPoint "/inbox">] InboxPage of PageModel<Inbox.Model>
    | [<EndPoint "/teams">] Teams
    | [<EndPoint "/about">] About
    | [<EndPoint "/account">] AccountPage of PageModel<Account.Model>
    | [<EndPoint "/404">] NotFound

/// Shared messages — data loading plus session/auth. The reference says do
/// NOT split Shared into sub-unions prematurely; it only has a handful of
/// cases, so a single Shared.Msg is right here.
module Shared =

    /// The kinds of server-loaded canonical caches. One per shared entity.
    type DataKind =
        | Games
        | Servers
        | Tournaments
        | News
        | Players
        | Teams

    /// A loaded cache payload: which kind plus the raw server array. The
    /// update normalizes it into the id-keyed map for that kind.
    type LoadedData =
        | GamesLoaded of Game[]
        | ServersLoaded of GameServer[]
        | TournamentsLoaded of Tournament[]
        | NewsLoaded of News[]
        | PlayersLoaded of Player[]
        | TeamsLoaded of Team[]

    type Msg =
        | Load of DataKind
        | DataLoaded of LoadedData
        | ToggleTournament of string
        | ToggleFavoriteGame of string
        | GetSignedInAs
        | RecvSignedInAs of option<string>
        | SendSignIn of string * string
        | RecvSignIn of option<string>
        | SendSignOut
        | RecvSignOut
        | SaveProfile of string option * string option
        | RecvSaveProfile of bool
        | Error of exn
        | ClearError

    /// Dispatch a server-load command for one data kind, and mark that cache
    /// Loading. This collapses the six hand-written Get*/Got* pairs into one.
    let private loadCmd remote (k: DataKind) =
        let setLoading (shared: SharedModel) =
            match k with
            | Games -> { shared with games = Loading }
            | Servers -> { shared with servers = Loading }
            | Tournaments -> { shared with tournaments = Loading }
            | News -> { shared with news = Loading }
            | Players -> { shared with players = Loading }
            | Teams -> { shared with teams = Loading }
        // Each kind issues its own remote call but shares the DataLoaded handler.
        // `fetch` collapses the six hand-repeated Cmd.OfAsync.either lines.
        let fetch call wrap =
            Cmd.OfAsync.either call () (fun xs -> DataLoaded (wrap xs)) Error
        let cmd =
            match k with
            | Games -> fetch remote.getGames GamesLoaded
            | Servers -> fetch remote.getServers ServersLoaded
            | Tournaments -> fetch remote.getTournaments TournamentsLoaded
            | News -> fetch remote.getNews NewsLoaded
            | Players -> fetch remote.getPlayers PlayersLoaded
            | Teams -> fetch remote.getTeams TeamsLoaded
        setLoading, cmd

    /// Apply a DataLoaded payload to the matching cache slice.
    let private applyLoaded (shared: SharedModel) (loaded: LoadedData) =
        match loaded with
        | GamesLoaded g -> { shared with games = Loaded (SharedModel.indexById g (fun x -> x.id)) }
        | ServersLoaded s -> { shared with servers = Loaded (SharedModel.indexById s (fun x -> x.id)) }
        | TournamentsLoaded t -> { shared with tournaments = Loaded (SharedModel.indexById t (fun x -> x.id)) }
        | NewsLoaded n -> { shared with news = Loaded (SharedModel.indexById n (fun x -> x.id)) }
        | PlayersLoaded p -> { shared with players = Loaded (SharedModel.indexById p (fun x -> x.id)) }
        | TeamsLoaded t -> { shared with teams = Loaded (SharedModel.indexById t (fun x -> x.id)) }

    /// Update shared state for a Shared.Msg. Pure: takes the SharedModel and
    /// returns the new SharedModel + a command of Shared.Msg (the caller lifts
    /// it into the root with Cmd.map SharedMsg).
    let update remote (shared: SharedModel) (msg: Msg) =
        match msg with
        | Load k ->
            let setLoading, cmd = loadCmd remote k
            setLoading shared, cmd
        | DataLoaded loaded -> applyLoaded shared loaded, Cmd.none

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
                | Some _ -> Cmd.ofMsg (Load Players)
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
            // updated handle/bio. The result carries success/failure so a
            // failed write is surfaced to the user, not silently dropped.
            let cmd =
                Cmd.batch [
                    Cmd.OfAsync.either remote.saveProfile (handle, bio) RecvSaveProfile Error
                    Cmd.ofMsg (Load Players)
                ]
            // Reset any prior profile feedback while the save is in flight.
            { shared with profileSaving = true; profileSaved = false; profileError = None }, cmd
        | RecvSaveProfile ok ->
            if ok then
                { shared with profileSaving = false; profileSaved = true }, Cmd.none
            else
                let err = Some "Profile could not be saved (the server data store is read-only)."
                { shared with profileSaving = false; profileError = err }, Cmd.none

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
        /// Servers page: the selected game segment of the segmented control
        /// (audit #18/#19). None = first game in manifest order.
        serversSelected: string option
        /// Games page: the selected genre filter chip (interactive Chip,
        /// audit #27). None = show all genres.
        gamesGenre: string option
        /// Games page: free-text search over name/genre (42-audit #13).
        gamesSearch: string
        /// Games page: sort key "name" | "players" | "favourites" (42-audit #14).
        gamesSort: string
        /// Header notification-inbox popup open state (bell in the top-right).
        inboxOpen: bool
    }

let initModel =
    { page = Home
      shared = SharedModel.init
      sidebarOpen = false
      serversSelected = None
      gamesGenre = None
      gamesSearch = ""
      gamesSort = "name"
      inboxOpen = false }

/// The root message is an orchestration boundary, not an event dump.
/// Nested messages are composed into the root and lifted with Cmd.map:
///   - SharedMsg carries the shared-layer messages (data + session/auth)
///   - AccountMsg carries the Account page's local messages
/// (reference: message-organization + the-root-message).
type Message =
    | SetPage of Page
    | SetSidebarOpen of bool
    | SetInboxOpen of bool
    | SharedMsg of Shared.Msg
    | AccountMsg of Account.Msg
    | MembersMsg of Members.Msg
    | InboxMsg of Inbox.Msg
    | TournamentsMsg of Tournaments.Msg
    | GamesMsg of Games.Msg
    | SelectServerGame of string option
    | SelectServerDetail of string
    | MemberDetail of string
    | SetGameGenre of string option
    | SetGameSearch of string
    | SetGameSort of string

/// The startup command: load the shared, cross-page caches in parallel and
/// resolve the initial session. Kept as a single named binding (rather than an
/// inline list in Main) so the program composition stays declarative and the
/// full startup intent is visible in one place.
let initCmd =
    Cmd.batch [
        Cmd.ofMsg (SharedMsg Shared.GetSignedInAs)
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.Games))
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.Servers))
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.Tournaments))
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.News))
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.Players))
        Cmd.ofMsg (SharedMsg (Shared.Load Shared.Teams))
    ]

let update remote message model =
    match message with
    | SetPage page ->
        // Closing the nav drawer on navigation is intuitive: picking a link
        // dismisses the drawer on mobile. The transient "Profile saved" / save
        // error also resets on navigation (state-lifetime rule).
        let shared = { model.shared with profileSaved = false; profileError = None }
        { model with page = page; sidebarOpen = false; inboxOpen = false; shared = shared }, Cmd.none

    | SetSidebarOpen open' ->
        { model with sidebarOpen = open' }, Cmd.none

    | SetInboxOpen open' ->
        { model with inboxOpen = open' }, Cmd.none

    | SelectServerGame gameId ->
        { model with serversSelected = gameId }, Cmd.none

    | SetGameGenre genre ->
        { model with gamesGenre = genre }, Cmd.none

    | SetGameSearch q ->
        { model with gamesSearch = q }, Cmd.none

    | SetGameSort k ->
        { model with gamesSort = k }, Cmd.none

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
        // in-page updates via the shared PageModel holder. Login is
        // interpreted by the root: it issues a Shared session message — a
        // cross-feature effect translated by the parent (Phase 14, kept minimal).
        match model.page with
        | AccountPage pm ->
            match msg with
            | Account.Login (username, password, rememberMe) ->
                // A single login intent carries both fields + remember-me
                // (42-audit #28/#36). The mock backend keeps sessions
                // ephemeral; the flag is threaded for the host to persist.
                let send =
                    Cmd.ofMsg (SharedMsg (Shared.SendSignIn (username, password)))
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

    | InboxMsg msg ->
        // The Inbox feature's local update (its search draft) — same
        // PageModel pattern as Members.
        match model.page with
        | InboxPage pm ->
            let m, cmd = Inbox.update msg pm.Model
            Router.definePageModel pm m
            model, Cmd.map InboxMsg cmd
        | _ -> model, Cmd.none

    | TournamentsMsg msg ->
        // A cross-feature effect: the Tournaments feature does not mutate
        // shared state directly. It emits its own local message, and the root
        // translates it into a shared effect message that the Shared layer
        // owns (reference: "a shared update is dispatched, not reached into").
        // Tournaments owns no local Model, so every message maps straight to a
        // shared/UI effect. ToggleRegistration → Shared.ToggleTournament;
        // ViewDetails is a pure UI intent handled as an imperative effect in
        // Main.fs (no state change here), so it maps to Cmd.none and the
        // dialog is opened by the service-aware wrapper.
        match msg with
        | Tournaments.ToggleRegistration tournamentId ->
            model, Cmd.ofMsg (SharedMsg (Shared.ToggleTournament tournamentId))
        | Tournaments.ViewDetails _ ->
            model, Cmd.none

    | GamesMsg msg ->
        // Same cross-feature pattern as Tournaments: the Games feature emits
        // its own local message and the root translates it into a shared
        // effect (favourite games), never mutating Shared directly.
        match msg with
        | Games.ToggleFavorite gameId ->
            model, Cmd.ofMsg (SharedMsg (Shared.ToggleFavoriteGame gameId))

    // Detail-dialog intents. The pure update only records the selection in
    // the model; the imperative side effect (opening the Radzen side dialog)
    // is layered on top in Main.fs (services wrapper).
    | SelectServerDetail serverId ->
        { model with serversSelected = Some serverId }, Cmd.none
    | MemberDetail playerId ->
        model, Cmd.none

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
        | InboxPage pm -> Router.definePageModel pm Inbox.init
        | _ -> ()
    Router.inferWithModel SetPage (fun model -> model.page) defaultPageModel
    |> Router.withNotFound NotFound