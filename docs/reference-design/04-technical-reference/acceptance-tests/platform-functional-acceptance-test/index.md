# Platform functional acceptance test

A production release is not complete until these pass. This extends the
original feature-level functional acceptance test (navigate/see/act/return)
with the platform-level checklists.

## Client caching

```text
[ ] first route visit fetches only required viewport data
[ ] repeat route renders from IndexedDB
[ ] inactive forum route causes no forum synchronization
[ ] inactive forum route has no forum SignalR subscription
[ ] stale active route revalidates once on activation
[ ] reconnect syncs only active scopes
[ ] cursor advances only after local data transaction commits
[ ] tombstone removes deleted cached entity
```

## Request coordination

```text
[ ] two components requesting same key produce one network fetch
[ ] leaving one component does not cancel while another subscribes
[ ] zero subscribers cancels expensive request
[ ] completed batch remains cached after route changes
[ ] low-priority prefetch cancels before visible query
```

## PWA/versioning

```text
[ ] refresh normally serves cached runtime/static assets
[ ] new release downloads coherent new asset snapshot
[ ] old tab behavior tested
[ ] IndexedDB upgrade preserves drafts/outbox/uploads
[ ] incompatible contract produces controlled UpgradeRequired
```

## Database

```text
[ ] JSON files are no longer authoritative
[ ] Postgres writes survive Pod restart
[ ] Atlas migration status clean
[ ] PgBouncer pool budget verified
[ ] optimistic concurrency conflict tested
[ ] change-log retention/reset path tested
```

## Cache

```text
[ ] destroying Dragonfly does not lose business data
[ ] L1/L2 hit metrics visible
[ ] cache stampede test bounded
[ ] stale authorization is not possible due to unsafe caching
```

## Realtime

```text
[ ] Topic A subscriber does not receive Topic B event
[ ] authorization checked before joining private group
[ ] missed realtime events repaired by delta sync
[ ] duplicate event ignored via version/sequence
[ ] two BFF replicas scale-out path tested
```

## Messaging

```text
[ ] business write + outbox commit atomic
[ ] duplicate broker delivery does not duplicate business effect
[ ] worker failure returns message/retries correctly
[ ] poison message reaches dead-letter policy
[ ] queue depth/backpressure metrics alert
```

## Media

```text
[ ] video bytes go browser -> provider, not through BFF
[ ] resumable session resumes after network interruption
[ ] post handles provider deletion
[ ] invalid provider/resource ID rejected
[ ] arbitrary embed HTML never accepted
```

## Auth

```text
[ ] external provider maps to one canonical Account
[ ] same email does not silently merge accounts
[ ] Personas work independently of login identity
[ ] linked profile cannot impersonate login credential
```

## Storage

```text
[ ] database volume uses intended cached LV
[ ] dm-cache mode/policy verified
[ ] reboot mount/PV path verified
[ ] HDD/NVMe health metrics visible
[ ] database restore tested onto independent target
```

## Cilium

```text
[ ] default deny active
[ ] DNS still functions where required
[ ] every intentional dependency has explicit allow
[ ] Hubble displays denied test flow
[ ] unexpected egress remains denied
[ ] Policy Audit Mode disabled in production
```

## GitOps

```text
[ ] Argo shows application Healthy/Synced
[ ] cluster-wide resources cannot be created from app project
[ ] app-specific policy lives with app deployment
[ ] third-party versions are pinned
[ ] secrets absent from Git
```
