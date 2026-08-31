# Identity data model

Recommended conceptual tables:

```text
identity.accounts
identity.personas
identity.external_login_credentials
identity.linked_external_profiles
```

## Account

```text
canonical authentication/security principal
```

## Persona

```text
one public identity presented inside 42WASD
```

One account may own several personas.

## ExternalLoginCredential

```text
provider identity allowed to authenticate the account
```

Keep unambiguous.

Normally:

```text
(provider, provider_subject) UNIQUE
```

## LinkedExternalProfile

```text
informational/integration association
```

May be shown on several personas depending on product policy without making
authentication ambiguous.
