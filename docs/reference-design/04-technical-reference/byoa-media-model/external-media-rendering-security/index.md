# External media rendering security

Treat media providers as untrusted external content sources.

## Use

```text
allowlisted providers
provider-specific embed templates
Content Security Policy
sandboxed iframe settings where appropriate
referrer/privacy settings
URL/resource ID validation
```

## Do not

Do not accept arbitrary HTML embed code from users.

For unknown URLs:

```text
show link
```

rather than injecting provider-supplied markup.
