# Capability cheat sheet

| Concern | Recommended owner/tool |
|---|---|
| Root MVU program | Bolero `ProgramComponent<Model, Msg>` |
| Routing | Bolero `Router.infer` / `Router.inferWithModel` |
| Route representation | F# `Page` DU |
| Temporary route/page state | `PageModel<'T>` |
| Persistent cross-page state | `Shared.Model` |
| Local feature logic | feature/page `Model`, `Msg`, `update`, `view` |
| Async effects | Elmish `Cmd` |
| Child command lifting | `Cmd.map` |
| Server calls | Bolero Remoting or explicit HTTP client |
| Shared client/server contracts | `Community.Shared` |
| Rendering optimization | Bolero `ElmishComponent` where justified |
| Pure visual components | normal F# view functions/modules |
| Canonical entity cache | normalized maps keyed by IDs |
| Tests | pure `update` tests + routing + integration tests |

## How to read this

The cheat sheet is the **reuse-first** map: before writing new F# functions,
check whether Bolero/Elmish already provides the tool on this table.

## Platform capability matrix

At platform scale, every component is deliberate about which concerns it
touches. These boundaries are deliberate — no component is used outside its
intended responsibility merely for convenience.

| Component | Browser state | Durable DB | Distributed cache | Service RPC | Async work | Realtime | K8s network policy | Deployment |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Elmish | ✅ | ❌ | ❌ | ❌ | ❌ | consumes | ❌ | ❌ |
| IndexedDB | ✅ local | local only | local cache | ❌ | local outbox | stores local state | ❌ | ❌ |
| Service Worker | static assets | ❌ | static browser cache | intercepts fetch | limited browser lifecycle | ❌ | ❌ | ❌ |
| Bolero Remoting | client contract | ❌ | ❌ | browser↔BFF | ❌ | request/response | ❌ | ❌ |
| SignalR | transient | ❌ | ❌ | realtime hub | streaming | ✅ | ❌ | ❌ |
| ASP.NET Core BFF | request state | via services | HybridCache L1 | ✅ | submits jobs | ✅ | ❌ | workload |
| SQLProvider/Npgsql | ❌ | ✅ access | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| PostgreSQL | ❌ | ✅ | DB buffers only | SQL | durable metadata | ❌ | ❌ | CloudNativePG |
| HybridCache | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | library |
| Dragonfly | ❌ | ❌ primary | ✅ shared | RESP | pub/sub capability | possible backplane | ❌ | workload |
| Dapr | ❌ | adapter-dependent | adapter-dependent | ✅ | ✅ | pub/sub | ❌ | sidecar/control plane |
| RabbitMQ | ❌ | message durability | ❌ | not primary RPC | ✅ | event distribution | ❌ | workload/operator/chart |
| CloudNativePG | ❌ | manages PostgreSQL | ❌ | ❌ | DB jobs | ❌ | ❌ | ✅ DB lifecycle |
| Cilium | ❌ | ❌ | ❌ | network path only | network path | network path | ✅ | CNI |
| Hubble | ❌ | ❌ | ❌ | observes | observes | observes | observes verdicts | platform |
| Argo CD | ❌ | ❌ | ❌ | ❌ | reconciliation | ❌ | applies policy | ✅ |
| LVM dm-cache | ❌ | block layer | disk hot cache | ❌ | ❌ | ❌ | ❌ | host storage |