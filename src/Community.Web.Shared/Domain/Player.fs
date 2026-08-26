namespace Community.Web.Shared.Domain

/// A player in the community (a registered member).
type Player =
    {
        id: string
        username: string
        discord: string option
        /// The player's display handle, editable from the Account profile form.
        handle: string option
        /// A short bio shown on the profile, editable from the Account form.
        bio: string option
    }