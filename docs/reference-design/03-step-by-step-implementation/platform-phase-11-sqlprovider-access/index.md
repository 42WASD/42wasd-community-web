# Add SQLProvider PostgreSQL access

Create a server/infrastructure persistence project or module (architecture doc
Phase 11).

Example structure:

```text
src/
└── Community.Persistence/
    ├── Database.fs
    ├── ForumQueries.fs
    ├── AccountQueries.fs
    └── Mapping.fs
```

Use SQLProvider against the schema.

Keep mapping boundary:

```text
SQLProvider row/entity
      ↓
domain type
      ↓
Shared DTO when crossing browser boundary
```

Do not reference SQLProvider types from `Community.Web.Client`.

## Acceptance

```text
[ ] build obtains typed schema safely
[ ] runtime connection string comes from secret/config
[ ] no production DB password needed at browser/build artifact
[ ] one read query works
[ ] one transactional write works
```
