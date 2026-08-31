# Interactive versus queued work

## Use direct synchronous execution for

```text
load a topic
create normal comment
edit profile field
authorize group subscription
```

## Use durable queued execution for

```text
import large external history
send many notifications
expensive batch recomputation
provider operation with long retry window
bulk moderation scan
```

## Use Dapr Workflow for

```text
multi-step durable process
waiting on external events
compensation/retry across services
```

## The rule

No framework should infer this decision from function duration alone.

The application declares the semantic execution mode.
