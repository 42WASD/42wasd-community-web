# Dapr Workflow qualification

Use workflow only when **all** are true:

```text
operation spans meaningful time
needs durable progress
has multiple steps
must survive process restart
has retry/compensation/wait semantics
```

## Do not use a durable workflow for

```text
SELECT topic
```

or:

```text
insert ordinary comment
```

because persistence of every workflow step creates unnecessary latency and
complexity.
