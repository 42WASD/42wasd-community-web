namespace Community.Web.Client.Pages

open Elmish

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