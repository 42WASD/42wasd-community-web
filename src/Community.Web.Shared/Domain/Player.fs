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
        /// Game ids this player has favourited (Games page toggle, reflected
        /// on Home's favourites stat). Part of the persisted player record so
        /// the shared effect survives refresh and moves to the DB as-is.
        favoriteGames: string list
        /// News ids this player has marked read (Inbox unread dot + badge).
        /// Part of the persisted player record for the same reason.
        readNews: string list
    }