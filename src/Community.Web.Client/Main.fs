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
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif