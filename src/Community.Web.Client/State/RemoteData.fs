namespace Community.Web.Client.State

/// The recommended single `RemoteData<'T>` type for async server-backed values.
///
/// Use this instead of ad-hoc `bool IsLoading` / `bool HasError` fields.
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Loaded of 'T
    | Failed of string

/// Helpers over RemoteData.
module RemoteData =
    /// Convenience: fold a `RemoteData<'T>` into a concrete value.
    let fold loading loaded failed rd =
        match rd with
        | NotAsked -> loading
        | Loading -> loading
        | Loaded v -> loaded v
        | Failed err -> failed err