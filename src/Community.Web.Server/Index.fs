module Community.Web.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open Community.Web

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "Bolero Application" }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.4/css/bulma.min.css" }
        // Radzen dark base theme — the 42 Abu Dhabi palette (Phase 17) is
        // applied on top by overriding --rz-* vars in css/index.css.
        // design.md: load a dark base first, then override vars in our own css.
        link { attr.rel "stylesheet"; attr.href "_content/Radzen.Blazor/css/material-dark-base.css" }
        link { attr.rel "stylesheet"; attr.href "css/index.css" }
        link { attr.rel "stylesheet"; attr.href "Community.Web.Client.styles.css" }
    }
    body {
        nav {
            attr.``class`` "navbar is-dark"
            "role" => "navigation"
            attr.aria "label" "main navigation"
            div {
                attr.``class`` "navbar-brand"
                a {
                    attr.``class`` "navbar-item has-text-weight-bold is-size-5"
                    attr.href "https://fsbolero.io"
                    img { attr.style "height:40px"; attr.src "https://github.com/fsbolero/website/raw/master/src/Website/img/wasm-fsharp.png" }
                    "  Bolero"
                }
            }
        }
        div {
            attr.id "main"
            comp<Client.Main.MyApp>
        }
        boleroScript
        // Radzen interactive components need the Radzen.Blazor.js script.
        script { attr.src "_content/Radzen.Blazor/Radzen.Blazor.min.js" }
    }
}
