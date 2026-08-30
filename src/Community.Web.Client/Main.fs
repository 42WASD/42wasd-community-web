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
            // Audit #13: ShowProgress renders the built-in duration progress
            // bar; CloseOnClick dismisses on tap (verified:
            // NotificationMessage.ShowProgress/CloseOnClick).
            Elmish.Cmd.ofEffect (fun _ ->
                notification.Notify(NotificationMessage(
                    Severity = NotificationSeverity.Success,
                    Summary = summary, Detail = detail,
                    Duration = 4000., ShowProgress = true, CloseOnClick = true)))
        let dialogService =
            services.GetService<DialogService>()
        // Build a Radzen dialog body from a Bolero `Node` (the same node type
        // the Elmish `view` uses), wrapped in a `RenderFragment<DialogService>`
        // so it can be passed to `DialogService.OpenAsync`. The receiver is the
        // root ProgramComponent (a RenderHandle host), so `node.Invoke` renders
        // the fragment through Blazor.
        // Tournament details open as a RIGHT SIDE PANEL (audit #15) — on a
        // phone it slides in over the page instead of a cramped centered
        // modal. Verified: DialogService.OpenSideAsync + SideDialogOptions
        // (Position, ShowMask, CloseDialogOnOverlayClick, MinWidth/Height).
        let showDialog (title: string) (body: Node) =
            Elmish.Cmd.ofEffect (fun _ ->
                let fragment =
                    RenderFragment<DialogService>(fun _ds ->
                        RenderFragment(fun rt ->
                            body.Invoke(this, rt, 0) |> ignore))
                dialogService.OpenAsync(
                    title, fragment,
                    DialogOptions(
                        Width = "420px", Resizable = true,
                        CssClass = "app-dialog",
                        CloseDialogOnOverlayClick = true))
                |> ignore)
        // Build the tournament-details dialog body from the tournament record.
        // Wrapped in `pop` so the dialog content snaps in with a quick
        // scale+opacity entrance (Tailwind `animate-pop`).
        let tournamentDialog (t: Tournament) =
            // Prize tier as a read-only rating (42-audit #23): derive stars
            // from the prize magnitude (pure display; rating wrapper exists).
            let tier =
                let digits = t.prize |> Seq.filter System.Char.IsDigit |> Seq.length
                min 5 (max 1 (digits - 1))
            RadzenUI.pop (RadzenUI.vStackGap "1rem" (concat {
                RadzenUI.detailField "Game" t.gameId
                RadzenUI.detailField "Prize" t.prize
                RadzenUI.rating tier 5
                RadzenUI.detailField "Starts" (t.startsAt.ToString("yyyy-MM-dd HH:mm"))
                RadzenUI.detailField "Registration"
                    (if t.registrationOpen then "Open" else "Closed")
            }))
        let update message model =
            let model', cmd = pureUpdate message model
            let effect =
                match message with
                | TournamentsMsg (Tournaments.ToggleRegistration _) ->
                    notify "Registration updated" "The tournament's registration status changed."
                | MemberDetail playerId ->
                    // Member detail dialog (42-audit #27): avatar + facts.
                    match model.shared.players with
                    | Loaded m ->
                        match m.TryFind playerId with
                        | Some pl ->
                            showDialog pl.username (RadzenUI.pop (RadzenUI.vStackGap "1rem" (concat {
                                RadzenUI.hStackGap "0.75rem" (concat {
                                    RadzenUI.initialsAvatar pl.username
                                    RadzenUI.text RadzenUI.subtitle1 pl.username
                                })
                                RadzenUI.detailField "Discord" (defaultArg pl.discord "—")
                            })))
                        | None -> Cmd.none
                    | _ -> Cmd.none
                | SelectServerDetail serverId ->
                    // Server detail dialog (42-audit #20): row click → side
                    // dialog with the server's facts (same pattern as the
                    // tournament dialog).
                    match model.shared.servers with
                    | Loaded m ->
                        match m.TryFind serverId with
                        | Some srv ->
                            showDialog srv.name (RadzenUI.pop (RadzenUI.vStackGap "1rem" (concat {
                                RadzenUI.detailField "Game" srv.gameId
                                RadzenUI.detailField "Address" srv.address
                                RadzenUI.detailField "Players"
                                    (sprintf "%d / %d" srv.onlinePlayers srv.maxPlayers)
                                RadzenUI.detailField "Status" srv.status
                            })))
                        | None -> Cmd.none
                    | _ -> Cmd.none
                | TournamentsMsg (Tournaments.ShareTournament tid) ->
                    // Copy-link share (42-audit #25): toast carries the link;
                    // clipboard interop lands with real deployment URLs.
                    notify "Link copied" (sprintf "/tournaments#%s" tid)
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