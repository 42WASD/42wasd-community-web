# Bolero Template — Full Setup & Debugging Guide

A complete guide for setting up, running, and debugging a Bolero application using the Elmish Model-View-Update (MVU) trace. Written for Linux/WSL2 Ubuntu 24.04, but the `.cmd`/Windows equivalents are noted where relevant.

---

## Prerequisites
- Linux (WSL2 Ubuntu 24.04 in this session)
- `sudo` access
- Internet access

---

## Phase 0 — Install .NET SDK

```bash
# Check if dotnet is already installed
dotnet --version
dotnet --list-sdks

# Install .NET 10 SDK (Ubuntu 24.04)
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify
dotnet --version          # → 10.x
dotnet --list-sdks
```

> ⚠️ The template targets `net10.0` (the README says 8.0, but trust `global.json`/`.fsproj`).

---

## Phase 1 — Clone & checkout

```bash
# Clone both repos
git clone https://github.com/fsbolero/Bolero
git clone https://github.com/fsbolero/Template

# Checkout the latest tags
git -C Bolero checkout v0.25.63
git -C Bolero-Template checkout v0.25.17

# Verify
git -C Bolero describe --tags          # → v0.25.63
git -C Bolero-Template describe --tags # → v0.25.17
```

---

## Phase 2 — Build & pack the template

```bash
cd /home/jyao/BOLERO-SPIKE/Bolero-Template

# Run the FAKE build (auto-installs paket 10.3.1 + nbgv 3.6.133 via dotnet tool restore)
./build.sh
# Windows: build.cmd
```

**What it does:**
- `dotnet tool restore` → installs Paket + Nerdbank.GitVersioning locally
- `dotnet run --project .build` → runs the FAKE build
- Produces: `build/Bolero.Templates.<version>.nupkg`

Result: `build/Bolero.Templates.0.25.17-g3ac9a11.nupkg`

---

## Phase 3 — Install template & scaffold an app

```bash
# Install the template locally
dotnet new install /home/jyao/BOLERO-SPIKE/Bolero-Template/build/Bolero.Templates.0.25.17-g3ac9a11.nupkg

# Create an app
cd /home/jyao/BOLERO-SPIKE
dotnet new bolero-app -o MyBoleroApp

# Optional variants
dotnet new bolero-app -o MyApp --minimal
dotnet new bolero-app -o MyApp --render InteractiveServer
dotnet new bolero-app -o MyApp --render WebAssembly
```

Generated structure:
```
MyBoleroApp/
├── MyBoleroApp.sln
└── src/
    ├── MyBoleroApp.Client/     # Elmish UI (WASM client)
    └── MyBoleroApp.Server/     # ASP.NET host + remoting
```

---

## Phase 4 — Run the app

```bash
cd /home/jyao/BOLERO-SPIKE/MyBoleroApp

# Run the server (foreground)
dotnet run --project src/MyBoleroApp.Server
```

**Output tells you the URL:**
```
Now listening on: https://localhost:44323
Now listening on: http://localhost:5006     ← use this
```

Access: `http://localhost:5006` (Home / Counter / Download data pages).

> If hot-reload is enabled (`#if DEBUG`), edits to `Main.fs` reload live. For program-pipeline changes, rebuild + restart the server.

---

## Phase 5 — Enable Elmish debugging (trace)

Edit `src/MyBoleroApp.Client/Main.fs`, find the `Program` pipeline, add the hooks:

```fsharp
override this.Program =
    let bookService = this.Remote<BookService>()
    let update = update bookService
    Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
    |> Program.withRouter router
    |> Program.withErrorHandler (fun (msg, exn) ->
        printfn "[ELMISH-ERROR] msg=%A exn=%A" msg exn)
    |> Program.withTrace (fun model msg subs ->
        printfn "[ELMISH-TRACE] msg=%A model=%A subs=%A" msg model subs)
    |> Program.withConsoleTrace
    |> Program.withTermination (fun _ -> false) (fun _ -> ())
#if DEBUG
    |> Program.withHotReload
#endif
```

**Hook cheat-sheet:**

| Hook | What it logs |
|---|---|
| `withConsoleTrace` | `New message:: X` + `Updated state:: {full model}` (to browser console) |
| `withTrace` | custom BEFORE-state: message + model + subscriptions |
| `withErrorHandler` | unhandled exceptions escaping the dispatch loop |
| `withTermination` | exit criteria (here: never) |

**Rebuild after editing:**
```bash
cd /home/jyao/BOLERO-SPIKE/MyBoleroApp
dotnet build src/MyBoleroApp.Client/MyBoleroApp.Client.fsproj -c Debug
```

**Restart cleanly (kill old, start new):**
```bash
pkill -f "MyBoleroApp.Server"
dotnet run --project src/MyBoleroApp.Server
```

---

## Phase 6 — Capture the trace via Playwright

The browser can be driven to capture the message trace. Equivalent Playwright snippets:

**1. Reload a page and capture the message trace:**
```js
const logs = [];
page.on('console', msg => logs.push(`[${msg.type()}] ${msg.text()}`));
page.on('pageerror', err => logs.push(`[pageerror] ${err.message}`));
await page.goto('http://localhost:5006/counter', { waitUntil: 'networkidle' });
await page.waitForTimeout(2500);
return logs;
```

**2. Click buttons and capture resulting state:**
```js
const plus  = page.locator('button', { hasText: '+' }).first();
const minus = page.locator('button', { hasText: '-' }).first();
await plus.click();  await page.waitForTimeout(400);
await plus.click();  await page.waitForTimeout(400);
await minus.click(); await page.waitForTimeout(400);
// read counter value
const m = (await page.locator('body').innerText()).match(/value is: ([-\d]+)/);
return m && m[1];
```

**3. Sign in (fill form + submit):**
```js
await page.locator('input').nth(0).fill('alice');
await page.locator('input').nth(1).fill('password');
await page.getByRole('button', { name: 'Sign in' }).click();
await page.waitForTimeout(2000);
```

**4. Click a menu link:**
```js
await page.locator('nav a', { hasText: 'Counter' }).first().click();
```

---

## Phase 7 — Server-side checks (remoting)

The server logs HTTP access + remoting calls to its terminal. For endpoints that don't need antiforgery, you can test directly:

```bash
# Test the signIn contract directly (reveals server-side logic bugs)
curl -s -X POST http://localhost:5006/books/signIn \
  -H 'Content-Type: application/json' \
  -d '["alice","password"]' -w '\nHTTP:%{http_code}\n'
```

---

## Debugging workflow summary

1. **Run** → `dotnet run --project src/MyBoleroApp.Server`
2. **Open** → `http://localhost:5006`
3. **Capture** → hook browser console, reload
4. **Read the trace** → look for `New message:: X` → `Updated state:: { ... }`
5. **Cross-reference** → open the matching `| X ->` case in `Main.fs` and check the logic
6. **Reproduce** → click the feature, watch the message, confirm the state is wrong
7. **Server-side bugs** → use `curl` against the remoting endpoints

---

## Useful gotchas (learned in this session)

- **Routing is the first thing to test** — if `SetPage` is broken, every other page is unreachable and its bugs are invisible.
- **The trace runs client-side (WASM)** — it prints to the **browser console**, not the server terminal. (Server shows only HTTP/remoting logs.)
- **`withTrace` param order** is `model msg subs` (model first).
- **Rebuild + restart** after changing the `Program` pipeline — hot-reload is unreliable for that.
- **Server-side bugs (e.g. `signIn` inverted)** are invisible to *any* client trace — you must test the endpoint's contract directly.