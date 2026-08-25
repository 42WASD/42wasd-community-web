namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Tournaments page — feature-owned view. Selects the canonical Tournaments
/// data from Shared; owns no state of its own.
module Tournaments =

    let view (shared: SharedModel) =
        Layout.Tournaments()
            .Rows(dataRows shared.tournaments <| fun t ->
                tr { td { t.name }; td { t.prize }; td { t.startsAt.ToString("yyyy-MM-dd") } })
            .Elt()