# Stateful page — PageModel

Use `PageModel<'T>` for page state that is not part of the URL.

## Goal

```text
a page with form/draft state that must not appear in the URL
state lives in PageModel<'T>, excluded from the route
```

## Implementation

```text
Router.inferWithModel supplies defaults for PageModel
PageModel<'T> holds the transient page state
```

## Verification

The page keeps its transient state across in-page updates but resets
appropriately per the state-lifetime rule.