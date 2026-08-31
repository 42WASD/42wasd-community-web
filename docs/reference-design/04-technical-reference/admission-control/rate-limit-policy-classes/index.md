# Rate-limit policy classes

Define named policies:

```text
anonymous-read
authenticated-read
mutation-standard
authentication-sensitive
expensive-search
external-provider
background-admission
signalr-connect
```

## Use combinations of

```text
token bucket
sliding window
concurrency limiting
```

## Key by

```text
authenticated account when possible
IP for anonymous/abuse boundary
provider credential for provider quota
```

## The rule

Do not use IP alone for authenticated users behind carrier-grade NAT.
