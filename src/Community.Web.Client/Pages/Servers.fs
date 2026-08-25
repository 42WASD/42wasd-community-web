namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Servers page — feature-owned view. Selects the canonical Servers data from
/// Shared; owns no state of its own.
module Servers =

    let view (shared: SharedModel) =
        Layout.Servers()
            .Rows(dataRows shared.servers <| fun s ->
                tr { td { s.name }; td { s.address }; td { s.onlinePlayers.ToString() }; td { s.status } })
            .Elt()