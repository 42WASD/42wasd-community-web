# PostgreSQL durability versus caching

Do not confuse:

```text
HybridCache / Dragonfly
```

with:

```text
PostgreSQL storage cache
```

They cache at different layers.

## Complete path

```text
BFF/service L1 object
    ↓ miss
Dragonfly shared object
    ↓ miss
PostgreSQL query
    ↓
PostgreSQL shared buffers / OS page cache
    ↓ miss
NVMe dm-cache
    ↓ miss
HDD
```

One item can be hot at several layers.

Do not cache every query at every layer.
