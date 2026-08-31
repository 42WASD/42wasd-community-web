# Backup test policy

A backup is not accepted merely because a job says "Succeeded."

Regularly test:

```text
create isolated restore target
restore latest backup
start PostgreSQL
run integrity/application checks
record restore time
destroy test restore
```

## Track

```text
RPO
RTO
last successful backup
last successful restore test
```

Initial desired policy should be explicitly documented based on acceptable
data loss.
