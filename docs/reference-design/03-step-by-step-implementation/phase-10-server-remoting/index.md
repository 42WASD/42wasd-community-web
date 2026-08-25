# Server remoting

Expose server functions over Bolero remoting.

## Goal

```text
configure a remoting service in the server
client calls shared async functions
```

## Verification

Use `curl` against the remoting endpoint to confirm JSON responses.

> Remember: an F# `unit` serializes to JSON `null` for remoting — not `[]` and
> not an empty body. Sending `[]` or an empty body causes errors; `null` works.