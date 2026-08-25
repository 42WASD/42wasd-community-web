# Developer vs gaming community

The architecture does **not** change between community types — only the
domain vocabulary and theme.

## Developer community domain

```text
Community.Shared/Domain/
├── Event.fs
├── Project.fs
├── Member.fs
├── Organization.fs
└── Sponsor.fs
```

Pages/features:

```text
Home
Events
Projects
Members
About
```

Typical home sections:

```text
Hero
Stats
Upcoming events
Featured projects
Community members
Sponsors
Join Discord / GitHub
```

## Gaming community domain

```text
Community.Shared/Domain/
├── Game.fs
├── GameServer.fs
├── Tournament.fs
├── Player.fs
├── Team.fs
└── News.fs
```

Pages/features:

```text
Home
Servers
Tournaments
Games
Members
About
```

Typical home sections:

```text
Hero
Online/community stats
Games we play
Active servers
Upcoming tournaments
Latest news
Join Discord
```

Same architecture. Different vocabulary and theme.