namespace Community.Web.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting

/// Load a JSON array from a file under the server's data folder.
module private Loaders =
    let loadJson<'a> (env: IWebHostEnvironment) (fileName: string) : 'a[] =
        let json = Path.Combine(env.ContentRootPath, "data", fileName) |> File.ReadAllText
        JsonSerializer.Deserialize<'a[]>(json)

    /// Serialize a JSON array back to a file under the server's data folder.
    /// **Never throws**: a failed write (e.g. read-only filesystem in the
    /// container, where data/ is baked read-only) must not crash the request
    /// or take down the site. The caller decides how to surface the failure.
    /// Returns true on success, false on failure.
    let saveJson<'a> (logger: ILogger) (env: IWebHostEnvironment) (fileName: string) (value: 'a[]) : bool =
        let path = Path.Combine(env.ContentRootPath, "data", fileName)
        try
            let json = JsonSerializer.Serialize(value, JsonSerializerOptions(WriteIndented = true))
            File.WriteAllText(path, json)
            true
        with exn ->
            // Data is baked read-only in production; in-memory state still
            // works, so a failed write is logged, not fatal.
            logger.LogError(exn, "Failed to persist data to {Path}", path)
            false

type CommunityApiService(ctx: IRemoteContext, env: IWebHostEnvironment, logger: ILogger<CommunityApiService>) =
    inherit RemoteHandler<Community.Web.Shared.Remoting.CommunityApi>()

    let games = Loaders.loadJson<Game> env "games.json"
    let servers = Loaders.loadJson<GameServer> env "servers.json"
    let mutable tournaments = Loaders.loadJson<Tournament> env "tournaments.json"
    let news = Loaders.loadJson<News> env "news.json"
    let mutable players = Loaders.loadJson<Player> env "players.json"
    let teams = Loaders.loadJson<Team> env "teams.json"

    /// Replace the signed-in player's record in the in-memory roster and
    /// persist players.json (best-effort, like saveProfile). Returns the
    /// updated record, or None when the caller has no roster entry (the demo
    /// backend signs in ANY username, not only rostered players).
    let updateSignedInPlayer (name: string) (update: Player -> Player) : option<Player> =
        match players |> Array.tryFind (fun p -> p.username = name) with
        | None -> None
        | Some p ->
            let p' = update p
            players <- players |> Array.map (fun x -> if x.id = p'.id then p' else x)
            Loaders.saveJson logger env "players.json" players |> ignore
            Some p'

    override this.Handler =
        {
            getGames = fun () -> async {
                return games
            }

            getServers = fun () -> async {
                return servers
            }

            getTournaments = fun () -> async {
                return tournaments
            }

            setTournamentRegistration = ctx.Authorize <| fun (tournamentId, open') -> async {
                // Shared effect: flip the canonical tournament's registration
                // gate in memory and persist tournaments.json (best-effort).
                tournaments <-
                    tournaments
                    |> Array.map (fun t ->
                        if t.id = tournamentId then { t with registrationOpen = open' } else t)
                return Loaders.saveJson logger env "tournaments.json" tournaments
            }

            getNews = fun () -> async {
                return news
            }

            getPlayers = fun () -> async {
                return players
            }

            getTeams = fun () -> async {
                return teams
            }

            signIn = fun (username, password) -> async {
                if password = "password" then
                    do! ctx.HttpContext.AsyncSignIn(username, TimeSpan.FromDays(365.))
                    return Some username
                else
                    return None
            }

            signOut = fun () -> async {
                return! ctx.HttpContext.AsyncSignOut()
            }

            getUsername = ctx.Authorize <| fun () -> async {
                return ctx.HttpContext.User.Identity.Name
            }

            saveProfile = ctx.Authorize <| fun (handle, bio) -> async {
                let name = ctx.HttpContext.User.Identity.Name
                players <-
                    players
                    |> Array.map (fun p ->
                        if p.username = name then
                            { p with handle = handle; bio = bio }
                        else
                            p)
                // The in-memory cache is always updated (the site stays
                // alive); the file write is best-effort. A failed write
                // (e.g. read-only filesystem) is caught inside saveJson —
                // logged and reported to the caller as false, never an
                // unhandled 500 (Bolero's remoting only catches
                // RemoteUnauthorizedException, so any other exception would
                // crash the request).
                return Loaders.saveJson logger env "players.json" players
            }

            setFavoriteGames = ctx.Authorize <| fun (gameIds) -> async {
                let name = ctx.HttpContext.User.Identity.Name
                return updateSignedInPlayer name (fun p -> { p with favoriteGames = gameIds })
            }

            setReadNews = ctx.Authorize <| fun (newsIds) -> async {
                let name = ctx.HttpContext.User.Identity.Name
                return updateSignedInPlayer name (fun p -> { p with readNews = newsIds })
            }
        }