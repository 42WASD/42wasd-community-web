namespace Community.Web.Client.State

open System
open Community.Web.Client.State
open Community.Web.Shared.Domain

/// Persistent, cross-page application state — the "Shared" layer of the root
/// model. Entities are stored once, normalized in maps keyed by id, so any
/// number of pages read the same canonical cache (per the reference design).
type SharedModel =
    {
        games: RemoteData<Map<string, Game>>
        servers: RemoteData<Map<string, GameServer>>
        tournaments: RemoteData<Map<string, Tournament>>
        news: RemoteData<Map<string, News>>
        players: RemoteData<Map<string, Player>>
        teams: RemoteData<Map<string, Team>>
        account: option<string>
        /// The set of game ids the signed-in user has favourited (a shared
        /// cross-feature value: set on the Games page, reflected on Home).
        favoriteGames: Set<string>
        error: string option
        signInFailed: bool
        /// True while a profile save is in flight — drives the Save button's
        /// busy spinner (RadzenButton IsBusy).
        profileSaving: bool
        /// True briefly after the signed-in player's profile is saved, driving
        /// the "Profile saved" confirmation on the Account page.
        profileSaved: bool
        /// Set when a profile save fails (e.g. read-only data store) so the
        /// user sees why it didn't persist, instead of a silent success.
        profileError: string option
    }

/// Helpers for building and querying shared state.
module SharedModel =
    /// The initial shared state: nothing loaded yet, not signed in.
    let init =
        {
            games = NotAsked
            servers = NotAsked
            tournaments = NotAsked
            news = NotAsked
            players = NotAsked
            teams = NotAsked
            account = None
            favoriteGames = Set.empty
            error = None
            signInFailed = false
            profileSaving = false
            profileSaved = false
            profileError = None
        }

    /// Build a `Map<string, 'T>` from an array of entities keyed by id.
    let indexById (entities: 'T[]) (getId: 'T -> string) =
        entities
        |> Array.fold (fun (m: Map<string, 'T>) e -> m.Add(getId e, e)) Map.empty

    /// The entities of a normalized cache, in id order, discarding the keys.
    /// A common select: `Map.toArray m |> Array.map snd`.
    let values (m: Map<string, 'T>) : 'T[] =
        m |> Map.toArray |> Array.map snd