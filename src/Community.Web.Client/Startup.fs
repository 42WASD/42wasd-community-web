namespace Community.Web.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Bolero.Remoting.Client
open Radzen

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<Main.MyApp>("#main")
        builder.Services.AddBoleroRemoting(builder.HostEnvironment) |> ignore
        // Register Radzen services (Dialog, Notification, Tooltip, Theme) per
        // design.md — imperative UI services live on the host, never in pure
        // state/update logic.
        builder.Services.AddRadzenComponents() |> ignore
        builder.Build().RunAsync() |> ignore
        0
