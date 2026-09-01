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

The client is published **Release + trimming, interpreted** in the container
today. The reference target REMAINS **AOT + trimming** — AOT is a design goal
of this phase, not an abandoned one — but it is currently DISABLED in the
container because two problems were never root-caused:

1. **Production breakage**: with AOT on, the serving image came up but several
   pages misbehaved at runtime (Members NRE et al). The immediate suspects
   (Radzen reflection paths) were fixed, but the failure kept recurring in
   other shapes and the true root cause was never positively identified.
2. **Iteration cost**: an AOT publish took ~4–6 min per build (wasm-ld is
   memory- and CPU-hungry), which made debugging the above nearly impossible.

**Follow-up (deliberate, not optional):** re-enable `RunAOTCompilation` in a
probe build, capture the managed stack traces + MVU trace from the browser
console against a minimal page set, and identify the exact stripping root
cause. Re-enable AOT in the container only once that cause is fixed and the
whole app passes the live page-by-page check. Until then the flags below are
the shipped configuration; keep the AOT line ready to flip.

Flags live in `src/Community.Web.Client/Community.Web.Client.fsproj`
(Release-only; the dev server stays interpreted + untrimmed).

```xml
<!-- AOT target: flip RunAOTCompilation back to true after the root cause
     above is fixed and verified page-by-page in a probe build. -->
<RunAOTCompilation>false</RunAOTCompilation>
<PublishTrimmed>true</PublishTrimmed>
<TrimMode>partial</TrimMode>   <!-- NOT full — see below -->
<WasmEnableSIMD>true</WasmEnableSIMD>
<InvariantGlobalization>true</InvariantGlobalization>
<WasmEnableExceptionHandling>true</WasmEnableExceptionHandling>
<WasmDebugLevel>0</WasmDebugLevel>
```

- **Payload (re-measured 2026-09-01, trimmed + interpreted, no AOT)**: 32 MB
  raw → **6.9 MB gzip / 5.8 MB brotli** of precompressed `_framework` assets.
  Top compressed contributors: `Radzen.Blazor` 1.4 MB, `FSharp.Core` 1.1 MB,
  `System.Private.CoreLib` 0.7 MB, `System.Private.Xml` 0.7 MB, native runtime
  `dotnet.native` 0.6 MB. Trimming — not AOT — is the dominant payload lever.
- **`wasm-tools` is NOT required** while AOT is off. The Dockerfile's
  `workload install wasm-tools` line is commented out; re-enable it together
  with `RunAOTCompilation` after the root cause is fixed.
- **Do NOT use `<TrimMode>full</TrimMode>`.** It strips reflection metadata
  that Bolero remoting (`FSharpValue.MakeRecord` → the `CommunityApi` proxy)
  and Radzen `RadzenDataGrid` (`PropertyAccess` expression trees) need. `full`
  trim breaks login (app fails to init) and renders the Members grid empty in
  a deployed build while the dev build works — the tell-tale symptom pair.
  `partial` (the default) keeps the reflection targets while still shrinking
  the payload.
- **Trimming caveat that DOES bite `partial`:** the router's
  `Router.definePageModel` writes page state via `PageModel.SetModel` →
  `Unsafe.AsRef`, which silently no-ops in the trimmed build (page state stays
  null). Page-local state (Members search, Inbox) therefore flows back through
  the root model immutably (`page = MembersPage { Model = m }`) instead of
  mutating the router's template record — see App.fs `MembersMsg`/`InboxMsg`.
  Server-side SSR also constructs pages with a null `PageModel.Model`, so every
  page view must null-guard its own model (Members/Inbox/Account all do).

## Verification

```bash
bash scripts/docs/verify.sh   # docs pipeline, must report VERIFY OK
dotnet test                   # app's own tests pass
docker build .                # image builds cleanly
```