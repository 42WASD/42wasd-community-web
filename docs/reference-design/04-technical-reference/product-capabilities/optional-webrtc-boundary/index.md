# Optional WebRTC boundary

Use WebRTC only for traffic where peer-to-peer bandwidth can materially help:

```text
voice
video
large direct file transfer
some game-state cases
```

## Do not use it to synchronize forum truth.

Normal text DM:

```text
SignalR
```

is simpler and the bandwidth is tiny.

## Even WebRTC requires

```text
signaling
STUN
TURN fallback
authentication
```

and TURN relay can still consume server/provider bandwidth.
