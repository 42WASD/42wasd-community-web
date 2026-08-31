# Hubble operator queries

Keep operational recipes for:

```text
flows from community-web
flows to PostgreSQL
dropped flows only
policy verdicts only
namespace-specific flows
DNS failures
```

The specific CLI syntax should be validated against installed Cilium/Hubble
version.

## The important operational property

```text
an operator can answer:
"what tried to connect, from where, to where, on which port, and why was it denied?"
```

within minutes.
