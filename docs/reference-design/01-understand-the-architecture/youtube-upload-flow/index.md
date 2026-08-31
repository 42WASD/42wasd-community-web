# YouTube upload flow

Selected client flow:

```text
user selects File/Blob
      ↓
show local preview immediately
      ↓
authorize YouTube account
      ↓
create forum post with pending MediaRef
      ↓
start YouTube resumable upload
      ↓
persist upload session metadata in IndexedDB
      ↓
report progress in Elmish
      ↓
user may navigate elsewhere in SPA
      ↓
upload completes
      ↓
receive YouTube video ID
      ↓
update post MediaRef -> Available
```

## Memory rule

Do not copy the entire video into the F# WASM managed heap.

Keep the browser `File`/`Blob` as the source and stream/upload through the
provider API.

## Security boundary

Use Google Identity Services' **authorization code model**: the browser
initiates the user-consent flow, but the BFF owns the confidential token
exchange and refresh-token persistence. Do not use a legacy implicit/token
flow merely to keep all OAuth logic inside WebAssembly.

## Background execution caveat

If the browser/OS terminates, background execution is not universally
guaranteed. Resume from saved resumable-session state when the app returns.
