namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui.Templates
open Community.Web.Shared.Domain

/// Members page — feature-owned view. Selects the shared Players list (the
/// canonical member roster); owns no state of its own.
module Members =

    let view (shared: SharedModel) =
        Layout.Members()
            .Rows(dataRows shared.players <| fun p ->
                tr { td { p.username }; td { defaultArg p.discord "" } })
            .Elt()