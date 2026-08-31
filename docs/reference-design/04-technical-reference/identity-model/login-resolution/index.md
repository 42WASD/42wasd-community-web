# Login resolution

Example:

```text
Sign in with Discord
       ↓
Discord subject = D123
       ↓
lookup ExternalLoginCredential
       ↓
exactly one Account
       ↓
authenticate account
       ↓
user chooses/uses active Persona
```

## Multiple public identities

If a user wants two public identities:

```text
one Account
  ├── Persona A
  └── Persona B
```

is preferred over making the same Discord login ambiguously authenticate two
separate Accounts.
