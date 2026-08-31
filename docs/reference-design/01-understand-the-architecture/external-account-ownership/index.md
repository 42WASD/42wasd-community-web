# External account ownership rules

Separate:

```text
ExternalLoginCredential
    proves sign-in

LinkedExternalProfile
    integration/display capability
```

## Examples

```text
Google     -> login credential
Discord    -> login + linked profile
Steam      -> linked game identity, optionally login
FACEIT     -> linked game identity
YouTube    -> BYOA media authorization, not necessarily login
```

## Why

This prevents one integration from accidentally becoming the canonical owner
of all platform content. A provider that can authenticate you does not
automatically own your posts, and a provider you linked for display does not
automatically authenticate you.
