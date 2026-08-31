# PostgreSQL logical schema organization

Start with logical PostgreSQL schemas:

```text
identity
forum
game
tournament
notification
integration
platform
```

Example:

```text
identity.accounts
identity.personas
identity.external_login_credentials
identity.linked_external_profiles

forum.posts
forum.comments
forum.reactions
forum.change_log

platform.outbox
platform.inbox
```

## Database-per-service is a separate choice

Whether every microservice immediately owns a physically separate database is
a separate decision.

At first, one PostgreSQL cluster can host several logically isolated
schemas/databases.

Do not force:

```text
one Postgres server per tiny service
```

on a single-node homelab.
