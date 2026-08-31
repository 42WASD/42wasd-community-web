# YouTube OAuth and resumable upload

Use Google Identity Services' current **authorization code model** and minimum
YouTube scopes (architecture doc Phase 44). The browser initiates the
user-consent flow, but the BFF owns the confidential token exchange and
refresh-token persistence.

## Selected security boundary

```text
Browser
  -> Google Identity Services popup/redirect
  -> receives short-lived authorization code
  -> sends code to BFF over authenticated HTTPS

BFF
  -> validates request/state
  -> exchanges code with Google token endpoint
  -> stores refresh token server-side only
  -> keeps OAuth client secret server-side only
  -> returns/leases only a short-lived access token to browser memory
     when a direct upload is active

Browser
  -> starts YouTube resumable upload directly against Google
  -> video bytes NEVER pass through 42WASD BFF
```

This preserves the BYOA/no-media-proxy requirement while following Google's
safer code-model guidance. Do not use a legacy implicit/token flow merely to
keep all OAuth logic inside WebAssembly.

## Resumable sequence

```text
authorize via code model
BFF exchanges code / secures refresh token
obtain short-lived upload access token in browser memory
start upload session directly with YouTube
save session URI/state
PUT content/chunks directly to YouTube
handle 308 Resume Incomplete
query Range after interruption
resume
complete
store returned video ID
```

## Acceptance

```text
[ ] 1 GB test file does not enter .NET WASM byte[] all at once
[ ] progress shown
[ ] interrupted upload resumes
[ ] browser navigation within SPA does not kill uploader
[ ] full browser termination results in resumable recovery, not false completion
```
