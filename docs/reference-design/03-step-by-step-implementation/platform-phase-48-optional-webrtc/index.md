# Optional WebRTC for heavy P2P

Do **not** use this phase for ordinary text forum correctness (architecture
doc Phase 48).

Use where bandwidth savings matter:

```text
voice
video
large direct file transfer
small realtime peer session
```

Still deploy/obtain:

```text
signaling
STUN
TURN fallback
auth
```

TURN means relay bandwidth may still be required.

For chat text, SignalR + persistent server message history is likely simpler
and cheap enough.
