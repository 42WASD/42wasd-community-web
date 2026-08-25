namespace Community.Web.Shared.Domain

open System

/// A community-run game server. Members can see active servers on the home page.
type GameServer =
    {
        id: string
        name: string
        gameId: string
        address: string
        onlinePlayers: int
        maxPlayers: int
        status: string // e.g. "online" | "maintenance" | "offline"
    }