# YouTube upload client rules

Use the provider's current authorization-code model. For YouTube/Google, the
browser obtains an authorization code, while the BFF performs the confidential
code exchange and stores refresh tokens.

## Never expose

```text
server OAuth client secret
refresh token
long-lived provider credential
```

to WebAssembly or IndexedDB. If the browser must upload directly to the
provider, give it only the short-lived access token needed for that active
upload and keep it in memory rather than durable browser storage.

## Memory and resumability

Use resumable upload.

Persist only:

```text
upload state/session reference
confirmed byte position
post association
provider resource identity when available
```

Do not copy the entire video into the .NET WASM managed heap.

Use browser `File`/`Blob` APIs and chunked reads.

## Browser termination

If the browser process terminates:

```text
the upload may stop
```

and the next app session resumes when the provider/session permits.

Do not promise guaranteed background execution across browser termination.
