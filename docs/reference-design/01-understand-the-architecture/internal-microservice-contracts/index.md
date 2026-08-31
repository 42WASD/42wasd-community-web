# Internal microservice contracts

Backend services use `.proto`.

Example:

```protobuf
syntax = "proto3";

package community.forum.v1;

message GetPostRequest {
  bytes post_id = 1;
}

message GetPostResponse {
  Post post = 1;
}
```

## Rules

```text
never reuse removed field numbers
reserve removed numbers/names
add fields compatibly
version packages on breaking semantic changes
set deadlines
propagate cancellation
make retryable commands idempotent
```

## Wire representation is unavoidable

If Service A and Service B are separate processes:

```text
A memory
  ↓
bytes
  ↓
socket/network
  ↓
bytes
  ↓
B memory
```

Sharing the same F# source type does not allow one process to dereference
another process's object.

Use **gRPC + Protobuf** as the default internal synchronous contract because it
is compact, strongly schema-driven, supports streaming/deadlines/cancellation,
and allows future services in Go/Rust/Python/etc.

If profiling later finds an extraordinary hot path where materialization
overhead is dominant, benchmark FlatBuffers/Cap'n Proto or colocated
shared-memory designs. Do not make that the default architecture.

## Identifier encoding

If UUIDs are represented as 16 raw bytes internally, keep a consistent code
generator/helper layer. If developer simplicity matters more, strings are
acceptable initially; measure before optimizing identifiers at the expense of
usability.
