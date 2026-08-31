# Verified framework behavior

These are the current, verified facts about Bolero, Elmish, and supporting
frameworks as of 2026-08-25. They directly support the architecture in this
guide.

## Bolero — main repository

```text
Bolero integrates Elmish
routing binds URL to a field in the Elmish model
remoting exposes asynchronous server functions
current main-repository getting-started requirement: .NET SDK 10.0
```

> **Documentation mismatch:** the Bolero main README says .NET 10, but some
> older/template/docs surfaces still mention .NET 8.
>
> **Decision:** follow the current main Bolero repository and tested template
> output; pin the SDK in `global.json`; do not copy an old SDK number from an
> outdated page blindly.

## Bolero routing

```text
endpoint type commonly represented by F# union
endpoint stored in Elmish model
PageChanged message updates it
Router.infer binds route <-> model
PageModel<'T> is excluded from URL
Router.inferWithModel supplies defaults for PageModel
```

## Bolero `ProgramComponent`

```text
ProgramComponent<'model, 'msg>
```

is the main Bolero component whose content is defined by an Elmish program.

Selected rule:

> One primary `ProgramComponent` for the application. Do not create independent
> program stores for ordinary page fragments.

## Bolero `ElmishComponent`

```text
ElmishComponent<'model, 'msg>
```

receives a sub-model and only re-renders when that model changes.

Interpretation:

> rendering boundary != state ownership boundary. Use it for rendering
> isolation/optimization when needed, not as justification for local MVU state
> on every visual component.

## Elmish parent-child composition

```text
child Model
child Msg
child update
parent wraps child Msg
parent uses Cmd.map
```

This is the mechanism used when a feature genuinely deserves its own MVU
boundary. It is not a requirement to make every component a child application.

## Elm application structure guidance

Elm's official guide recommends page-centered modules containing `Model`,
`init`, `update`, `view`, helpers — and warns against globally splitting code
into `Model/`, `View/`, `Update/`. It also warns against translating the visual
component tree directly into architectural components.

## Framework-agnostic feature organization

Angular guidance recommends organizing by feature areas and avoiding top-level
directories based purely on code type. Redux guidance likewise recommends
feature folders and organizing state around data/functionality rather than UI
components. These are not Bolero rules but independently support the same
feature-oriented repository principle.

## Platform verification notes — 2026-08-27

The platform layers were verified against current primary documentation on the
same date. Record the verified facts and decisions here so upgrades start from
evidence:

### Bolero

```text
Bolero 0.25 published August 2026; adds .NET 10 support
project-template command: Bolero.Templates::0.25.17
0.25 also optimized HTML-template code generation and supports
  task/async callbacks in generated templates
```

Decision: upgrade the existing app to tested Bolero 0.25.x, target .NET 10,
pin tested package versions.

### Radzen

```text
Radzen.Blazor actively moving through 11.2.x in August 2026
```

Decision: upgrade to a tested 11.2.x patch; do not auto-follow the newest
patch in production; record the tested patch in the platform release
manifest.

### ASP.NET Core response compression

```text
.NET 10 includes built-in Zstandard / Brotli / Gzip, negotiated via
  Accept-Encoding; Zstandard quality configurable 1–22
```

Decision: benchmark dynamic responses around zstd quality 3–6; use higher
build-time compression for immutable assets; skip tiny/already-compressed
bodies. Compression over HTTPS has security implications where
attacker-controlled and secret material share compressed contexts — treat as a
security review item.

### Blazor PWA

```text
.NET 10 PWA guidance uses service-worker.js / service-worker.published.js /
  service-worker-assets.js
published asset manifest contains hashes for Blamanaged/static resources
cached application snapshot is treated coherently to avoid mixing
  incompatible app files
```

Decision: use the framework PWA asset snapshot; do not invent custom runtime
caching in IndexedDB.

### SignalR

```text
.NET 10 MessagePack hub protocol package line is 10.0.x
self-hosted scale-out docs recommend a Redis backplane
```

Decision: MessagePack for realtime payloads; evaluate Dragonfly through
StackExchange.Redis compatibility; production-enable only after
group/fanout/reconnect/backplane tests.

### Aspire

```text
code-first orchestration and observability layer
multi-language, but AppHost authoring is currently C# or TypeScript
not the production runtime
```

Decision: small C# Community.AppHost for local developer orchestration; F#
services remain F#; production remains Kubernetes + Argo.

### Dapr

```text
Runtime 1.18.2, release date 2026-07-21
building blocks: workflow, service invocation, pub/sub, state, bindings,
  actors, secrets, configuration, distributed lock, cryptography, jobs
```

Decision: pin 1.18.2 or a newer tested 1.18.x patch; do not float latest.

### RabbitMQ

```text
documentation line 4.2
quorum queues are the modern replicated/durable queue type (Raft)
RabbitMQ recommends publisher confirms, manual consumer acks, bounded
  prefetch for reliable messaging
```

Decision: normal work queues first; quorum only where a replicated broker
cluster actually exists; streams for large replay/event-log use cases.

### Dragonfly

```text
1.40.0, release date 2026-08-04
Redis/Memcached API compatibility, multithreaded shared-nothing design
1.40 includes fixes in tiered storage, replication, pub/sub
```

Decision: HybridCache secondary store candidate = Dragonfly; pin a tested
1.40.x patch.

### PostgreSQL

```text
18: native uuid, uuidv7(), asynchronous I/O improvements, strong
  relational/JSONB/index/transaction feature set
```

Decision: PostgreSQL 18.x, native types, UUIDv7 for distributed domain IDs.

### CloudNativePG

```text
1.30.0, released 2026-06-29
supports PostgreSQL 18 and Kubernetes 1.34–1.36 in the 1.30 release notes
notable: DatabaseRole CRD, lease-based primary election, security hardening,
  PostgreSQL 18.4 default image
```

Decision: CloudNativePG 1.30.x; single instance initially on a single
physical node.

### SQLProvider

```text
PostgreSQL provider uses Npgsql; exposes schema-derived typed F# access
```

Decision: SQLProvider.PostgreSql with the PostgreSQL schema as persistence
source of truth.

### Atlas

```text
atlas migrate diff   -> usable migration-generation workflow
atlas migrate apply  -> migration application workflow
atlas migrate lint   -> Atlas Pro feature starting with Atlas v0.38
```

Decision: reviewed versioned SQL migrations in Git; apply to an ephemeral
PostgreSQL 18 CI database; run schema/constraint/integration tests; optional
SQL static/safety linter; `atlas migrate apply` in a controlled deployment
job. Do not make a production CI gate depend on `atlas migrate lint` unless
Atlas Pro is intentionally adopted.

### Cilium/Hubble

```text
Cilium 1.20.1 stable documentation audited
supports standard NetworkPolicy, CiliumNetworkPolicy,
  CiliumClusterwideNetworkPolicy, Kubernetes ClusterNetworkPolicy in 1.20
Hubble provides node/cluster flow visibility through Relay
```

Decision: default deny, Hubble Relay, policy verdict workflow. Hubble has
performance overhead depending on traffic/aggregation — tune observation.

### Argo CD

```text
3.5.0 GA: 2026-08-04; latest verified patch 3.5.1, released 2026-08-12
ApplicationSet generators: list, clusters, Git directories/files, SCM
  providers, others
```

Decision: existing Argo setup remains the deployment controller; central
infra repo owns ApplicationSets/AppProjects; application repos own their app
manifests per the selected ownership convention.

### LVM dm-cache

```text
lvmcache(7): dm-cache is a read/write hot-spot cache where portions of a
  slow LV are transparently placed on a smaller faster LV
default policy: smq; modes: writethrough, writeback
```

Decision: `smq`, writethrough initial production mode.