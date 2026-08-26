namespace Community.Web.Shared.Domain

/// A game title the community plays and runs servers for.
type Game =
    {
        id: string
        name: string
        genre: string
        description: string
        /// A banner/cover image URL used on the landing carousel & cards.
        imageUrl: string
    }