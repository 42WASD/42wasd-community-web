# Image/file BYOA provider policy

Do not hard-code one provider until its current API and terms explicitly
permit:

```text
user OAuth/account authorization
browser-originated upload
embedding/hotlinking for your use
acceptable quotas
stable resource identity
```

## Adapter

Provide:

```text
IMediaProviderAdapter
```

and make YouTube the first verified video adapter.

## Baseline fallback for other media

```text
user pastes an external URL
platform stores provider/resource/url metadata
client renders only providers/domains allowed by policy
```

Never make arbitrary HTML embed code from a user trusted.
