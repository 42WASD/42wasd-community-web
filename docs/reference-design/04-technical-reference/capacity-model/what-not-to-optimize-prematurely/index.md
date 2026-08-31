# What not to optimize prematurely

Do not start with:

```text
custom binary browser protocol
Cap'n Proto everywhere
WebTransport replacement for SignalR
custom scheduler
custom database
custom distributed cache
custom Kubernetes operator
```

until profiling demonstrates a measured bottleneck.

The selected stack already gives:

```text
binary internal RPC
binary realtime
Zstd HTTP compression
request dedupe
multi-level cache
delta synchronization
direct external media
```

which removes the largest predictable waste.
