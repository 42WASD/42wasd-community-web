# Technical reference

Part IV is a compact technical reference for the design decisions in this
document. It is `tracked: false` — narrative/reference, not on the progress
page.

## Reading order

The sections capture the canonical patterns and rules:

- **The root source of truth model** — how state is shaped.
- **The root message** — how messages flow.
- **The recommended page shape** — the standard page module.
- **The recommended RemoteData** — async state handling.
- **Normalized entity state** — canonical entity caches.
- **The events page state** — a concrete example.
- **The dependency rule** — who may depend on whom.
- **The routing state rule** — route as source of truth.
- **The state lifetime rule** — how long state survives.
- **The functional acceptance test** — what "done" means.
- **Performance principles** — when to optimize.
- **Upgrade policy** — how upgrades are adopted.
- **Abstraction philosophy** — when to abstract.
- **Primary references** — the evidence base.
- **The final recommendation** — the one-paragraph conclusion.