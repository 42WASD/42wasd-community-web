# Client prefetch policy

Prefetch may occur only for high-probability adjacent data.

## Examples

```text
next forum page when user is near bottom
linked topic when pointer/focus indicates likely navigation
next comments page near viewport end
```

## Do not prefetch

```text
all forums
all user profiles
all old topics
```

because network bandwidth is a constrained resource.

Prefetch uses lower priority than visible queries and should be first to
cancel.
