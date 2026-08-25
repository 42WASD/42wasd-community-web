# More features

Add the remaining community features following the same shape.

## Features

```text
Events
Projects (developers) / Servers (gaming)
Members
Search
```

## Shape

```text
page-local model + RemoteData + canonical cache
list -> detail or cards
loading / loaded / failed states
```

## Verification

Each feature survives navigate-away-and-back in the correct state, and all
entities share normalized canonical caches.