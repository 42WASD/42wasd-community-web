# DTO projection rule

Never return a persistence entity merely because it already has similar
fields.

## Example database row

```text
account
  id
  primary_email
  password_hash
  security_stamp
  created_at
  last_login_at
  moderation_flags
```

## Browser DTO

```fsharp
type PublicAccountDto =
    {
        Id: AccountId
        DisplayName: string
        Avatar: MediaRef option
    }
```

The mapping boundary is deliberate.

## Reasons

```text
security
API stability
projection size
independent schema evolution
```
