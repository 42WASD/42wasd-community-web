# End-to-end external account login

```text
Browser
  ↓
Sign in with provider
  ↓
provider OAuth/OpenID flow
  ↓
BFF callback
  ↓
provider subject verified
  ↓
ExternalLoginCredential lookup
  ↓
Account established
  ↓
active Persona selected
  ↓
auth cookie/session issued
```

## Linking

Linking Steam/FACEIT/etc. can be:

```text
integration/profile link
```

without necessarily becoming primary login credentials.
