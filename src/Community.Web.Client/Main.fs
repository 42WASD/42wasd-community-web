module Community.Web.Client.Main

open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Community.Web.Client.App
open Community.Web.Client.Ui.Layout
open Community.Web.Shared.Remoting

/// The single root ProgramComponent. Content is defined by the Elmish
/// program composed in App (state + update + router) and Ui (view).
type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        let communityApi = this.Remote<CommunityApi>()
        let update = update communityApi
        let program =
            Program.mkProgram (fun _ ->
                initModel,
                Cmd.batch [
                    Cmd.ofMsg GetSignedInAs
                    Cmd.ofMsg GetGames
                    Cmd.ofMsg GetServers
                    Cmd.ofMsg GetTournaments
                    Cmd.ofMsg GetNews
                    Cmd.ofMsg GetPlayers
                ])
                update view
            |> Program.withRouter router
#if DEBUG
        // Dev-only Elmish tracing. The message trace runs in the BROWSER console,
        // not the server terminal.
        program
        |> Program.withConsoleTrace
        |> Program.withErrorHandler (fun (msg, exn) ->
            printfn $"Elmish error after %A{msg}: {exn}")
        |> Program.withTermination
            (fun _ -> false)
            (fun _ -> printfn "Program terminated.")
        |> Program.withHotReload
#else
        program
#endif