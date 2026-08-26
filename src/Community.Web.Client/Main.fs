module Community.Web.Client.Main

open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.Extensions.DependencyInjection
open Radzen
open Community.Web.Client.App
open Community.Web.Client.Pages
open Community.Web.Client.Ui.Layout
open Community.Web.Shared.Remoting

/// The single root ProgramComponent. Content is defined by the Elmish
/// program composed in App (state + update + router) and Ui (view).
type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        let communityApi = this.Remote<CommunityApi>()
        // Pure, unit-tested state transition (tests call App.update directly).
        let pureUpdate = update communityApi
        // Service-aware wrapper: keeps App.update pure/testable, and layers
        // imperative UI services (Notification/Dialog) on top as effects. The
        // services are resolved here (where the IServiceProvider is available)
        // and run as Elmish commands — never inside pure update/view (design.md).
        let services = this.Services
        let notification =
            services.GetService<NotificationService>()
        let notify (summary: string) (detail: string) =
            Elmish.Cmd.ofEffect (fun _ -> notification.Notify(NotificationSeverity.Success, summary, detail))
        let update message model =
            let model', cmd = pureUpdate message model
            let effect =
                match message with
                | TournamentsMsg (Tournaments.ToggleRegistration _) ->
                    notify "Registration updated" "The tournament's registration status changed."
                | _ -> Cmd.none
            model', Cmd.batch [ cmd; effect ]
        let program =
            Program.mkProgram (fun _ -> initModel, App.initCmd)
                update view
            |> Program.withRouter router
#if DEBUG
        // Dev-only Elmish tracing. The message trace runs in the BROWSER console,
        // not the server terminal.
        program
        |> Program.withConsoleTrace
        |> Program.withTrace (fun (msg: Message) (model: Model) _subs ->
            // Max-explicit trace: log every message + the resulting page so the
            // browser console shows the full MVU flow (per AGENTS.md: trace the
            // dispatch log before fixing a runtime issue).
            printfn "TRACE msg=%A -> page=%A" msg model.page)
        |> Program.withErrorHandler (fun (msg, exn) ->
            printfn $"Elmish error after %A{msg}: {exn}")
        |> Program.withTermination
            (fun _ -> false)
            (fun _ -> printfn "Program terminated.")
        |> Program.withHotReload
#else
        program
#endif