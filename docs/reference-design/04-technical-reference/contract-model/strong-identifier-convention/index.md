# Strong identifier convention

Use domain-specific wrappers in F#:

```fsharp
type AccountId = AccountId of Guid
type PersonaId = PersonaId of Guid
type PostId = PostId of Guid
type CommentId = CommentId of Guid
type TournamentId = TournamentId of Guid
```

This prevents:

```text
deletePost(accountId)
```

from compiling merely because every ID is a bare `Guid`.

## For PostgreSQL

```text
uuid
```

is the storage type.

For newly generated distributed IDs:

```text
UUIDv7
```

is the selected default.

PostgreSQL 18 includes native `uuidv7()` generation.
