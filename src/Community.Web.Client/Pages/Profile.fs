namespace Community.Web.Client.Pages

open Bolero
open Bolero.Html
open Elmish
open Community.Web.Client.Ui.Templates

/// Profile page — feature-owned, auth-gated. Holds the profile-edit draft in
/// its own Model (transient, carried by the route's PageModel). Access is
/// gated: a signed-out visitor sees a sign-in prompt instead of the form.
///
/// Session (who is signed in) lives on the root `SharedModel.account`; this
/// feature only owns the profile-edit draft. Sign-out is passed in as a
/// callback (a cross-feature session effect owned by the root/Shared).
module Profile =

    /// The Profile page's transient, page-local state (the edit form).
    type Model =
        {
            /// The displayed/editable handle. Seeded from the signed-in username.
            handle: string
            /// A short biography.
            bio: string
        }

    /// The Profile page's local messages. Pure draft edits, lifted into the
    /// root with Cmd.map.
    type Msg =
        | SetHandle of string
        | SetBio of string
        | Save

    /// A fresh, empty profile draft (the default PageModel value).
    let init =
        {
            handle = ""
            bio = ""
        }

    /// The Profile page's local update. Purely updates the draft; emits no
    /// commands (a save would be a server effect owned by the root/Shared).
    let update msg model =
        match msg with
        | SetHandle h -> { model with handle = h }, Cmd.none
        | SetBio b -> { model with bio = b }, Cmd.none
        | Save -> model, Cmd.none

    /// Render the auth gate: when signed out, show a sign-in prompt instead of
    /// the profile form. When signed in, render the profile editor seeded with
    /// the current handle.
    let view (form: Model) (username: option<string>) (localDispatch: Msg -> unit) (signOut: unit -> unit) =
        cond username <| function
        | None ->
            // Auth-gate: signed-out visitors see a sign-in prompt.
            Layout.ProfileSignedOut()
                .Elt()
        | Some name ->
            Layout.Profile()
                .Username(name)
                .Handle(form.handle, fun h -> localDispatch (SetHandle h))
                .Bio(form.bio, fun b -> localDispatch (SetBio b))
                .Save(fun _ -> localDispatch Save)
                .SignOut(fun _ -> signOut ())
                .Elt()