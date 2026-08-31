# Database indexing review

For every important query capture (architecture doc Phase 54):

```text
WHERE
ORDER BY
JOIN
LIMIT
```

Use `EXPLAIN (ANALYZE, BUFFERS)` in test/staging against realistic data.

## Typical indexes

```text
forum list:
(category_id, bumped_at DESC, id DESC)

comments:
(topic_id, created_at, id)

user content:
(author_id, created_at DESC, id DESC)

outbox:
(published_at/null status, created_at)
```

Do not create every possible index. Every index consumes disk and write work.
