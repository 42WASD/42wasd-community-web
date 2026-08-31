# Contract model

Browser/server contract handshake, Shared DTO organization, strong identifiers,
DTO projection, and progressive page composition.

## Subsections

- **Browser/server contract handshake** — ClientHello/ServerHello versioning.
- **Shared F# DTO organization** — feature-owned contract folders.
- **Strong identifier convention** — domain-specific ID wrappers.
- **DTO projection rule** — never return a persistence entity directly.
- **Progressive page composition contract** — per-source `Remote<'T>` fields.
- **Streamed page-fragment operation** — when one stream is justified.
