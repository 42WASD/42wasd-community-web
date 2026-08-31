# Cilium policy example

## Illustrative application policy

```yaml
apiVersion: cilium.io/v2
kind: CiliumNetworkPolicy
metadata:
  name: community-web-egress
  namespace: community
spec:
  endpointSelector:
    matchLabels:
      app.kubernetes.io/name: community-web

  egress:
    - toEndpoints:
        - matchLabels:
            k8s:io.kubernetes.pod.namespace: community
            app.kubernetes.io/name: forum-service
      toPorts:
        - ports:
            - port: "50051"
              protocol: TCP

    - toEndpoints:
        - matchLabels:
            k8s:io.kubernetes.pod.namespace: data
            app.kubernetes.io/name: dragonfly
      toPorts:
        - ports:
            - port: "6379"
              protocol: TCP
```

The exact labels/ports must match actual manifests.

DNS and Dapr paths require explicit additional rules.

Do not assume this example alone is a complete production policy.
