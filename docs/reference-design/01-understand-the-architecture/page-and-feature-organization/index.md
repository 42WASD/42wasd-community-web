# Page and feature organization

**Route is not always feature.** Many routes may belong to one coherent domain
feature.

For a small site:

```text
Pages/
├── Home.fs
├── Events.fs
├── Projects.fs
├── Members.fs
└── About.fs
```

is enough.

If Events grows into:

```text
/events
/events/upcoming
/events/past
/events/{id}
/events/{id}/register
```

do **not** automatically create five unrelated top-level features. Promote
Events into a feature folder:

```text
Features/
└── Events/
    ├── Events.fs
    ├── ListPage.fs
    ├── DetailPage.fs
    ├── RegistrationPage.fs
    ├── EventCard.fs
    └── EventFilters.fs
```

## The rule

> `route != feature` — many routes may belong to one coherent domain feature.

Grow the folder only when the feature earns it.