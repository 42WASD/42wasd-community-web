# Dev Workflow — From Source to Browser

> Source: *The Elmish Book* (Zaid Ajaj) — part 5 (Improving Development
> Workflow): webpack-mode, compiler-directives, configuration-variables,
> hot-module-replacement, understanding-webpack-loaders, static-images,
> styling-with-sass, introducing-femto.

Learning templates hide the build; production needs it tuned. This page is
the mental model of the Fable toolchain: what runs, when, and why.

## The 1.4 MB counter

The book's own measurement: the counter example — a few buttons and an
`h1` — compiled to a **1,414 KB** bundle in development mode. Flipping
webpack to `production` (tree-shaking + minification) took the same app
to **161 KB** — nine times smaller for one config line. Nobody hand-wrote
those extra megabytes; the build's *mode* decided. That is why the toolchain
is worth understanding rather than ignoring.

## 1. webpack: the build's conductor

- webpack **calls Fable** (via `fable-loader`) to compile the `.fsproj`
  into JS, then bundles and serves it. `webpack.config.js` is the whole
  pipeline definition.
- **`mode`** switches personalities: `development` (fast, unminified,
  sourcemaps) vs `production` (minified, tree-shaken, optimized) — the
  1.4 MB → 161 KB story. The book's idiom: branch the config on
  `argv.mode`, and pass mode via npm scripts:

```json
// ✅ from The Elmish Book (webpack-mode.md)
"scripts": {
  "start": "webpack-dev-server --mode development",
  "build": "webpack --mode production"
}
```

- The dev server is a **static file server** for `dist/` — which is why
  you can already "talk HTTP" while developing (see
  [commands-and-async-data](../commands-and-async-data/index.md)).

## 2. Compiler directives: mode in F# code

- Propagate webpack's mode into the F# compiler via fable-loader options
  — `define: mode === "development" ? ["DEVELOPMENT"] : []` — then guard
  code with `#if DEVELOPMENT ... #endif`: log every state change while
  developing, compile that out in production.

## 3. Configuration variables

- Never hard-code per-machine values (API keys, feature flags). Twelve-
  factor rule: config lives **outside** the code.
- Front-end idiom: `.env` files (dev) / environment variables (CI) exposed
  as typed `Environment` lookups in F# (the book pairs this with the
  `dotenv-webpack` plugin).

## 4. Hot Module Replacement

- Full page refresh on every save **loses app state** (log in again after
  a font tweak!). **HMR** swaps only the changed module and *re-executes
  it separately* — "while maintaining the state" — so prototyping gets
  fast.
- Dev-only: the HMR runtime (websockets to the dev server) is compiled
  out of production bundles.

## 5. Loaders and static assets

- A webpack **rule** maps a file pattern to a tool: `.fs(x|proj)?` →
  fable-loader, images → asset loaders. Understanding this table is
  understanding the pipeline.
- Static images become **modules**: import them like code and webpack
  handles URLs/hashing in dev and prod.

## 6. Sass

- Inline styles (Feliz) are great from scratch; teams/designers bring CSS
  frameworks. **Sass** = CSS superset: variables, nesting, mixins, module
  imports — and crucially, **overriding third-party framework variables**
  before import.

## 7. Femto: nuget ↔ npm dependency bridge

- Fable bindings are thin F# shells over **npm packages** (Feliz needs
  `react@x`, `react-dom@x`), and versions must match. Manual installs and
  transitive dependency guesswork don't scale.
- **Femto** reads the version metadata inside the nuget binding and
  installs/resolves the exact npm dependencies — including transitive
  ones — with `dotnet tool install femto --global` once.

## Revise

The flowchart below is the **review map** of this page — each node a chunk
you just walked. Don't try to learn from it; use it to check coverage.

```mermaid
flowchart LR
    n1(["Dev workflow"])
    n2["webpack: the conductor"]
    n3["mode: development | production"]
    n4["fable-loader compiles .fsproj"]
    n5["dev server serves dist/"]
    n6["Compiler directives"]
    n7["DEVELOPMENT define"]
    n8["#if DEVELOPMENT blocks"]
    n9["e.g. trace state changes"]
    n10["Configuration variables"]
    n11["no hard-coded secrets"]
    n12[".env files → Environment"]
    n13["twelve-factor config"]
    n14["Hot Module Replacement"]
    n15["full refresh loses state"]
    n16["swap only changed modules"]
    n17["keep state while styling"]
    n18["Loaders &amp; assets"]
    n19["rules: which file → which tool"]
    n20["static images as modules"]
    n21["Sass"]
    n22["variables, nesting, mixins"]
    n23["override framework vars"]
    n24["Femto"]
    n25["nuget binding ↔ npm dep"]
    n26["version metadata resolves both"]
    n27["transitive npm deps handled"]
    n1 --> n2
    n2 --> n3
    n2 --> n4
    n2 --> n5
    n1 --> n6
    n6 --> n7
    n6 --> n8
    n6 --> n9
    n1 --> n10
    n10 --> n11
    n10 --> n12
    n10 --> n13
    n1 --> n14
    n14 --> n15
    n14 --> n16
    n14 --> n17
    n1 --> n18
    n18 --> n19
    n18 --> n20
    n1 --> n21
    n21 --> n22
    n21 --> n23
    n1 --> n24
    n24 --> n25
    n24 --> n26
    n24 --> n27
```

## Cross-links

- The app this pipeline builds: [fable-and-elm-architecture](../fable-and-elm-architecture/index.md).
- BCL-vs-JS boundaries behind the bindings: [fable-and-elm-architecture](../fable-and-elm-architecture/index.md).
- Config as a deployment concern: [architecture-decisions-and-soft-skills](../architecture-decisions-and-soft-skills/index.md).
- Measuring builds like measuring code (no guessing): [functions-and-composition](../functions-and-composition/index.md).
