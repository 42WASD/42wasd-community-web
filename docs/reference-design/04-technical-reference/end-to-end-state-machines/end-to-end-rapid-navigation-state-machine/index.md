# End-to-end rapid navigation state machine

```text
User:
Forum -> Servers -> Forum -> Profile -> Forum
```

## Client coordinator

```text
Forum query already complete?
    yes -> use local result immediately

same Forum query still in flight?
    yes -> new component subscribes to same task

query subscriber count becomes zero?
    cheap/almost complete -> finish/cache
    expensive -> cancel

prefetch no longer adjacent?
    cancel first
```

Server-side cache then collapses identical requests across users further.
