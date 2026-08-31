# Why not share F# types between backend services

Shared F# DTOs are retained for:

```text
Bolero client ↔ BFF
```

where both sides intentionally share one .NET contract.

Backend services use `.proto` because:

```text
services may later be written in Rust/Go/Python/etc.
deployment versions may diverge
wire compatibility matters more than source-language convenience
```

A network boundary still serializes.

No Kubernetes network protocol can dereference another Pod's managed F#
object directly.
