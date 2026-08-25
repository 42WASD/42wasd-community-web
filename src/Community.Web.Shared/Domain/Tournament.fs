namespace Community.Web.Shared.Domain

open System

/// A community-run tournament. `registrationOpen` reflects whether players may
/// still join.
type Tournament =
    {
        id: string
        name: string
        gameId: string
        startsAt: DateTime
        prize: string
        registrationOpen: bool
    }