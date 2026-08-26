module Community.Web.Server.Program

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Hosting.StaticWebAssets
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open Community.Web
open Bolero.Templating.Server
open Radzen

#nowarn 20 // Ignore the return value of app and builder methods

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddMvc()
    builder.Services.AddServerSideBlazor()
    builder.Services.AddAuthorization()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie()
    builder.Services.AddBoleroRemoting<CommunityApiService>()
    builder.Services.AddBoleroHost(server = false)
    // The Bolero server prerenders the page via the server DI container, and
    // the shared layout hosts RadzenComponents (Dialog/Notification/Tooltip).
    // Register Radzen services server-side too, or prerendering throws
    // "No registered service of type 'Radzen.DialogService'".
    builder.Services.AddRadzenComponents() |> ignore
#if DEBUG
    builder.Services.AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Community.Web.Client")
#endif

    let app = builder.Build()

#if DEBUG
    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration)
#endif

    if app.Environment.IsDevelopment() then
        app.UseWebAssemblyDebugging()

    app.UseBlazorFrameworkFiles()
    app.UseAuthentication()
    app.UseStaticFiles()
    app.UseRouting()
    app.UseAuthorization()
#if DEBUG
    app.UseHotReload()
#endif

    app.MapStaticAssets()
    app.MapBoleroRemoting()
    app.MapBlazorHub()
    app.MapFallbackToBolero(Index.page)

    app.Run()
    0
