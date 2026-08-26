namespace Community.Web.Shared.Remoting

open Bolero.Remoting
open Community.Web.Shared.Domain

/// The remote API contract shared between client and server.
/// The client calls these functions; the server implements them.
///
/// Lives in Community.Web.Shared so both sides compile against the SAME
/// contract type (no client<->server circular dependency). This is the
/// only Bolero dependency in the shared layer — `IRemoteService`/`BasePath`
/// is part of the shared Bolero library, not a server- or client-only concern.
type CommunityApi =
    {
        /// The list of games the community plays.
        getGames: unit -> Async<Game[]>

        /// The active community game servers.
        getServers: unit -> Async<GameServer[]>

        /// Upcoming / open tournaments.
        getTournaments: unit -> Async<Tournament[]>

        /// Latest community news posts.
        getNews: unit -> Async<News[]>

        /// Community players, used by the Members page.
        getPlayers: unit -> Async<Player[]>

        /// Community teams, used by the Teams page.
        getTeams: unit -> Async<Team[]>

        /// Sign into the application.
        signIn: string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername: unit -> Async<string>

        /// Save the signed-in player's profile (display handle + bio).
        /// Returns true if persisted, false if the write failed (e.g. the
        /// data folder is read-only in production). The in-memory state is
        /// still updated on the server either way.
        saveProfile: string option * string option -> Async<bool>

        /// Sign out from the application.
        signOut: unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/api"