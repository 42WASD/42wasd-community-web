namespace Community.Web.Shared.Domain

/// A player in the community (a registered member).
type Player =
    {
        id: string
        username: string
        discord: string option
    }