# The selected tool stack

| Layer | Selected tool | Why selected |
|---|---|---|
| Runtime | **.NET 10** | Current Bolero 0.25 target; current ASP.NET Core features including Zstd/HybridCache ecosystem |
| F# web | **Bolero 0.25.x** | Keeps existing full-stack F#/Blazor/Elmish architecture |
| Client state | **Elmish** | Explicit MVU state transitions |
| UI library | **Radzen Blazor 11.2.x** | Existing stack; mature component set; pin tested patch |
| Browser structured storage | **IndexedDB** | Native async transactional persistent browser database |
| App asset cache | **Blazor PWA Service Worker** | Coherent hashed static application snapshot |
| Client query coordination | **Own `IQueryCoordinator`; prototype DotNetQuery 1.3.x underneath** | Dedup/SWR/subscriber semantics while avoiding hard lock-in to a young package |
| Browser RPC | **Bolero Remoting** | Shared typed F# browser/BFF contract |
| Browser realtime | **SignalR + MessagePack** | High-level groups/reconnect/streaming with compact binary hub protocol |
| Dynamic response compression | **Zstandard where negotiated; Brotli/gzip fallbacks** | ASP.NET Core 10 native providers; benchmark levels per payload |
| Browser server | **ASP.NET Core BFF** | Existing server foundation; auth, DI, middleware, SignalR, rate limiting |
| Internal synchronous RPC | **gRPC + Protobuf** | Compact, strongly schema-driven, polyglot |
| Distributed runtime | **Dapr 1.18.2** | Service invocation, pub/sub, workflows, secrets/config plumbing, observability |
| Durable broker | **RabbitMQ 4.2** | Mature queues; quorum queues for critical work; prefetch/backpressure/acks |
| Database | **PostgreSQL 18.x** | Durable relational source of truth; UUIDv7; strong type/index/transaction model |
| DB Kubernetes lifecycle | **CloudNativePG 1.30.x** | Operator lifecycle, monitoring, PgBouncer support, managed roles, upgrades/recovery |
| F# database-first access | **SQLProvider.PostgreSql + Npgsql** | PostgreSQL schema drives typed F# data access |
| DB schema migration | **Atlas versioned migrations** | Diff/apply workflow; PR-reviewed migrations. Official `atlas migrate lint` is Atlas Pro from v0.38; OSS CI must use an explicit alternative validation path |
| App caching | **HybridCache** | L1 + L2 unified API and in-instance stampede protection |
| Distributed cache | **Dragonfly 1.40.x** | Redis-compatible, multithreaded, shared-nothing design; strong multicore fit |
| Authentication | **ASP.NET Core Identity + EF Core/Npgsql store in the Accounts bounded context + external providers** | Reuse the mature Identity store instead of hand-implementing security-sensitive `IUserStore` interfaces; domain services can still use SQLProvider |
| BYOA video | **Google Identity Services code model + BFF token exchange + YouTube resumable upload** | Media bytes still travel browser→YouTube, while refresh-token/client-secret handling stays server-side |
| Network policy | **Cilium 1.20.x** | eBPF networking/security; standard + Cilium policy support |
| Network observability | **Hubble Relay/UI/CLI** | Cluster-wide flow and policy verdict visibility |
| GitOps | **Argo CD 3.5.x** | Existing deployment controller; current 2026 stable line |
| Local distributed development | **Aspire AppHost** | One local model/dashboard for services, queues, DB/cache; not production runtime |
| DB physical tiering | **LVM dm-cache, `smq`, writethrough initially** | Automatic hot-block promotion to small NVMe over large HDD |
| Telemetry | **OpenTelemetry + existing Prometheus/Grafana-compatible stack** | Cross-service trace/metric/log correlation |
| Optional P2P | **WebRTC** | Only for bandwidth-heavy direct voice/video/file transfer where justified |
