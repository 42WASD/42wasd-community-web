# Functional core, imperative shell

The selected coding discipline for the whole platform:

# **Functional Core, Imperative Shell**

A pure function:

```fsharp
let renamePost newTitle post =
    if String.length newTitle < 3 then
        Error TitleTooShort
    else
        Ok { post with Title = newTitle }
```

does not:

```text
write database
call HTTP
read wall clock
mutate global singleton
publish queue message
```

It transforms a value into another value/result.

## The shell performs effects

```text
load post from PostgreSQL          IMPURE
            ↓
validate/rename                    PURE
            ↓
save post                          IMPURE
            ↓
write transactional outbox        IMPURE
```

## Benefits

```text
business rules test without infrastructure
less hidden shared mutable state
concurrency is easier to reason about
failures are explicit with Result/DU types
Elmish update traces are deterministic
```

## The boundary

F# is multi-paradigm. Use mutation, arrays, Span-like APIs, classes, and .NET
interfaces where profiling or framework interop requires them. Do not turn
"functional" into a ban on efficient code.

This is the same discipline the Elmish client already follows (pure `update`,
effects isolated in `Cmd`) extended to every backend service.
