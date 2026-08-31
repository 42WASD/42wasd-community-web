# Protobuf evolution rules

Follow additive evolution first.

## Safe pattern

```text
add field with new field number
add new RPC
add new message
```

## Breaking pattern

```text
reuse old field number
change semantic meaning silently
change scalar type incompatibly
delete a field number then reuse it
```

Reserve removed field numbers/names.

## Example

```protobuf
message PostSummary {
  string id = 1;
  string title = 2;

  reserved 3;
  reserved "legacy_author_name";

  string author_id = 4;
}
```
