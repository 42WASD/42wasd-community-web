# Database topology

Connection topology, connection budget, CloudNativePG baselines, and the
durability-versus-caching layering.

## Subsections

- **Database connection topology** — Pod → PgBouncer → rw service → primary.
- **Connection-budget formula** — reserve and bound backend sessions.
- **CloudNativePG baseline Cluster** — illustrative Cluster CR.
- **CloudNativePG Pooler baseline** — illustrative Pooler CR.
- **PostgreSQL durability versus caching** — the full cache path.
