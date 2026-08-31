# Product SLO candidates

Do not adopt these exact numbers without measuring, but define targets in this
form:

```text
cached route usable:
  p95 < X ms

active-scope fresh confirmation:
  p95 < X ms

normal forum mutation:
  p95 < X ms

SignalR visible update:
  p95 < X ms

DB pool wait:
  p95 < X ms

queue job acceptance:
  p95 < X ms
```

## For progressive pages, define

```text
time to first useful content
```

separately from:

```text
time to all optional enrichment
```

This rewards the architecture for rendering fast data early.
