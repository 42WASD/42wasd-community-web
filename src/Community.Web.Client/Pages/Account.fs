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

    /// The Account page's transient, page-local state. `username`/`password`
    /// are intentionally NOT stored here — the login intent is a single
    /// message carrying both values (see `Login`), so there is no staged
    /// draft that could fall out of sync with a later submit.
    type Model =
        {
            handle: string
            bio: string
            notice: option<string>
        }

    /// The Account page's local messages.
    type Msg =
        | Login of string * string
        | SetHandle of string
        | SetBio of string
        | ShowNotice of string
        | SaveProfile
        | Clear

    let init =
        {
            handle = ""
            bio = ""
            notice = None
        }

    let update msg model =
        // Guard the null PageModel seen under Release SSR/trimming (same
        // pattern as Members.update): the router may hand us a null model, so
        // start from `init` before projecting onto it.
        let model' = if isNull (box model) then init else model
        match msg with
        | SetHandle h -> { model' with handle = h }, Cmd.none
        | SetBio b -> { model' with bio = b }, Cmd.none
        | ShowNotice msg -> { model' with notice = Some msg }, Cmd.none
        | Clear -> init, Cmd.none
        // Login and SaveProfile are *intent* messages, not local reducers:
        // they carry no page-state change. The root interprets them as
        // cross-feature effects (sign-in / profile save) in App.update, so
        // here they are an explicit no-op rather than duplicated branches.
        | Login _ | SaveProfile -> model, Cmd.none

    /// The sign-in form (signed out), built on RadzenLogin. The mock backend
    /// accepts any username with the password "password". Register and
    /// forgot-password have no real flow, so they surface an informational
    /// notice instead of failing silently.
    let signInForm (form: Model) (signInFailed: bool) (dispatch: Msg -> unit) =
        RadzenUI.vStackGap "1.5rem" (concat {
            RadzenUI.text RadzenUI.display3 "Sign in"
            RadzenUI.text RadzenUI.subtitle1 "Use any username and the password \"password\"."
            // A single message carries both fields — no sequential
            // dispatch, so one user action is exactly one MVU message.
            // Wrapped in a centred, width-constrained card so labels sit
            // close to their inputs.
            RadzenUI.loginCard
                (fun (username, password) -> dispatch (Login (username, password)))
                (fun () -> dispatch (ShowNotice "Sign-up isn't available in this demo build — use any username and the password \"password\"."))
                (fun _ -> dispatch (ShowNotice "Password reset isn't available in this demo build."))
            cond form.notice <| function
            | None -> empty ()
            | Some msg -> RadzenUI.alert RadzenUI.infoAlert msg
            cond signInFailed <| function
            | false -> empty()
            | true -> RadzenUI.alert RadzenUI.dangerAlert "Sign in failed."
        })

    /// The profile editor (signed in).
    let profileForm (form: Model) (name: string) (profileSaved: bool) (profileError: string option) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        RadzenUI.vStackGap "1.5rem" (concat {
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
            cond profileError <| function
            | None -> empty()
            | Some err -> RadzenUI.alert RadzenUI.dangerAlert err
        })

    let view (form: Model) (username: option<string>) (signInFailed: bool) (profileSaved: bool) (profileError: string option) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        // Guard the null PageModel seen under Release SSR/trimming (same
        // pattern as Members.view): the router may construct AccountPage with
        // a null Model, so fall back to a fresh `init` before reading fields
        // (e.g. `notice`) that would otherwise throw NullReferenceException
        // and 500 the SSR render.
        let form = if isNull (box form) then init else form
        cond username <| function
        | Some name -> profileForm form name profileSaved profileError localDispatch signOut
        | None -> signInForm form signInFailed localDispatch