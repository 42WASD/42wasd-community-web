# RabbitMQ topology

Recommended logical exchanges/topics:

```text
community.events
community.jobs
community.deadletter
```

Suggested routing classes:

```text
forum.post.created
forum.post.edited
account.updated
notification.requested

job.external-profile-refresh
job.notification-send
job.tournament-build
job.media-metadata-refresh
```

## Separate

```text
domain/integration events
```

from:

```text
commands/jobs
```

because they have different semantics.
