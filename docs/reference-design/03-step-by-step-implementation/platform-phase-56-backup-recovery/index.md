# Backup/recovery

Before forum production data matters (architecture doc Phase 56):

```text
configure physical backup + WAL archive to independent target
schedule
monitor successful backup
perform restore drill
```

Test:

```text
delete disposable database
restore
verify post/account counts
```

A backup that has never been restored is not verified.

## Acceptance

```text
[ ] RPO documented
[ ] RTO documented
[ ] backup target survives primary HDD failure
[ ] restore procedure automated/documented
```
