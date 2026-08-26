namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Elmish
open Community.Web.Client.Ui

/// The Account page feature — owns the transient sign-in form draft.
/// (Phase 9: nested page messages.) This module holds its own Model, Msg,
/// init, and update, and its Msg is composed into the root Message and lifted
/// with Cmd.map.
///
/// Per the state-ownership model, the login/password fields are page-local and
/// discarded when leaving the page. Session/auth handling (sign-in/out,
/// getUsername) stays on the root Shared dispatcher, which owns
/// SharedModel.account and reads the form draft from here.
///
/// Forms are built on Radzen inputs (TextBox/Password/TextArea/Button).
module Account =

    /// The Account page's transient, page-local state.
    type Model =
        {
            username: string
            password: string
            handle: string
            bio: string
        }

    /// The Account page's local messages.
    type Msg =
        | SetUsername of string
        | SetPassword of string
        | SetHandle of string
        | SetBio of string
        | SaveProfile
        | Clear
        | Submit

    let init =
        {
            username = ""
            password = ""
            handle = ""
            bio = ""
        }

    let update msg model =
        match msg with
        | SetUsername s -> { model with username = s }, Cmd.none
        | SetPassword s -> { model with password = s }, Cmd.none
        | SetHandle h -> { model with handle = h }, Cmd.none
        | SetBio b -> { model with bio = b }, Cmd.none
        | Clear -> init, Cmd.none
        // Submit and SaveProfile are *intent* messages, not local reducers:
        // they carry no page-state change. The root interprets them as
        // cross-feature effects (sign-in / profile save) in App.update, so
        // here they are an explicit no-op rather than duplicated branches.
        | SaveProfile | Submit -> model, Cmd.none

    /// The sign-in form (signed out), built on RadzenLogin.
    let signInForm (_form: Model) (signInFailed: bool) (dispatch: Msg -> unit) =
        RadzenUI.vStackGap "1rem" (concat {
            RadzenUI.text RadzenUI.display3 "Sign in"
            RadzenUI.text RadzenUI.subtitle1 "Use any username and the password \"password\"."
            RadzenUI.login (fun (username, password) ->
                dispatch (SetUsername username)
                dispatch (SetPassword password)
                dispatch Submit)
            cond signInFailed <| function
            | false -> empty()
            | true -> RadzenUI.alert RadzenUI.dangerAlert "Sign in failed."
        })

    /// The profile editor (signed in).
    let profileForm (form: Model) (name: string) (profileSaved: bool) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        RadzenUI.vStackGap "1rem" (concat {
            RadzenUI.text RadzenUI.display3 "Account"
            RadzenUI.text RadzenUI.subtitle1 ("Signed in as " + name)
            RadzenUI.text RadzenUI.caption "Display handle"
            RadzenUI.textBox form.handle (fun h -> localDispatch (SetHandle h))
            RadzenUI.text RadzenUI.caption "Bio"
            RadzenUI.textArea form.bio (fun b -> localDispatch (SetBio b))
            RadzenUI.hStackGap "0.5rem" (concat {
                RadzenUI.button "Save" RadzenUI.primaryButton (fun () -> SaveProfile) localDispatch
                RadzenUI.button "Sign out" RadzenUI.lightButton (fun () -> signOut ()) (fun _ -> ())
            })
            cond profileSaved <| function
            | false -> empty()
            | true -> RadzenUI.alert RadzenUI.successAlert "Profile saved."
        })

    let view (form: Model) (username: option<string>) (signInFailed: bool) (profileSaved: bool) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        cond username <| function
        | Some name -> profileForm form name profileSaved localDispatch signOut
        | None -> signInForm form signInFailed localDispatch