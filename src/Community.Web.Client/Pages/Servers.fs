namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Servers page — feature-owned view. Selects the canonical Servers data from
/// Shared; owns no state of its own. Renders each server as a responsive
/// gaming-community server-status card (Phase 17/17b: 42 Abu Dhabi theme +
/// Radzen). The Radzen 12-col grid wraps the cards responsively.
module Servers =

    let view (shared: SharedModel) =
        cond shared.servers <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load servers."
        | Loaded m ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Servers"
                RadzenUI.rowGap "1rem"
                    (forEach (Map.toArray m) (fun (_, s) ->
                        RadzenUI.columnResponsive 12 6 4 (RadzenUI.serverCard s)))
            })