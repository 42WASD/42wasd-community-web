# Maturity tiers

A common adoption path from a working shell to a polished product. Do not
optimize for features that do not exist yet.

## Tier A — foundation

Implement immediately:

```text
official Bolero project
one ProgramComponent
Page DU
root Model / Msg
root update
Home + About
shared layout
```

## Tier B — state architecture

Add next:

```text
Shared.Model
RemoteData
page-local Models
nested Page.Msg
Cmd.map
```

## Tier C — real community data

Add after the shell is stable:

```text
Events
Projects / Servers
Members
server remoting
loading/error states
normalized entity caches
```

## Tier D — product polish

Add later:

```text
authentication
account state
theme persistence
analytics
SEO/static rendering decisions
render optimization
```

## The rule

> Do not optimize the architecture for features that do not exist yet. Ship one
> vertical slice first, then grow by evidence.

## Platform tiers

At platform scale the same staged discipline extends beyond the browser client.
These tiers order the platform capabilities (detailed phase by phase in
Part III):

## Tier A — make persistence correct

Implement first:

```text
.NET 10 / Bolero 0.25 migration
PostgreSQL 18
CloudNativePG
Atlas migration history
SQLProvider/Npgsql
explicit browser DTOs
real authentication
```

Goal:

```text
no authoritative JSON-file state
no business truth in Pod RAM
```

## Tier B — make browser loading efficient

Add next:

```text
PWA service worker
IndexedDB stores
active-route cache loading
query coordinator
cursor pagination
entity versions/tombstones
```

Goal:

```text
refresh/load does not redownload the application unnecessarily
route revisits render cached data immediately
inactive scopes do not synchronize
```

## Tier C — make server work efficient

Add:

```text
HybridCache
Dragonfly
projection queries
Npgsql/PgBouncer connection budget
response compression
rate limiting
```

Goal:

```text
many identical users do not create many identical origin queries
```

## Tier D — realtime only where useful

Add:

```text
SignalR
MessagePack
active-scope groups
gap detection
cursor repair
```

Goal:

```text
visible content updates quickly
invisible content generates no domain traffic
```

## Tier E — distributed backend

Add:

```text
Dapr
Forum Service
Accounts Service
Game/Server Service
Tournament Service
RabbitMQ
transactional outbox
workers
```

Goal:

```text
BFF remains browser-specific
business capabilities can scale/deploy independently
```

## Tier F — advanced durable processes

Add only when actual use cases exist:

```text
Dapr Workflow
search service
moderation pipelines
analytics/event streams
WebRTC for bandwidth-heavy P2P
native mobile shell
```