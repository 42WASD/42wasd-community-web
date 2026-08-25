# Production hardening

Prepare the app for production.

## Goal

```text
- deterministic verification pipeline is green (verify.sh)
- server deployable (static hosting + server as appropriate)
- remoting, error boundaries, and logging verified
- performance measured and acceptable
```

## Verification

```bash
bash scripts/docs/verify.sh   # docs pipeline, must report VERIFY OK
```

and the app's own tests pass.