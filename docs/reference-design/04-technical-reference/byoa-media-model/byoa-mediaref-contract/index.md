# BYOA MediaRef contract

Selected F# browser contract:

```fsharp
type MediaProvider =
    | YouTube
    | ExternalImageProvider of string
    | ExternalFileProvider of string

type MediaKind =
    | Video
    | Image
    | File

type MediaAvailability =
    | PendingUpload
    | Processing
    | Available
    | RemovedByOwner
    | ProviderError

type MediaRef =
    {
        Provider: MediaProvider
        ResourceId: string option
        Kind: MediaKind
        Availability: MediaAvailability
    }
```

## Database rule

Database stores provider-neutral identity.

Do not store a provider's display URL as the canonical ID if a stable provider
resource ID exists.
