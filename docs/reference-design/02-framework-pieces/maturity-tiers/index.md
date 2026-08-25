# Maturity tiers

A common adoption path from a working shell to a polished product. Do not
optimize for features that do not exist yet.

## Tier A — foundation

Implement immediately:

```text
official Bolero project
one ProgramComponent
Page DU
root Model / Msg
root update
Home + About
shared layout
```

## Tier B — state architecture

Add next:

```text
Shared.Model
RemoteData
page-local Models
nested Page.Msg
Cmd.map
```

## Tier C — real community data

Add after the shell is stable:

```text
Events
Projects / Servers
Members
server remoting
loading/error states
normalized entity caches
```

## Tier D — product polish

Add later:

```text
authentication
account state
theme persistence
analytics
SEO/static rendering decisions
render optimization
```

## The rule

> Do not optimize the architecture for features that do not exist yet. Ship one
> vertical slice first, then grow by evidence.