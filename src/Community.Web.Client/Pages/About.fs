namespace Community.Web.Client.Pages

open Bolero
open Community.Web.Client.Ui.Templates

/// About page — feature-owned view. A static page; owns no state and needs no
/// message case (per message-organization: "Static pages do not need a message
/// case").
module About =

    let view () =
        Layout.About().Elt()