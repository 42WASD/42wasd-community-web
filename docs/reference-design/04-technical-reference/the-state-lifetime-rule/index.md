# The state lifetime rule

State lifetime determines where state lives.

## Lifetime table

| Lifetime | Lives in |
|---|---|
| Survives navigation, needed across pages | `Shared.Model` |
| Needed during one page session | `PageModel<'T>` / page-local `Model` |
| Derived from route | `Page` route in root model |
| Ephemeral UI input | page-local `Model` / `PageModel<'T>` |

## Rule

> A wizard's or page's transient state survives refresh only if it lives in
> `Shared` (persistent), otherwise it lives in `PageModel`/page-local and
> resets on navigation.