namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Servers page — feature-owned view. Selects the canonical Servers data from
/// Shared; owns no state of its own. Renders each server as a gaming-
/// community server-status card (Phase 17: 42 Abu Dhabi theme + Radzen cards).
module Servers =

    let view (shared: SharedModel) =
        cond shared.servers <| function
        | NotAsked | Loading -> Layout.Servers().Rows(Layout.EmptyData().Elt()).Elt()
        | Failed _ -> Layout.Servers().Rows(Layout.EmptyData().Elt()).Elt()
        | Loaded m ->
            Layout.Servers()
                .Rows(forEach (Map.toArray m) (fun (_, s) -> RadzenUI.serverCard s))
                .Elt()