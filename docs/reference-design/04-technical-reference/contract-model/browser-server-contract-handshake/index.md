# Browser/server contract handshake

On BFF initialization, perform a small compatibility handshake.

## Example

```fsharp
type ClientHello =
    {
        AppVersion: string
        ContractVersion: int
        IndexedDbSchemaVersion: int
    }

type ServerHello =
    {
        ServerVersion: string
        MinSupportedContractVersion: int
        MaxSupportedContractVersion: int
        CurrentContractVersion: int
    }
```

## Rule

```text
if client.ContractVersion within supported range:
    continue
else:
    UpgradeRequired
```

Do not make every deployment forcibly break every old open tab.
