namespace Community.Web.Shared.Contracts

open Bolero.Remoting

/// The remote API contract shared between client and server.
/// The client calls these functions; the server implements them.
///
/// Lives in Community.Web.Shared so both sides compile against the SAME
/// contract type (no client<->server circular dependency). This is the
/// only Bolero dependency in the shared layer — `IRemoteService`/`BasePath`
/// is part of the shared Bolero library, not a server- or client-only concern.
type BookService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"