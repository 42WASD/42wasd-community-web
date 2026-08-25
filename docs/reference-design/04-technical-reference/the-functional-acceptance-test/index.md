# The functional acceptance test

What "done" means for a page or feature.

## The acceptance test

A feature is done when a user can:

1. Navigate to it from the current page.
2. See the correct content and state from the server.
3. Act on it (and the act updates state correctly).
4. Navigate away and back and arrive in the same, correct state.

## Verification

```bash
# run the docs verification
bash scripts/docs/verify.sh
```

and for the app's own tests:

```bash
dotnet test
```

## The functional verification

For a feature, run the app, exercise the four steps above, and confirm the
Elmish message trace (browser console) and any remoting round-trip look
correct.