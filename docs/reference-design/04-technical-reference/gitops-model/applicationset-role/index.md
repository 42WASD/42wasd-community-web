# ApplicationSet role

Use ApplicationSet for repeated environment/application generation where it
reduces duplication.

## Example

```text
same application
  staging overlay
  production overlay
```

or service discovery from a controlled directory.

## The rule

Do not use generators so dynamically that operators cannot tell which
repository/revision owns a production object.

Generated Argo Applications are still GitOps-controlled resources.
