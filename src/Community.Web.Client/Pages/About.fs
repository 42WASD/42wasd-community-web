namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.Ui

/// About page — feature-owned view. A static page; owns no state and needs no
/// message case (per message-organization: "Static pages do not need a message
/// case"). Built on Radzen text + card primitives.
module About =

    let view () =
        RadzenUI.vStackGap "1rem" (concat {
            RadzenUI.rise (RadzenUI.text RadzenUI.display3 "About")
            RadzenUI.card (RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.text RadzenUI.subtitle1
                    "The 42WASD gaming community."
                RadzenUI.text RadzenUI.body1
                    "Built with Bolero + Elmish on a Radzen Blazor design system."
            }))
        })