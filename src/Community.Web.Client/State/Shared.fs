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
        account: option<string>
        error: string option
        signInFailed: bool
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
            account = None
            error = None
            signInFailed = false
        }

    /// Build a `Map<string, 'T>` from an array of entities keyed by id.
    let indexById (entities: 'T[]) (getId: 'T -> string) =
        entities
        |> Array.fold (fun (m: Map<string, 'T>) e -> m.Add(getId e, e)) Map.empty