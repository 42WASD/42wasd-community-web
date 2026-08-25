module Community.Web.Client.Ui.Templates

open Bolero
open Bolero.Html
open Community.Web.Client.State

/// The single shared layout template. This file compiles before the feature
/// pages so they can reuse the template + shared rendering helpers without a
/// dependency cycle: pages depend on Templates, and the root view (which
/// composes pages) depends on both.
type Layout = Template<"wwwroot/main.html">

/// Render a RemoteData<Map<string,'T>> as a row list via a row renderer.
/// Shared helper used by the feature page views.
let dataRows (rd: RemoteData<Map<string, 'T>>) (render: 'T -> Node) =
    cond rd <| function
        | NotAsked | Loading -> Layout.EmptyData().Elt()
        | Loaded m -> forEach (Map.toArray m) (fun (_, t) -> render t)
        | Failed _ -> Layout.EmptyData().Elt()