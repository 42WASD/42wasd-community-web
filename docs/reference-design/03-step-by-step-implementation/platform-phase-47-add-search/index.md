# Add Search

Start with PostgreSQL search (architecture doc Phase 47).

Do not deploy OpenSearch/Elasticsearch immediately.

Use:

```text
indexes
full-text search
trigram extension if justified and supported
cursor pagination
```

Only extract Search Service or dedicated search engine when:

```text
query features
scale
ranking
index isolation
```

prove PostgreSQL insufficient.

## Acceptance

```text
[ ] search has latency budget
[ ] query rate limited
[ ] search does not scan entire table per keystroke
```
