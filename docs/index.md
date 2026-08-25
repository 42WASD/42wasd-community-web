# 42WASD Community Web Docs

Welcome to the documentation for **42WASD Community Web**.

This site is built on a **Single-Source-of-Truth (SSOT)** reading-order
manifest. Read `scripts/docs/README.md` to understand the technique before
adding content.

## Reference Design

See the Reference Design section (auto-generated nav) for the ordered parts and
phases. Only **actionable** phases (marked `tracked: true` in the manifest)
appear on the Implementation progress page.

## Quick start — the Bolero spike

The community web is a Bolero (F#/Elmish) application. Set it up the way the
spike established — see **`BOLERA TEMPLATE spike SETUP.md`** for the full
guide. The short version:

```bash
# 1. Install the .NET 10 SDK (pin the version via global.json)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# 2. Build & pack the template from a checked-out Bolero-Template tag
cd thirdparty/Bolero-Template
./build.sh                      # runs the FAKE build (paket + nbgv auto-installed)
# -> build/Bolero.Templates.<version>.nupkg

# 3. Install the template and scaffold an app
dotnet new install <path>/Bolero.Templates.<version>.nupkg
dotnet new bolero-app -o MyApp

# 4. Run the server
cd MyApp && dotnet run --project src/MyApp.Server
# open http://localhost:5006
```

> The Elmish message trace runs in the **browser console**, not the server
> terminal. Add `withConsoleTrace` / `withErrorHandler` / `withTermination` to
> the `Program` pipeline to enable debugging.

## Docs quick start

```bash
# Install deps
cd projects && uv sync

# Validate + build (one command, same as CI)
bash scripts/docs/verify.sh
```

## Implementation progress

See the Implementation page for live build progress (generated from
`docs/implementation/progress.yaml`).