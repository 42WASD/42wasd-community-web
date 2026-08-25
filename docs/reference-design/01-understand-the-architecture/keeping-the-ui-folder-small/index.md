# Keeping the Ui folder small

A global `Components/` directory easily becomes a silent re-creation of
folder-by-type organization:

```text
Components/
├── EventCard
├── EventFilter
├── ProjectEditor
├── MemberProfile
├── LoginForm
├── ...
```

Use a global `Ui/` folder only for truly cross-feature UI:

```text
Layout
Navbar
Footer
Button/primitives
Modal shell
typography helpers
design tokens
```

Keep feature-specific UI beside the feature:

```text
Pages/Events/
├── Events.fs
├── EventCard.fs
└── EventFilters.fs
```

This is the cleanest compromise between feature colocation and reuse. If a
component in `Ui/` turns out to be feature-owned (e.g. `EventCard`,
`ProjectEditor`, `TournamentRegistration`), move it beside that feature.