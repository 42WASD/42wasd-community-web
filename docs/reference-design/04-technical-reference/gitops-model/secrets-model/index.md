# Secrets model

Never commit:

```text
Postgres passwords
OAuth client secrets
YouTube credentials requiring secrecy
RabbitMQ passwords
Dapr secrets
cookie signing/data-protection secrets
```

to Git.

## Use

Use a dedicated secret management mechanism compatible with your cluster.

If introducing an external secret manager later:

```text
application references secret name
secret controller materializes Kubernetes Secret
```

## The rule

Do not let application business code fetch arbitrary cluster Secrets.
