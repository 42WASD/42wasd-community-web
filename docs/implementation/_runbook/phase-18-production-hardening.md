---
phase: 03-step-by-step-implementation/phase-18-production-hardening
---

**Phase 18 complete — production hardening.** The app is packaged as a single
container for Kubernetes. This turns the (previously static) "server deployable"
goal into a concrete, testable image that CI builds and the cluster runs.

### Reference

```text
- deterministic verification pipeline is green (verify.sh)
- server deployable (static hosting + server as appropriate)
- remoting, error boundaries, and logging verified
```

### What shipped

| Artifact | Purpose |
|---|---|
| `Dockerfile` | Multi-stage: `dotnet/sdk:10.0` publish → `dotnet/aspnet:10.0` runtime, non-root `USER app`, listens on `:8080`. |
| `.dockerignore` | Keeps `thirdparty/`, `site/`, `docs/`, `**/bin|obj` out of the build context. |
| `deploy/k8s/deployment.yaml` | `replicas: 1`, run-as-non-root, resource requests/limits, readiness + liveness `GET /`. |
| `deploy/k8s/service.yaml` | `ClusterIP` `:80 → 8080`. |
| `deploy/k8s/ingress.yaml` | TLS-terminating Ingress (nginx + cert-manager annotations). |
| `.github/workflows/container.yml` | Build + push to `ghcr.io/42wasd/42wasd-community-web` on `main` / `v*` tags. |

### Key decisions (verified against the project)

- **Single container for client + server.** The app is *hosted* Blazor WebAssembly:
  the `Server` project references the `Client` via `ProjectReference`, and
  `dotnet publish` emits the WASM assets next to the server DLL. No separate
  static host is needed — one Deployment serves everything.
- **Framework-dependent, `aspnet:10.0` runtime.** The repo targets `net10.0`
  (`global.json` SDK `10.0.111`). The runtime image already has the ASP.NET Core
  runtime, so the image stays small; self-contained was not needed.
- **Non-root + no privileges.** `USER app` and PodSecurity `runAsNonRoot: true`
  satisfy K8s "restricted" policy — no `allowPrivilegeEscalation`.
- **Restore isolation.** Only the `fsproj` files are copied before
  `dotnet restore` so NuGet layers cache; `radzen-blazor`/`Bolero` resolve from
  NuGet (the `thirdparty/` submodules are source references only, not in the
  image or build context).

### Verification

```bash
bash scripts/docs/verify.sh   # VERIFY OK
dotnet test                   # all pass
docker build .                # Release image builds cleanly
docker run -p 8080:8080 ghcr.io/42wasd/42wasd-community-web:latest
```

### Files changed

```
Dockerfile
.dockerignore
deploy/k8s/deployment.yaml
deploy/k8s/service.yaml
deploy/k8s/ingress.yaml
.github/workflows/container.yml
docs/reference-design/.../phase-18-production-hardening/index.md
docs/implementation/_runbook/phase-18-production-hardening.md   (this file)
docs/implementation/progress.yaml
docs/implementation/index.md
```

`verify.sh` reports `VERIFY OK`.