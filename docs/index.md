# 42WASD Community Web Docs

Welcome to the documentation for **42WASD Community Web**.

This site is built on a **Single-Source-of-Truth (SSOT)** reading-order
manifest. Read `scripts/docs/README.md` to understand the technique before
adding content.

## Reference Design

See the Reference Design section (auto-generated nav) for the ordered parts and
phases. Only **actionable** phases (marked `tracked: true` in the manifest)
appear on the Implementation progress page.

## Quick start

```bash
# Install deps
cd projects && uv sync

# Validate + build (one command, same as CI)
bash ../scripts/docs/verify.sh
```

## Implementation progress

See the Implementation page for live build progress (generated from
`docs/implementation/progress.yaml`).