# Authentication

Add authentication to the community site.

## Goal

```text
sign in / sign out
account state survives refresh (persistent lifetime)
auth-gated pages redirect or show a sign-in prompt
```

## Implementation

```text
Account state lives in Shared.Model
authentication messages on the root or a dedicated auth feature
server validates credentials and returns the account / null
```

## Verification

Sign in, refresh, and remain signed in. Sign out clears the account state.
Auth-gated pages behave correctly when logged out.