namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Members page — feature-owned view. Selects the shared Players list (the
/// canonical member roster); owns no state of its own. Renders each member as
/// a responsive member card (Phase 17b, Radzen primitives).
module Members =

    let memberCard (player : Player) =
        RadzenUI.columnResponsive 12 6 4 (concat {
            RadzenUI.cardOutlined (RadzenUI.vStackGap "0.25rem" (concat {
                RadzenUI.text RadzenUI.heading6 player.username
                RadzenUI.text RadzenUI.caption (defaultArg player.discord "")
            }))
        })

    let view (shared: SharedModel) =
        cond shared.players <| function
        | NotAsked | Loading ->
            RadzenUI.vStack (concat { RadzenUI.skeleton (); RadzenUI.skeleton () })
        | Failed _ ->
            RadzenUI.text RadzenUI.body1 "Couldn't load members."
        | Loaded m ->
            RadzenUI.vStackGap "1.5rem" (concat {
                RadzenUI.text RadzenUI.display3 "Members"
                RadzenUI.rowGap "1rem" (forEach (Map.toArray m) (fun (_, p) -> memberCard p))
            })