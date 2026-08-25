# Abstraction philosophy

Abstract by evidence of reuse, not by prediction.

## When to abstract

- A pattern repeats three or more times.
- A real, understood dependency inversion is needed (e.g. server vs client
  contracts).
- A change is actually being made, not imagined.

## When not to abstract

- A single-use helper.
- Predicted future needs with no current user.
- Premature interfaces for a domain with one implementation.

## The rule

> Do not build abstractions you do not yet need. Abstract when the second real
> user appears.