# Dapr Workflow

Use for genuinely multi-step durable flows.

## Example

```text
CreateTournament
     ↓
validate configuration
     ↓
reserve resources
     ↓
generate bracket
     ↓
create schedule
     ↓
publish notifications
     ↓
complete
```

## Every step needs

```text
idempotency
timeout
retry policy
compensation/failure behavior
observable status
```

## The rule

Do not use durable workflow machinery for a 5 ms `GetPost`.
