# Add page-level progressive aggregation

For each complex page classify source (architecture doc Phase 46):

```text
critical above-fold
optional above-fold
below-fold
external slow
```

Start independent requests.

Prioritize critical data.

Example:

```text
Topic core        critical
reaction counts   useful
server status     optional/independent
YouTube metadata  optional/external
```

## Acceptance

```text
[ ] slow YouTube does not delay post text
[ ] source failure affects only corresponding component
```
