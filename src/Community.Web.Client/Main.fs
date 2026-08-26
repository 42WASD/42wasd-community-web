module Community.Web.Client.Main

open Elmish
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Rendering
open Radzen
open Community.Web.Client.App
open Community.Web.Client.Pages
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Client.Ui.Layout
open Community.Web.Shared.Domain
open Community.Web.Shared.Remoting
open Bolero.Html

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
        let dialogService =
            services.GetService<DialogService>()
        // Build a Radzen dialog body from a Bolero `Node` (the same node type
        // the Elmish `view` uses), wrapped in a `RenderFragment<DialogService>`
        // so it can be passed to `DialogService.OpenAsync`. The receiver is the
        // root ProgramComponent (a RenderHandle host), so `node.Invoke` renders
        // the fragment through Blazor.
        let showDialog (title: string) (body: Node) =
            Elmish.Cmd.ofEffect (fun _ ->
                let fragment =
                    RenderFragment<DialogService>(fun _ds ->
                        RenderFragment(fun rt ->
                            body.Invoke(this, rt, 0) |> ignore))
                dialogService.OpenAsync(title, fragment, DialogOptions(Width = "520px", Resizable = true))
                |> ignore)
        // Build the tournament-details dialog body from the tournament record.
        let tournamentDialog (t: Tournament) =
            RadzenUI.vStackGap "1rem" (concat {
                RadzenUI.detailField "Game" t.gameId
                RadzenUI.detailField "Prize" t.prize
                RadzenUI.detailField "Starts" (t.startsAt.ToString("yyyy-MM-dd HH:mm"))
                RadzenUI.detailField "Registration"
                    (if t.registrationOpen then "Open" else "Closed")
            })
        let update message model =
            let model', cmd = pureUpdate message model
            let effect =
                match message with
                | TournamentsMsg (Tournaments.ToggleRegistration _) ->
                    notify "Registration updated" "The tournament's registration status changed."
                | TournamentsMsg (Tournaments.ViewDetails tournamentId) ->
                    // Look up the tournament from the canonical shared cache and
                    // open its detail dialog imperatively (a UI effect, not state).
                    match model.shared.tournaments with
                    | Loaded m ->
                        match m.TryFind tournamentId with
                        | Some t -> showDialog t.name (tournamentDialog t)
                        | None -> Cmd.none
                    | _ -> Cmd.none
                | _ -> Cmd.none
            model', Cmd.batch [ cmd; effect ]
        let program =
            Program.mkProgram (fun _ -> initModel, App.initCmd)
                update view
            |> Program.withRouter router
#if ELMISH_TRACE
        // Elmish tracing. The message trace runs in the BROWSER console,
        // not the server terminal. Enabled via the ELMISH_TRACE define so it
        // survives Release/AOT publish (where the Members/Server bugs only
        // reproduce) — the old `#if DEBUG` guard stripped it from every
        // published build, leaving no MVU log to debug against.
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
#else
        program
        |> Program.withErrorHandler (fun (msg, exn) ->
            printfn $"Elmish error after %A{msg}: {exn}")
#endif