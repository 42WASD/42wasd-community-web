# Contract deprecation policy

Maintain at least:

```text
Current contract
Previous compatible contract during rollout window
```

where practical.

## Before removing compatibility

```text
measure active old-client usage
ensure PWA update path works
announce/force reload when necessary
```

Server may reply:

```text
UpgradeRequired
```

only when it cannot safely serve the old contract.
