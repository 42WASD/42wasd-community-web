# Capacity planning

Track these separately:

```text
CPU:
  BFF
  services
  DB
  Dragonfly
  RabbitMQ

RAM:
  .NET heaps
  Postgres buffers/page cache
  Dragonfly
  OS cache

Disk:
  Postgres
  WAL
  RabbitMQ
  logs
  local caches

Network:
  browser API traffic
  SignalR
  internal gRPC
  provider APIs
```

## The rule

Do not treat:

```text
64 CPU cores
```

as permission to oversubscribe memory/disk.

The HDD remains a shared physical bottleneck.
