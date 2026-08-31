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

## Scope notes

Four framing notes govern everything in this reference design:

> **Scope note:** this is a systems-architecture and implementation guide. It
> deliberately separates browser UI state, browser persistent cache,
> browser/server synchronization, realtime notification, browser-facing RPC,
> business/domain services, database persistence, distributed caching, durable
> asynchronous work, external media ownership, Kubernetes networking, storage
> tiering, and GitOps deployment. No single framework solves all of these
> problems, and treating them as one problem creates a fragile system.

> **Deployment note:** the exact Kubernetes distribution, ingress/VPS path, and
> public-domain routing that deliver a request to the Community Web Pod are
> outside this document unless they materially affect application behavior.
> This guide begins at the application/Kubernetes workload boundary.

> **Storage note:** the selected database architecture assumes approximately
> **30 GB of available NVMe capacity used as a hot block cache in front of
> approximately 1 TB of HDD-backed capacity**. The exact Linux block devices
> are intentionally not hard-coded. Device creation commands are destructive
> and must only be run after verifying device names, backups, and recovery
> procedures.

> **Media rule:** 42WASD does **not** become the primary owner or relay of
> user-uploaded media. The selected product convention is **Bring Your Own
> Account (BYOA)**: when a user wants to attach media, the browser uploads
> through a provider account authorized by that user, and 42WASD stores a
> provider-neutral resource reference. The user's media remains owned by the
> user/provider. If the owner deletes it, the media may disappear from the
> post by design.

## Verification pass

The source architecture document was re-audited against current primary
documentation before being labeled implementation-grade (2026-08-27). That
pass corrected eleven areas that were too loose in the first edition, and the
corrections are baked into these pages:

```text
1.  Google/YouTube OAuth: browser-direct media bytes remain selected, but
    confidential code exchange and refresh-token custody belong to the BFF.
2.  ASP.NET Core Identity: use the mature EF Core + Npgsql Identity store
    inside Accounts instead of leaving a custom SQLProvider Identity store
    implicit.
3.  Atlas: `atlas migrate lint` is Atlas Pro from v0.38; OSS CI must not
    assume that command is available.
4.  Kubernetes local storage: local PV requires nodeAffinity and should use
    WaitForFirstConsumer binding.
5.  CloudNativePG operand image: do not use deprecated rolling `:18` tags; pin
    an explicit supported image/digest or a verified ClusterImageCatalog.
6.  CloudNativePG backup path: new deployments should prefer the Barman Cloud
    CNPG-I plugin instead of deprecated in-tree Barman Cloud configuration.
7.  PgBouncer/Npgsql: do not casually stack two pools; start with Npgsql
    pooling and add PgBouncer session pooling only when connection pressure
    justifies it.
8.  Dapr service authorization: Cilium default-deny is not a Dapr invocation
    ACL; configure Dapr accessControl explicitly because no ACL means
    allow-all invocation.
9.  Dapr RabbitMQ durability: explicitly enable durable queues, persistent
    delivery, publisher confirms, bounded prefetch and dead-letter behavior
    instead of relying on component defaults.
10. Multi-replica rate limiting: ASP.NET Core's in-process limiter protects
    each replica; strict product-wide quotas need a shared enforcement
    point/counter.
11. Platform compatibility preflight: verify the actual Kubernetes minor,
    Cilium health and storage device topology before selecting operator
    versions or running destructive LVM commands.
```

The architecture itself remains unchanged: route-scoped browser
synchronization, PostgreSQL as truth, disposable cache layers, BFF/browser
boundary, coarse backend services, durable asynchronous work, Cilium network
enforcement, and GitOps remain the selected design.

## Add a part

1. Create a folder under `docs/reference-design/<part-slug>/`.
2. Add it to `_sequence.yaml` under `parts`.
3. Re-run `scripts/docs/verify.sh` to regenerate nav + progress.

See `scripts/docs/README.md` for the full technique.