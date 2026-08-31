# Change-log retention

The client cannot assume a cursor is valid forever.

Define:

```text
change retention window
```

Example policy:

```text
retain sufficient history for normal reconnect windows
```

## If

```text
client cursor older than retained history
```

server returns:

```text
ResetRequired
```

Then active scope performs:

```text
fresh snapshot
```

instead of trying to replay missing history.

This lets change tables remain bounded.
