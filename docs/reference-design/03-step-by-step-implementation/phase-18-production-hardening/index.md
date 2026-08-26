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

## Verification

```bash
bash scripts/docs/verify.sh   # docs pipeline, must report VERIFY OK
dotnet test                   # app's own tests pass
docker build .                # image builds cleanly
```