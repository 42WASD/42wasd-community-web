# Core forum schema example

Illustrative starting SQL:

```sql
CREATE SCHEMA IF NOT EXISTS forum;

CREATE TABLE forum.posts (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    author_persona_id uuid NOT NULL,
    category_id uuid NOT NULL,
    title text NOT NULL,
    body text NOT NULL,
    version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE INDEX posts_category_created_idx
    ON forum.posts (category_id, created_at DESC, id DESC);

CREATE TABLE forum.comments (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    post_id uuid NOT NULL REFERENCES forum.posts(id),
    author_persona_id uuid NOT NULL,
    body text NOT NULL,
    version bigint NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE INDEX comments_post_created_idx
    ON forum.comments (post_id, created_at, id);
```

This is illustrative.

Production migrations must be generated/reviewed through Atlas.
