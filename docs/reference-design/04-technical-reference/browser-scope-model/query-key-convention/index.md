# Query key convention

The RequestCoordinator needs stable semantic keys.

## Examples

```text
forum-category:{categoryId}:sort:{sort}:after:{cursor}
topic:{topicId}:core
topic:{topicId}:comments:after:{cursor}
profile:{userId}:public
servers:list:{filterHash}
tournament:{tournamentId}:details
```

## Never use

```text
component instance ID
random GUID per render
raw URL if equivalent semantic queries can have different URLs
```

as the cache identity.

The key represents:

```text
"What logical data does this query mean?"
```
