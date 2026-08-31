# ASP.NET Data Protection

Because BFF replicas may need to understand the same authentication/session
protection state, review ASP.NET Core Data Protection key persistence.

## Do not rely on

```text
ephemeral per-Pod keys
```

if a cookie encrypted by Pod A must be understood by Pod B after restart.

Persist/share key material using an appropriate protected store.

Treat Data Protection keys as secrets.
