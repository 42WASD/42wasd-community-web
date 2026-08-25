# Framework pieces

Part II interprets the Bolero/Elmish framework pieces in light of the
architecture defined in Part I.

This part is `tracked: false` — it is narrative/conceptual and does not appear
on the Implementation progress page.

## Reading order

- **Capability cheat sheet** — which tool owns which concern.
- **Maturity tiers** — a staged adoption path from foundation to polish.
- **Verified framework behavior** — what the current Bolero docs actually
  state.
- **Failure modes to design for** — symptoms and fixes as the app grows.
- **Testing boundaries** — where the valuable tests live.

The guiding principle is that Bolero and Elmish provide the *machinery*, and
this reference design decides *how to compose it* so the app stays
maintainable.