# Client local database layout

Recommended logical stores in the browser:

```text
cache_forum_topics
cache_forum_posts
cache_users
cache_server_status
cache_tournaments

drafts

outbox

upload_sessions

sync_metadata
```

## Two classes of local data

Treat the `cache_*` group as **disposable**.

Treat:

```text
drafts
outbox
upload_sessions
```

as **user-owned local work** that requires explicit schema migration and must
never be silently discarded on a cache reset.

## The rule

The local database is a cache plus a small set of user-owned durable artifacts.
It is never a replica that must remain globally fresh — freshness is the job of
the route-scoped synchronization contract.
