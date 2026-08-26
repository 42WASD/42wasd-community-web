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
        // Tailwind utility framework — loaded via CDN. The browser runtime
        // scans the DOM at load (and after each Elmish render) and injects the
        // utility CSS used by our markup. This lets us replace custom
        // layout/animation rules (brand, card hover, mobile drawer, …) with
        // stock Tailwind classes.
        //
        // We import only `theme` + `utilities` (NOT `preflight`): preflight is
        // Tailwind's global reset, which would clobber Radzen's component
        // styling. theme provides the design tokens (--color-*, --spacing-*,
        // etc.) the utility classes reference.
        style {
            attr.``type`` "text/tailwindcss"
            // rawHtml so the `@import` quotes are NOT HTML-escaped (Bolero's
            // style builder escapes text, which breaks the CSS).
            // A custom `--animate-*` theme token generates an `animate-fade-in`
            // utility (same mechanism Tailwind uses for its own spin/ping/
            // pulse). Keyframes are declared inside `@theme` so they're emitted
            // alongside the token. Used in place of the old hand-rolled
            // `.fade-in` class in index.css.
            rawHtml
                ("@import \"tailwindcss/theme\";\n"
                 + "@import \"tailwindcss/utilities\";\n"
                 + "@theme {\n"
                 + "  --animate-fade-in: fade-in 0.4s cubic-bezier(0.22, 1, 0.36, 1);\n"
                 + "  @keyframes fade-in {\n"
                 + "    from { opacity: 0; transform: translateY(10px); }\n"
                 + "    to   { opacity: 1; transform: translateY(0); }\n"
                 + "  }\n"
                 + "}")
        }
        // The Tailwind browser runtime compiles the classes on the page.
        script { attr.src "https://cdn.jsdelivr.net/npm/@tailwindcss/browser@4" }
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
