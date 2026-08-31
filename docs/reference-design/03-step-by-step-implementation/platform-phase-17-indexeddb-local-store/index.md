# Implement IndexedDB local-store abstraction

Create (architecture doc Phase 17):

```fsharp
type ILocalStore =
    abstract GetTopic : TopicId -> Task<PostDetails option>
    abstract PutTopic : PostDetails -> Task<unit>
    abstract GetCursor : ScopeKey -> Task<int64 option>
    abstract ApplyChanges : ScopeKey * ChangeBatch -> Task<unit>
    abstract GetDrafts : unit -> Task<Draft list>
    ...
```

Implement via JS interop/selected IndexedDB wrapper.

Keep API independent of wrapper.

## Acceptance

```text
[ ] cache survives refresh
[ ] cache can be cleared independently
[ ] drafts survive cache reset
[ ] transaction saves entities + cursor atomically
```
