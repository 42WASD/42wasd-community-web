# Production hardening

Prepare the app for production.

## Goal

```text
- deterministic verification pipeline is green (verify.sh)
- server deployable (static hosting + server as appropriate)
- remoting, error boundaries, and logging verified
- performance measured and acceptable
```

## Deliverables

The app ships as a **single container image** of the ASP.NET Core hosted Blazor
WebAssembly server. The one image serves the compiled client assets, the
Bolero remoting API, and the JSON `data/` files — nothing is cloned or built
at runtime.

```text
Dockerfile               # multi-stage: SDK publish -> aspnet runtime (non-root)
.dockerignore            # keeps thirdparty/, docs/, site/ out of the build context
deploy/k8s/              # Deployment + Service + Ingress manifests
.github/workflows/container.yml   # build + push image to GHCR on main / v* tags
```

## Build & push

```bash
# Local image build (verifies the Dockerfile before pushing)
docker build -t 42wasd-community-web .

# Push to GHCR (also done automatically by container.yml on main / tags)
docker push ghcr.io/42wasd/42wasd-community-web:latest
```

## Runtime contract

```text
image base   mcr.microsoft.com/dotnet/aspnet:10.0   (framework-dependent)
container     non-root user (PodSecurity "restricted")
port          8080  (ASPNETCORE_URLS=http://+:8080)
health        readiness/liveness probe on GET /
data          JSON files baked into image (read-only); mount a volume to
              make them writable at runtime
```

## WASM performance & trimming (measured)

The client is published **Release + AOT + trimming** in the container. Flags live
in `src/Community.Web.Client/Community.Web.Client.fsproj` (Release-only; the dev
server stays interpreted + untrimmed).

```xml
<RunAOTCompilation>true</RunAOTCompilation>
<PublishTrimmed>true</PublishTrimmed>
<TrimMode>partial</TrimMode>   <!-- NOT full — see below -->
<WasmEnableSIMD>true</WasmEnableSIMD>
<InvariantGlobalization>true</InvariantGlobalization>
<WasmEnableExceptionHandling>true</WasmEnableExceptionHandling>
<WasmDebugLevel>0</WasmDebugLevel>
```

- **Payload**: interpreted dev build ~12.9 MB / 227 requests → AOT+trimmed
  ~7.8 MB brotli / ~64 requests. `dotnet.native.wasm.br` (~5.5 MB) dominates.
- **`wasm-tools` workload is required** for AOT. The Dockerfile's SDK stage runs
  `dotnet workload install wasm-tools` before publish; locally use
  `sudo dotnet workload install wasm-tools`.
- **Do NOT use `<TrimMode>full</TrimMode>`.** It strips reflection metadata that
  Bolero remoting (`FSharpValue.MakeRecord` → the `CommunityApi` proxy) and
  Radzen `RadzenDataGrid` (`PropertyAccess` expression trees) need. `full` trim
  breaks login (app fails to init) and renders the Members grid empty in a
  deployed build while the dev build works — the tell-tale symptom pair.
  `partial` (the default) keeps the reflection targets while still shrinking
  the payload.

## Verification

```bash
bash scripts/docs/verify.sh   # docs pipeline, must report VERIFY OK
dotnet test                   # app's own tests pass
docker build .                # image builds cleanly
```