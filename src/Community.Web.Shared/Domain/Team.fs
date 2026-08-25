namespace Community.Web.Shared.Domain

/// A team composed of players, competing in tournaments.
type Team =
    {
        id: string
        name: string
        players: Player[]
    }