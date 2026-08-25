namespace Community.Web.Server

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Hosting
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

type CommunityApiService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Community.Web.Shared.Remoting.CommunityApi>()

    let games = Loaders.loadJson<Game> env "games.json"
    let servers = Loaders.loadJson<GameServer> env "servers.json"
    let tournaments = Loaders.loadJson<Tournament> env "tournaments.json"
    let news = Loaders.loadJson<News> env "news.json"
    let players = Loaders.loadJson<Player> env "players.json"
    let teams = Loaders.loadJson<Team> env "teams.json"

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
        }