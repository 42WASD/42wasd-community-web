# Failure modes to design for

These are the failure modes this architecture is designed to prevent. Each
shows a symptom and the design rule that prevents it.

## The monolith update

**Symptom:** one huge `update` matches dozens of messages.

**Fix:** nested page-local `Msg` + `Cmd.map`.

## Gigantic Model

**Symptom:** `Model` is a giant record with unrelated fields.

**Fix:** `Shared.Model` for cross-page state, page-local `Model` for the rest,
page-message reductions.

## Deeply nested view with repeated ownership questions

**Symptom:** on every render you wonder "who owns this state?".

**Fix:** the state-ownership rules and page shape.

## `update` returns wrong thing for a sub-feature

**Symptom:** sub-feature state duplicated across multiple parents.

**Fix:** single parent owns a page-local sub-model.

## Re-rendering everything

**Symptom:** every keystroke re-renders the entire page.

**Fix:** use normal pure functions and only `ElmishComponent` when measured.

## Remoting exposed wrong

**Symptom:** server functions mixed with routing or UI concerns.

**Fix:** a remoting API module that isolates server-side effects.

## Update function becomes un-testable

**Symptom:** no pure functions remain.

**Fix:** keep `update` pure; isolate effects in `Cmd`; test `update` directly.

## Route/state contradiction

**Symptom:** URL and visible page disagree after navigation.

**Fix:** single routing source of truth bound to a model field; page-level state
lives in `PageModel<'T>`.

## State that does not survive navigation

**Symptom:** a wizard's step resets on refresh.

**Fix:** the state-lifetime rule decides what is in the route URL versus
`PageModel<'T>` versus `Shared.Model`.

## Platform failure modes

The platform layers add their own failure modes; the architecture's required
behavior for each is captured in the platform phases and the end-to-end state
machines in Part IV. The load-bearing ones:

```text
cached data appears but server truth changed -> foreground active-scope revalidate, never blank the page
client missed SignalR events                 -> detect sequence gap, delta-sync from cursor
old application after server upgrade         -> contract handshake -> UpgradeRequired + reload UI
IndexedDB upgrade blocked by old tab         -> handle blocked/versionchange, prompt reload
cache contains old representation after deployment -> version prefix in cache keys, explicit invalidation, bounded TTL
Dragonfly unavailable                        -> L1 still works, origin load increases, protect origin
PostgreSQL unavailable                       -> writes fail clearly; stale reads only if policy allows
two users edit same post                     -> optimistic version conflict, no silent overwrite
duplicate RabbitMQ delivery                  -> consumer idempotency/inbox, never assume exactly-once
outbox event published twice                 -> event ID + consumer dedup/idempotent projection update
outbox publisher dies post-publish           -> duplicate publish tolerated by consumers
BFF request disconnects during expensive query -> propagate CancellationToken; do not cancel shared fetch while other subscribers remain
browser leaves during durable job            -> job continues, reload status by JobId
browser dies during YouTube upload           -> resume from saved resumable-session state
user deletes BYOA media                      -> MediaRef marked Removed, post remains (product behavior)
external provider API is rate limited        -> backoff, cached metadata, never block core content
SignalR Pod dies                             -> reconnect to another Pod, rejoin active groups, cursor repair
SignalR backplane fails                      -> realtime degraded; durable truth remains correct
Dapr sidecar unavailable                     -> readiness prevents routing to a broken sidecar path
RabbitMQ queue grows without bound           -> depth/age alerts, admission control, dead-letter policy
HDD stalls under DB load                     -> monitor await/queue depth/dm-cache hit ratio
NVMe cache fails (writethrough)              -> origin holds committed data; restore cache acceleration
NVMe cache fails (writeback)                 -> dirty blocks may be lost — why writeback is deferred
same disk holds primary and "backup"         -> not disaster recovery; disk failure loses both
bad migration deployed                       -> lint, staging apply, restore point, expand/contract, manual approval for destructive DDL
Cilium default deny breaks a dependency      -> Hubble inspection, narrow policy update, keep deny
audit mode left on in production             -> alert/config check; audit is discovery, not enforcement
Argo detects live drift                      -> Git is desired state; fix Git, not kubectl edit
```

## The nuance behind the one-liners

A few of these have important detail worth spelling out:

- **Old application after server upgrade:** never let incompatible old browser
  code mutate new schema semantics silently.
- **IndexedDB upgrade blocked by old tab:** do not delete important local
  stores merely to unblock the upgrade.
- **PostgreSQL unavailable:** reads may optionally serve explicitly
  stale/non-authoritative cached projections if product policy allows — but do
  not acknowledge a forum post as durable if it exists only in RAM/cache.
- **Cache contains old representation:** never deserialize incompatible
  arbitrary old cache content without version handling.
- **Browser dies during YouTube upload:** on next app start, discover the
  incomplete upload, ask/auto-resume per policy, query the provider's uploaded
  range, and continue. Do not promise universal browser background execution.
- **Bad migration deployed:** avoid irreversible destructive DDL in the same
  release that first stops writing the old shape.
- **Cilium default deny breaks a dependency:** do not disable default deny as
  the permanent fix.
- **Argo detects live drift:** do not normalize `kubectl edit` as the
  operations model.

Each of these has a designed response in the architecture; none is left to
improvisation at incident time.