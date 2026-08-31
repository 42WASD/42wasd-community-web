# Add response compression

Enable compression early in ASP.NET pipeline for eligible responses
(architecture doc Phase 29).

## Benchmark

```text
no compression
gzip
br
zstd quality 3
zstd quality 6
```

with representative:

```text
forum list
topic with comments
server status aggregate
large sync batch
```

Record:

```text
compressed bytes
CPU time
latency
allocations
```

Do not compress below measured threshold.

## Acceptance

```text
[ ] browser negotiates zstd where supported
[ ] fallback works
[ ] already-compressed media bypasses
```
