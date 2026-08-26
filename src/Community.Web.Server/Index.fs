module Community.Web.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Community.Web

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "42WASD Community" }
        ``base`` { attr.href "/" }
        link { attr.rel "icon"; attr.``type`` "image/svg+xml"; attr.href "/42wasd.svg" }
        link { attr.rel "alternate icon"; attr.href "/favicon.ico" }
        link { attr.rel "stylesheet"; attr.href "_content/Radzen.Blazor/css/material-dark-base.css" }
        link { attr.rel "stylesheet"; attr.href "css/fonts.css" }
        link { attr.rel "stylesheet"; attr.href "css/index.css" }
        link { attr.rel "stylesheet"; attr.href "Community.Web.Client.styles.css" }
    }
    body {

        div {
            attr.id "main"
            comp<Client.Main.MyApp>
        }
        boleroScript
        // Radzen interactive components need the Radzen.Blazor.js script.
        script { attr.src "_content/Radzen.Blazor/Radzen.Blazor.min.js" }
    }
}
