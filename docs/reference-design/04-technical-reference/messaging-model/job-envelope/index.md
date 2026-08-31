# Job envelope

Conceptual:

```protobuf
message JobEnvelope {
  string job_id = 1;
  string job_type = 2;
  string requested_by_account_id = 3;
  google.protobuf.Timestamp created_at = 4;
  bytes payload = 5;
}
```

## Better in real `.proto` contracts

```text
use typed job messages
```

rather than one giant arbitrary byte payload when job families are stable.

Include:

```text
idempotency ID
correlation/trace ID
schema/version information
```

as required.
