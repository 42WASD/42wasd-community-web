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
/// component state. The page's search term lives in the page-local PageModel
/// (state-ownership table: page-owned search → page Model); its write-back
/// works in production thanks to the vendored Bolero SetModel fix
/// (thirdparty/Bolero Router.fs: Unsafe.AsRef no-opped under trimmed WASM).
/// Empty/loading/error states follow the RemoteData rules.
module Inbox =

    // ---------------------------------------------------------------- page

    /// The dedicated inbox page: local state — a search term, mirroring the
    /// Members page pattern.
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

    /// One inbox item: unread dot (only when the id is NOT in the shared
    /// read set) + title + body + relative date, rendered with Tailwind
    /// utilities on a Radzen surface row. `onRead` fires when the user
    /// clicks the item's mark-read control (None = no read tracking, e.g.
    /// a signed-out visitor — the dot then never renders).
    let private inboxItem (n: News) (isRead: bool) (onRead: unit -> unit) =
        RadzenUI.cardOutlinedClass "p-[var(--pad-card)] text-left w-full" (
            RadzenUI.vStackGap "0.375rem" (concat {
                RadzenUI.hStackGap "0.5rem" (concat {
                    // Unread marker: the cyan brand dot (same as online pips).
                    if isRead then empty () else
                        span {
                            attr.``class`` "inline-block w-2 h-2 rounded-full bg-[var(--rz-primary)] shrink-0"
                        }
                    RadzenUI.text RadzenUI.subtitle1 n.title
                })
                RadzenUI.text RadzenUI.body2 n.body
                RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                    RadzenUI.text RadzenUI.caption (n.publishedAt.ToString("MMM d, yyyy"))
                    if not isRead then
                        RadzenUI.textButton "Mark read" onRead
                })
            }))

    /// The item list shared by popup + page: latest-first, filtered by `q`.
    /// `readIds` drives the unread dots; `onRead newsId` marks one read.
    let private itemList (news: RemoteData<Map<string, News>>) (readIds: Set<string>)
                         (onRead: string -> unit) (reload: unit -> unit) (q: string) =
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
                        inboxItem n (readIds.Contains n.id) (fun () -> onRead n.id)
                })

    // ------------------------------------------------------------- popup

    /// The popup content: a compact header (title + unread count + mark-all)
    /// + items. Rendered INSIDE the header's inbox panel in Layout.fs — this
    /// function only builds the content node. `onMarkAll` fires the
    /// Shared.MarkAllNewsRead shared effect (a no-op visually when everything
    /// is already read).
    let popupContent (news: RemoteData<Map<string, News>>) (readIds: Set<string>)
                     (onRead: string -> unit) (onMarkAll: unit -> unit)
                     (reload: unit -> unit) (close: unit -> unit) =
        let unreadCount =
            match news with
            | Loaded ns ->
                ns.Keys |> Seq.filter (readIds.Contains >> not) |> Seq.length
            | _ -> 0
        RadzenUI.vStackGap "0.75rem" (concat {
            RadzenUI.hStackGapAlign "0.5rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                RadzenUI.hStackGap "0.5rem" (concat {
                    RadzenUI.text RadzenUI.subtitle1 "Notifications"
                    if unreadCount > 0 then
                        RadzenUI.badgePill RadzenUI.dangerBadge (string unreadCount)
                })
                RadzenUI.hStackGap "0.25rem" (concat {
                    if unreadCount > 0 then
                        RadzenUI.textButton "Mark all read" onMarkAll
                    RadzenUI.link "/inbox" (RadzenUI.iconButton "open_in_new"
                        (fun () -> close ()))
                })
            })
            itemList news readIds onRead reload ""
        })

    // -------------------------------------------------------------- page

    /// The dedicated /inbox page view. Takes the page's OWN Model (nullable
    /// under SSR — the router constructs the Page with a null Model; guard
    /// before reading fields) plus the shared read set + mark actions.
    let view (news: RemoteData<Map<string, News>>) (readIds: Set<string>)
             (onRead: string -> unit) (onMarkAll: unit -> unit)
             (model: Model) (onSearch: string -> unit) (reload: unit -> unit) =
        let search = if isNull (box model) then "" else model.search
        let unreadCount =
            match news with
            | Loaded ns -> ns.Keys |> Seq.filter (readIds.Contains >> not) |> Seq.length
            | _ -> 0
        RadzenUI.vStackGap "var(--gap-section)" (concat {
            RadzenUI.pageHeadingCrumb "Notifications"
                (Some "Everything that happened while you were away.")
                []
            RadzenUI.hStackGapAlign "0.75rem" RadzenUI.alignCenter RadzenUI.justifyBetween (concat {
                RadzenUI.searchBox search onSearch
                if unreadCount > 0 then
                    RadzenUI.textButton "Mark all read" onMarkAll
            })
            RadzenUI.cardGroup (
                itemList news readIds onRead reload search)
        })
