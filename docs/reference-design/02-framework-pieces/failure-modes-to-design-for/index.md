# Failure modes to design for

These are the failure modes this architecture is designed to prevent. Each
shows a symptom and the design rule that prevents it.

## The monolith update

**Symptom:** one huge `update` matches dozens of messages.

**Fix:** nested page-local `Msg` + `Cmd.map`.

## Gigantic Model

**Symptom:** `Model` is a giant record with unrelated fields.

**Fix:** `Shared.Model` for cross-page state, page-local `Model` for the rest,
page-message reductions.

## Deeply nested view with repeated ownership questions

**Symptom:** on every render you wonder "who owns this state?".

**Fix:** the state-ownership rules and page shape.

## `update` returns wrong thing for a sub-feature

**Symptom:** sub-feature state duplicated across multiple parents.

**Fix:** single parent owns a page-local sub-model.

## Re-rendering everything

**Symptom:** every keystroke re-renders the entire page.

**Fix:** use normal pure functions and only `ElmishComponent` when measured.

## Remoting exposed wrong

**Symptom:** server functions mixed with routing or UI concerns.

**Fix:** a remoting API module that isolates server-side effects.

## Update function becomes un-testable

**Symptom:** no pure functions remain.

**Fix:** keep `update` pure; isolate effects in `Cmd`; test `update` directly.

## Route/state contradiction

**Symptom:** URL and visible page disagree after navigation.

**Fix:** single routing source of truth bound to a model field; page-level state
lives in `PageModel<'T>`.

## State that does not survive navigation

**Symptom:** a wizard's step resets on refresh.

**Fix:** the state-lifetime rule decides what is in the route URL versus
`PageModel<'T>` versus `Shared.Model`.