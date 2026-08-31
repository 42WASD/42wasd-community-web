# Search architecture

Start with PostgreSQL search capabilities.

Do not deploy Elasticsearch/OpenSearch immediately merely because the product
has a forum.

## First

```text
appropriate indexes
PostgreSQL full-text search
measured query plans
```

## Extract a Search service/engine when

```text
relevance requirements exceed Postgres solution
scale requires independent indexing
cross-domain search becomes complex
```

## If extracted

```text
PostgreSQL remains source of truth
search index is rebuildable projection
```
