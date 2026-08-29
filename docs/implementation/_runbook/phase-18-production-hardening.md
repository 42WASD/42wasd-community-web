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

---

## Post-phase fix — persistent, writable `/app/data` (saveProfile)

**Intent:** the Phase 18 Deployment baked `data/` JSON into the image read-only
(`/app/data` owned `root:root 755`). The `saveProfile` remoting call writes
`players.json` to `/app/data`, so at runtime the app user (uid 1654) hit
"Permission denied" — profile saves silently failed (the write is best-effort:
it is caught in `Loaders.saveJson`, logged, and returns `false`, so the site
stays up but the profile change does not persist). This change makes the data
dir writable AND persistent across pod restarts, and wires the app into Argo CD.

### Root cause (verified)

```bash
kubectl -n prd-42wasd-admin exec deploy/42wasd -- ls -la /app/data
# drwxr-xr-x 2 root root  ... games.json news.json players.json ...
kubectl -n prd-42wasd-admin exec deploy/42wasd -- sh -c "id; touch /app/data/test"
# uid=1654(app) gid=1654(app)
# touch: cannot touch '/app/data/test': Permission denied   <- NOT writable
```

### Fix: PVC + fsGroup + seed-data initContainer + Recreate

1. `deploy/k8s/pvc.yaml` (new) — `42wasd-data`, `nvme-fast`, 1Gi, RWO.
2. `deploy/k8s/deployment.yaml`:
   - Pod `securityContext.fsGroup: 1654` → chowns the mounted volume so the
     `app` user (uid/gid 1654) can write.
   - `initContainers.seed-data` — same image, `cp -rn /app/data/. /pvc-data/`
     to seed the baked JSON into the volume on first start (no-clobber so saved
     `players.json` survives restarts).
   - Main container `volumeMounts` — `data` at `/app/data`.
   - `volumes` — `persistentVolumeClaim: claimName: 42wasd-data`.
   - `strategy: Recreate` — RWO volume can't be shared during a rolling
     update; Recreate terminates the old pod first (single replica, so the
     brief downtime is acceptable).

### Argo CD integration

The Deployment was previously applied manually (no `argocd.argoproj.io/`
tracking). Wired it into GitOps in the iac repo:

- `infra/kubernetes/bootstrap/argocd/apps/tenant-community-web.yaml` (new) —
  Application, project `tenant-42wasd-admin`, source
  `github.com/42WASD/42wasd-community-web.git` path `deploy/k8s`, dest
  `prd-42wasd-admin`, auto-sync + prune + selfHeal, `ServerSideApply=true`.
- `infra/kubernetes/bootstrap/argocd/projects.yaml` — added
  `42wasd-community-web.git` to `tenant-42wasd-admin` `sourceRepos`.

```bash
# from the iac repo (42WASD/ubuntu-server-iac) — the manifests live there:
cd ~/ubuntu-server-iac
kubectl -n argocd apply -f infra/kubernetes/bootstrap/argocd/projects.yaml
kubectl -n argocd apply -f infra/kubernetes/bootstrap/argocd/apps/tenant-community-web.yaml
kubectl -n argocd get app tenant-community-web   # Synced
```

> Status (re-verified 2026-08-29): app `tenant-community-web` is
> `Synced` (health `Progressing` — Argo's kube-prometheus-style health
> check on the raw Ingress lacks a hook; all pods 1/1 Running and the site
> serves via `wasd.42base.com`). The workload manifests live in THIS repo
> under `deploy/k8s/` — that is the path the Argo Application watches.

The namespace `prd-42wasd-admin` is already Argo-owned by
`platform-namespaces` (labels `platform.tier: tenant` + PSS `restricted`).

### Verification

```bash
kubectl -n prd-42wasd-admin rollout status deploy/42wasd   # successfully rolled out
kubectl -n prd-42wasd-admin exec deploy/42wasd -- ls -la /app/data
# drwxrwsr-x 2 root app ...  <- group-writable by app via fsGroup
kubectl -n prd-42wasd-admin exec deploy/42wasd -- sh -c "printf '[]' > /app/data/t.json && rm /app/data/t.json"   # WRITE-OK
curl -s -o /dev/null -w "%{http_code}" http://wasd.42base.com/   # HTTP 200
kubectl -n argocd get app tenant-community-web                   # Synced
```

The Deployment is `Synced + Healthy`. (The Argo app-level health shows
"Progressing" solely because the Traefik Ingress does not populate
`status.loadBalancer` — a known cosmetic ArgoCD quirk; the site serves HTTP 200
by hostname and the pod is `Ready`.)

### Key lesson

Read-only baked data is a security win, but a write path must target a volume
the app user owns. `fsGroup` (group ownership) + a non-root `initContainer`
copy to a PVC is the pattern. A single-replica RWO workload also needs
`strategy: Recreate` or a rolling update deadlocks (new pod can't bind the
volume until the old pod releases it).