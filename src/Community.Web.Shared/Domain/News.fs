namespace Community.Web.Shared.Domain

open System

/// A news/announcement post for the community home page.
type News =
    {
        id: string
        title: string
        body: string
        publishedAt: DateTime
    }