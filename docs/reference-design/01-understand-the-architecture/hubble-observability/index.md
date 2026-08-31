# Hubble observability

Enable:

```text
Hubble
Hubble Relay
Hubble UI if useful
Hubble metrics
```

## Operator workflow

```text
request fails
    ↓
inspect Hubble flow
    ↓
source identity
destination identity
port/protocol
policy verdict
    ↓
decide whether connection is legitimate
```

## The rule

Do not auto-create allow rules merely because traffic was observed.

Compromised software also generates traffic.

## Overhead note

Hubble has performance overhead depending on traffic/aggregation; tune
observation rather than assuming infinite free visibility. Avoid exploding
Hubble metric label cardinality.
