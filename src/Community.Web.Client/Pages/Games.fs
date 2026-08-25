namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Games page — feature-owned view. Selects the canonical Games data from
/// Shared; owns no state of its own (static list page).
module Games =

    let view (shared: SharedModel) =
        Layout.Games()
            .Rows(dataRows shared.games <| fun g ->
                tr { td { g.name }; td { g.genre }; td { g.description } })
            .Elt()