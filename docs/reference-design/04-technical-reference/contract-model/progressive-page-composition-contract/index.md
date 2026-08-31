# Progressive page composition contract

For pages with multiple data sources, do not require one giant all-or-nothing
DTO.

## Example Elmish model

```fsharp
type Remote<'T> =
    | NotAsked
    | Loading
    | Ready of 'T
    | Failed of AppError

type TopicPageModel =
    {
        Core: Remote<TopicDto>
        Comments: Remote<CommentPageDto>
        Reactions: Remote<ReactionSummaryDto>
        GameContext: Remote<GameContextDto>
        Media: Map<MediaRef, Remote<MediaMetadataDto>>
    }
```

## On route activation

```text
core request
comments request
reaction request
game context request
```

can begin concurrently according to priority.

Render each independently.

Do not block 5 ms PostgreSQL data on a 700 ms external provider.
