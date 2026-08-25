namespace Community.Web.Shared.Contracts

open System

/// Canonical book entity — the single contract between client and server.
/// Lives in Community.Web.Shared so both sides compile against the same type.
type Book =
    {
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }