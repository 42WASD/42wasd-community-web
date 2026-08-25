module Community.Web.Client.Ui.Layout

open Bolero
open Bolero.Html
open Community.Web.Client.App
open Community.Web.Shared.Contracts

/// The single shared layout template. Keeps the global `Ui/` folder small:
/// only cross-feature UI lives here (per the reference design). Page-specific
/// views will move beside their page in a later phase.
type Layout = Template<"wwwroot/main.html">

let homePage (model: Model) (dispatch: Message -> unit) =
    Layout.Home().Elt()

let counterPage (model: Model) (dispatch: Message -> unit) =
    Layout.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

let dataPage (model: Model) (username: string) (dispatch: Message -> unit) =
    Layout.Data()
        .Reload(fun _ -> dispatch GetBooks)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
        .Rows(cond model.books <| function
            | None ->
                Layout.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr {
                        td { book.title }
                        td { book.author }
                        td { book.publishDate.ToString("yyyy-MM-dd") }
                        td { book.isbn }
                    })
        .Elt()

let signInPage (model: Model) (dispatch: Message -> unit) =
    Layout.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty()
            | true ->
                Layout.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Layout.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view (model: Model) (dispatch: Message -> unit) =
    Layout()
        .Menu(concat {
            menuItem model Home "Home"
            menuItem model Counter "Counter"
            menuItem model Data "Download data"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty()
            | Some err ->
                Layout.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()