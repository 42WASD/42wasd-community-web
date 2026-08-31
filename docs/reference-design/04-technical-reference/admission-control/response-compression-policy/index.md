# Response compression policy

Selected browser policy:

```text
if body is sufficiently large and compressible:
    negotiate zstd
    fallback brotli
    fallback gzip

if body is tiny:
    no compression

if body already compressed:
    no redundant compression
```

## Initial benchmark points

```text
dynamic API zstd quality:
  3
  4
  6

precompressed static assets:
  benchmark substantially higher quality offline/build-time
```

ASP.NET Core 10 supports Zstandard response compression and quality levels
1–22.

Do not set quality 22 globally merely because CPU exists.

## Measure

```text
bytes saved
p50/p95 CPU time
p50/p95 response latency
```
