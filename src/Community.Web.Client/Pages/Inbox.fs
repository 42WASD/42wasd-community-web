namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Elmish
open Community.Web.Client.State
open Community.Web.Client.Ui
open Community.Web.Shared.Domain

/// Inbox — the notification center behind the header bell (user request
/// 2026-08-30). One module, two surfaces:
///   1. `view`        — the RadzenPopup CONTENT (inbox items inside the
///                      top-right popup; the popup chrome lives in Layout).
///   2. `view` (page) — the dedicated /inbox PAGE (route Inbox), the same
///                      items rendered full-width with page heading.
/// Data = the shared News cache (canonical announcement source). Open state
/// for the popup lives in the ROOT Model (`inboxOpen`) — pure MVU, no
/// component state. Empty/loading/error states follow the RemoteData rules.
module Inbox =

    // ---------------------------------------------------------------- page

    /// The dedicated inbox page: local state (a read filter) so the page can
    /// show All or only unread-style "announcements" — kept minimal: a
    /// search term, mirroring the Members page pattern.
    type Model =
        {
            search: string
        }

    type Msg =
        | SetSearch of string

    let init = { search = "" }

    /// Pure update (null PageModel guard under trimming, same as Members).
    let update (msg: Msg) (model: Model) =
        match msg with
        | SetSearch s ->
            let model' = if isNull (box model) then init else model
            { model' with search = s }, Cmd.none

    // ------------------------------------------------------------ helpers

    /// One inbox item: unread dot + title + body + relative date, rendered
    /// with Tailwind utilities on a Radzen surface row.
    let private inboxItem (n: News) =
        RadzenUI.cardOutlinedClass "p-[var(--pad-card)] text-left w-full" (
            RadzenUI.vStackGap "0.375rem" (concat {
                RadzenUI.hStackGap "0.5rem" (concat {
                    // Unread marker: the cyan brand dot (same as online pips).
                    span {
                        attr.``class`` "inline-block w-2 h-2 rounded-full bg-[var(--rz-primary)] shrink-0"
                    }
                    RadzenUI.text RadzenUI.subtitle1 n.title
                })
                RadzenUI.text RadzenUI.body2 n.body
                RadzenUI.text RadzenUI.caption (n.publishedAt.ToString("MMM d, yyyy"))
            }))

    /// The item list shared by popup + page: latest-first, filtered by `q`.
    let private itemList (news: RemoteData<Map<string, News>>) (reload: unit -> unit) (q: string) =
        match news with
        | Loading ->
            RadzenUI.vStackGap "0.5rem" (concat {
                RadzenUI.skeletonCardBody ()
                RadzenUI.skeletonCardBody ()
            })
        | Failed _ ->
            RadzenUI.failedViewRetry "notifications" reload
        | NotAsked ->
            RadzenUI.text RadzenUI.body2 "No notifications yet."
        | Loaded ns ->
            let all = ns.Values |> Seq.sortByDescending (fun n -> n.publishedAt) |> Seq.toList
            let needle = q.Trim().ToLowerInvariant()
            let items =
                if needle = "" then all
                else
                    all |> List.filter (fun n ->
                        n.title.ToLowerInvariant().Contains needle
                        || n.body.ToLowerInvariant().Contains needle)
            if List.isEmpty items then
                RadzenUI.text RadzenUI.body2 "No notifications match your search."
            else
                RadzenUI.vStackGap "0.5rem" (concat {
                    for n in items do
                        inboxItem n
                })

    // ------------------------------------------------------------- popup

    /// The popup content: a compact header (title + count) + search + items.
    /// Rendered INSIDE RadzenUI.popup in Layout.fs — this function only
    /// builds the content node.
    let popupContent (news: RemoteData<Map<string, News>>) (reload: unit -> unit) (close: unit -> unit) =
        RadzenUI.vStackGap "0.75rem" (concat {
            RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                RadzenUI.text RadzenUI.subtitle1 "Notifications"
                RadzenUI.link "/inbox" (RadzenUI.iconButton "open_in_new"
                    (fun () -> close ()))
            })
            itemList news reload ""
        })

    // -------------------------------------------------------------- page

    /// The dedicated /inbox page view. Callbacks: reload (pull news again),
    /// and the page-local search dispatch.
    let view (news: RemoteData<Map<string, News>>) (search: string) (onSearch: string -> unit) (reload: unit -> unit) =
        RadzenUI.vStackGap "var(--gap-section)" (concat {
            RadzenUI.pageHeadingCrumb "Notifications"
                (Some "Everything that happened while you were away.")
                []
            RadzenUI.searchBox search onSearch
            RadzenUI.cardGroup (
                itemList news reload search)
        })
