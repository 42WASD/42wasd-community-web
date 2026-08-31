# Database growth policy

Track:

```text
database size
largest relations
index size
WAL rate
HDD fill rate
```

At thresholds such as:

```text
70%
80%
90%
```

have explicit actions.

Do not discover that the 1 TB disk is full because PostgreSQL suddenly stops
writing.

## Potential actions

```text
vacuum/index tuning
archive old event logs
partition retention
increase storage
move explicitly cold datasets
```

## The rule

Never auto-delete business data without retention policy.
