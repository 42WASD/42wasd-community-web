# BYOA media model

42WASD does **not** become the primary owner or relay of user-uploaded media.
The selected product convention is **Bring Your Own Account (BYOA)**: when a
user wants to attach media, the browser uploads through a provider account
authorized by that user, and 42WASD stores a provider-neutral resource
reference. The user's media remains owned by the user/provider. If the owner
deletes it, the media may disappear from the post by design.

## Recommended contract

```fsharp
type MediaProvider =
    | YouTube
    | ExternalImageProvider of string
    | ExternalFileProvider of string

type MediaKind =
    | Video
    | Image
    | File

type MediaStatus =
    | Pending
    | Available
    | Failed of string
    | Removed

type MediaRef =
    {
        Id: MediaAttachmentId
        Provider: MediaProvider
        ResourceId: string option
        Kind: MediaKind
        Status: MediaStatus
    }
```

## Storage rule

The DB stores provider/resource identity, not a long generated URL.

Provider adapter generates current embed/preview URLs.

## The conflict this resolves

First-party media storage conflicts with this product constraint:

```text
42WASD stores:
provider
resource ID
kind
status
metadata needed by the post

provider stores:
video/image/file bytes
```

The browser talks directly to the provider when the provider's API permits it.

42WASD does not proxy hundreds of megabytes through the home server merely to
attach a forum video.
