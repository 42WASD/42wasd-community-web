namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Elmish
open Community.Web.Client.Ui.Templates

/// The Account page feature — owns the transient sign-in form draft.
/// (Phase 9: nested page messages.) This module holds its own Model, Msg,
/// init, and update, and its Msg is composed into the root Message and lifted
/// with Cmd.map.
///
/// Per the state-ownership model, the login/password fields are page-local and
/// discarded when leaving the page. Session/auth handling (sign-in/out,
/// getUsername) stays on the root Shared dispatcher, which owns
/// SharedModel.account and reads the form draft from here.
module Account =

    /// The Account page's transient, page-local state (the sign-in form).
    type Model =
        {
            username: string
            password: string
        }

    /// The Account page's local messages — form draft editing, plus a Submit
    /// intent that the root translates into a Shared session message. These
    /// are lifted into the root with Cmd.map. No message here reaches into
    /// Shared or another owner's state.
    type Msg =
        | SetUsername of string
        | SetPassword of string
        | Clear
        | Submit

    /// A fresh, empty sign-in form (the default PageModel value).
    let init =
        {
            username = ""
            password = ""
        }

    /// The Account page's local update. Purely updates the transient form
    /// draft; emits no commands (session effects live on the root, which
    /// interprets Submit by issuing a Shared.SendSignIn).
    let update msg model =
        match msg with
        | SetUsername s -> { model with username = s }, Cmd.none
        | SetPassword s -> { model with password = s }, Cmd.none
        | Clear -> init, Cmd.none
        | Submit -> model, Cmd.none

    /// The Account page's feature-owned view. It takes the slices of Shared it
    /// needs (the authenticated username + sign-in failure flag) *selected*,
    /// not duplicated, plus the live transient form draft (from the active
    /// PageModel). Local form messages are dispatched to the local dispatcher;
    /// the sign-out action is passed in as a callback because it is a
    /// cross-feature (session) effect owned by the root/Shared.
    let view (form: Model) (username: option<string>) (signInFailed: bool) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        cond username <| function
        | Some name ->
            Layout.AccountSignedIn()
                .Username(name)
                .SignOut(fun _ -> signOut ())
                .Elt()
        | None ->
            // The form draft comes from the live transient form (the active
            // page's PageModel value) supplied by the caller.
            Layout.SignIn()
                .Username(form.username, fun s -> localDispatch (SetUsername s))
                .Password(form.password, fun s -> localDispatch (SetPassword s))
                .SignIn(fun _ -> localDispatch Submit)
                .ErrorNotification(
                    cond signInFailed <| function
                    | false -> empty()
                    | true ->
                        Layout.ErrorNotification()
                            .HideClass("is-hidden")
                            .Text("Sign in failed. Use any username and the password \"password\".")
                            .Elt()
                )
                .Elt()