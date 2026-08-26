namespace Community.Web.Client.State

/// The recommended single `RemoteData<'T>` type for async server-backed values.
///
/// Use this instead of ad-hoc `bool IsLoading` / `bool HasError` fields.
type RemoteData<'T> =
    | NotAsked
    | Loading
    | Loaded of 'T
    | Failed of string