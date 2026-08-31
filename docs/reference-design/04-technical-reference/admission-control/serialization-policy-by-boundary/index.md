# Serialization policy by boundary

Use:

```text
Browser Bolero RPC:
  Bolero/shared F# contract serialization

SignalR:
  MessagePack

Backend synchronous RPC:
  Protobuf/gRPC

Dapr Pub/Sub:
  CloudEvents envelope + typed payload contract

Database:
  native PostgreSQL types

Media:
  provider's own optimized media formats/transport
```

## The rule

Do not encode normal binary IDs as hexadecimal to "save space."

Hex increases size.
