# Multi-source page rendering

Do not make a giant all-or-nothing DTO where one slow provider delays
everything.

## Elmish page state

```fsharp
type Remote<'T> =
    | NotAsked
    | Loading
    | Ready of 'T
    | Failed of AppError

type TopicPageModel =
    {
        Core: Remote<PostDetails>
        Reactions: Remote<ReactionSummary>
        ServerStatus: Remote<ServerStatus>
        Media: Remote<MediaMetadata>
    }
```

## Composition

Start independent requests concurrently.

Render each section when it becomes `Ready`.

## Server-owned aggregate

If server-side ownership of the aggregate is required, use a SignalR stream of
typed fragments:

```fsharp
type PageUpdate =
    | CoreLoaded of PostDetails
    | ReactionsLoaded of ReactionSummary
    | ServerLoaded of ServerStatus
    | MediaLoaded of MediaMetadata
    | SourceFailed of source: Source * error: AppError
```

Only keep that stream alive while the route is active.

## The rule

Slow or optional sources never block already available useful page content.
