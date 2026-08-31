# Upgrade test matrix

For every significant client release test:

```text
old client + old server
old client + new server
new client + new server
```

where supported.

## For IndexedDB

```text
schema N -> N+1
schema N-1 -> N+1 if direct upgrade supported
```

## For database

```text
old app + expanded schema
new app + expanded schema
```

during rolling transition.

## For service `.proto`

```text
old caller -> new callee
new caller -> old callee
```

where rolling version skew is expected.
