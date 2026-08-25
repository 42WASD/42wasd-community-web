namespace Community.Web.Shared.Domain

/// A game title the community plays and runs servers for.
type Game =
    {
        id: string
        name: string
        genre: string
        description: string
    }