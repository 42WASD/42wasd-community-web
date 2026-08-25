# Testing boundaries

Not every line of Bolero UI needs a test. The valuable tests are concentrated in
the pure, decidable logic.

## What to test

```text
pure update functions
routing round-trips
business invariants
remote data reductions
```

## What to keep thin

```text
view code (mostly structural)
pure visual helpers (delegate to snapshots only when stable)
```

## How

```text
use a test framework (expecto / xUnit)
pure update tests need no DOM
routing tests run Router.infer round-trips
integration tests can use Playwright / browser automation against a running app
```

## The boundary rule

> The best return is testing the state transitions that encode real rules.
> The rendering is covered by the framework and by a handful of integration
> tests.