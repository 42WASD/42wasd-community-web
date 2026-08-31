# Implement the BYOA media provider abstraction

Client (architecture doc Phase 43):

```text
IMediaProviderAdapter
```

Server/BFF:

```text
MediaRef validation
provider allowlist
post attachment state
```

First provider:

```text
YouTube
```

## Acceptance

```text
[ ] post may exist with Pending attachment
[ ] post core does not wait for upload
[ ] provider ID is stored, not fragile generated URL
```
