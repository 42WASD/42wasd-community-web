# Hubble-driven network-policy workflow

For every new service (architecture doc Phase 51):

```text
1. deploy staging under audit/discovery approach
2. execute integration/e2e tests
3. inspect Hubble flows
4. compare with declared dependency graph
5. generate/propose policy
6. review
7. merge
8. Argo enforces
9. run tests again under enforcement
```

Never:

```text
observed traffic -> automatic permanent allow
```

## Acceptance

```text
[ ] undeclared egress blocked
[ ] declared dependencies work
[ ] Hubble explains drops
```
