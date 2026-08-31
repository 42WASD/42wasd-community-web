# Rejected foundations

Several attractive tools were evaluated and deliberately **not** selected as
the foundation. Record why, so the decision is not silently re-litigated.

## Entity Framework Core

EF Core is the mainstream .NET ORM. It is **not** the default data-access layer
for ordinary 42WASD domain services because the selected domain-data preference
is:

```text
PostgreSQL schema is authoritative
        ↓
typed F# access should be derived from it
        ↓
SQLProvider for domain persistence
```

There is, however, one deliberate exception in this architecture:

```text
Accounts / ASP.NET Core Identity
        ↓
EF Core + Npgsql Identity store
        ↓
PostgreSQL identity schema
```

Why make this exception? ASP.NET Core Identity's mature built-in persistence
path is based on Identity stores implemented over EF Core, and PostgreSQL is
officially supported through `Npgsql.EntityFrameworkCore.PostgreSQL`. Reusing
that store is safer and substantially less implementation work than writing a
custom `IUserStore`, `IUserLoginStore`, `IUserTokenStore`, lockout, claims,
role, security-stamp, and external-login persistence stack yourself.

This does **not** require EF Core to become the persistence abstraction for
Forum/Game/Tournament services. Keep the persistence technology private to each
bounded context.

Use a custom Identity storage provider only if you later have a concrete reason
to replace the mature EF/Npgsql store and are prepared to implement and
security-test the required Identity store interfaces.

Rule:

```text
Forum bounded context       -> SQLProvider/Npgsql
Game bounded context        -> SQLProvider/Npgsql
Tournament bounded context  -> SQLProvider/Npgsql
Accounts Identity store     -> EF Core/Npgsql
```

Do not use two persistence frameworks against the same tables without a
specific, documented ownership reason.

## Giraffe

Giraffe is an F# functional HTTP layer on ASP.NET Core.

Use it where a service genuinely needs clean F# HTTP routes, for example:

```text
webhook endpoints
public REST API
internal HTTP integration surface
```

Do not add it to a gRPC-only backend service or to the Bolero BFF merely
because it exists.

## Direct browser gRPC

Browsers have different constraints from native gRPC clients.

You already have Bolero Remoting for browser RPC and SignalR for realtime.

Keep:

```text
browser <-> BFF: Bolero/SignalR
service <-> service: gRPC/Protobuf
```

unless a future API requirement justifies gRPC-Web/transcoding.

## Global always-on browser synchronization

Rejected.

It wastes bandwidth/compute and conflicts with the active-scope requirement.

## WebRTC for forum synchronization

Rejected.

Forum state needs an authoritative server, durable ordering, moderation,
offline recovery, and scalable fanout.

WebRTC is optional for bandwidth-heavy peer traffic, not authoritative forum
state.

## First-party media object storage as the primary attachment path

Rejected as a product convention.

BYOA provider-backed media is selected.

Operator-owned object storage may still be appropriate for **database backups**
or internal system artifacts. That is a separate durability requirement from
user media ownership.

## One giant monolith

Not selected because independent domain deployment/ownership is a stated goal.

However, avoid replacing it with a distributed monolith. Services must be
coarse bounded contexts.

## One microservice per operation

Rejected.

Network calls are not a substitute for ordinary functions.

## Redis/Valkey as the primary cache choice

Excellent mature alternatives.

Dragonfly is selected because the target machine has many cores and the user
explicitly wants a multicore Redis-compatible cache.

Keep the application on `HybridCache`/`IDistributedCache` abstractions so this
choice remains reversible.

## Garnet

Strong .NET-adjacent alternative with interesting tiered-storage behavior.

Not selected initially because Dragonfly offers a more direct Redis-compatible
operational path and has current pub/sub/tiered-storage work. Benchmark Garnet
later if cache workload justifies it.

## DotNetQuery as an architectural dependency

Not yet.

Its semantics are useful and it is a good prototype candidate, but it is young.
Put an internal F# interface in front of it.
