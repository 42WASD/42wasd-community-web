# Reference Design

Welcome to the reference design for **42WASD Community Web**.

This section is organized as a **reading order**: parts (I, II, III, …) and
phases (numbered 1, 2, 3, …) are **derived** from the SSOT manifest at
`docs/reference-design/_sequence.yaml` — never stored in the pages themselves.

- **Parts** = top-level sections of your design (e.g. "Understand the system",
  "Step-by-step implementation").
- **Tracked parts** appear on the [Implementation](../implementation/index.md)
  progress page as numbered **phases**.
- **Untracked parts** are narrative/conceptual and do not get phase numbers.

## Add a part

1. Create a folder under `docs/reference-design/<part-slug>/`.
2. Add it to `_sequence.yaml` under `parts`.
3. Re-run `scripts/docs/verify.sh` to regenerate nav + progress.

See `scripts/docs/README.md` for the full technique.