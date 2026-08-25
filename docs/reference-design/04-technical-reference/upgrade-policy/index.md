# Upgrade policy

Upgrades are adopted deliberately, not automatically.

## Policy

- Keep the dependency set lean.
- When a framework/tool version is used, pin it (e.g. `global.json` for the
  SDK).
- Re-run the full verification pipeline after any upgrade.
- Upgrade a package only when a need is real, or when the current version is
  no longer supported.

## Evidence-based

Follow the current main Bolero repository for the authoritative SDK version
rather than copying an outdated SDK number from an old page.