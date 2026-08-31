# Framework pieces

Part II interprets the Bolero/Elmish framework pieces in light of the
architecture defined in Part I.

This part is `tracked: false` — it is narrative/conceptual and does not appear
on the Implementation progress page.

## Reading order

- **Capability cheat sheet** — which tool owns which concern, plus the
  platform capability matrix (component × concern boundaries).
- **Maturity tiers** — a staged adoption path from foundation to polish, plus
  the platform tiers A–F.
- **Verified framework behavior** — what the current Bolero docs actually
  state, plus the 2026-08-27 platform verification notes and version
  decisions.
- **Failure modes to design for** — symptoms and fixes as the app grows, plus
  the platform failure-mode table.
- **Testing boundaries** — where the valuable tests live.
- **Observability requirements** — what to track per boundary.
- **Security boundaries** — what each boundary must never leak or trust.

The guiding principle is that Bolero and Elmish provide the *machinery*, and
this reference design decides *how to compose it* so the app stays
maintainable.