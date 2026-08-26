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
        // Self-contained SVG favicons (no embedded <style> classes, which
        // Chrome/Edge ignore when loading the tab icon). Start grey/inactive;
        // favicon.js swaps to the pink active icon once the app has hydrated
        // and the skeletons are gone. The `?v=` query busts Chromium/Edge's
        // aggressive favicon cache so a stale default icon is never shown.
        link { attr.rel "icon"; attr.``type`` "image/svg+xml"; attr.href "/favicon-inactive.svg?v=5" }
        link { attr.rel "alternate icon"; attr.``type`` "image/x-icon"; attr.href "/favicon-inactive.ico?v=5" }
        link { attr.rel "apple-touch-icon"; attr.href "/favicon.svg?v=5" }
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
        // Swap the grey inactive tab icon → pink active icon once the app
        // has hydrated (skeletons gone). Pure JS, no interop.
        script { attr.src "js/favicon.js" }
    }
}
