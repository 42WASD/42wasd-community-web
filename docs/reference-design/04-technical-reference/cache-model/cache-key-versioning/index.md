# Cache key versioning

Use namespaced keys:

```text
forum:topic:v1:{topicId}
forum:category:v3:{categoryId}:{sort}:{cursor}
account:public:v2:{accountId}
server:status:v1:{serverId}
```

When representation changes incompatibly:

```text
v2 -> v3
```

new code naturally misses old keys.

Old keys expire later.

This is safer than trying to deserialize incompatible payloads.
