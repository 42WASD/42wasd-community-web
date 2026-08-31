# Response compression

## Optimization order

```text
1. do not request unnecessary data
2. do not request unchanged data
3. project only needed fields
4. use compact serialization where appropriate
5. batch where it reduces overhead without adding bad latency
6. compress sufficiently large remaining representation
```

## Providers

ASP.NET Core 10 supports:

```text
zstd
br
gzip
```

negotiated via `Accept-Encoding`.

Start dynamic Zstd benchmarking around moderate quality such as 3–6, then
measure representative payloads.

## Do not compress

```text
already-compressed images/video
tiny events where framing/compressor overhead makes output larger
```

For static published assets, aggressive build-time compression is cheaper
because it is performed once.

Do not compress below measured threshold.

## Security note

Compression over HTTPS has security implications where attacker-controlled and
secret material share compressed contexts. Treat that as a security review
item.
