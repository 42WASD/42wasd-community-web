---
phase: 03-step-by-step-implementation/phase-14-cross-feature-effects
---

**Phase 14 complete — cross-feature effects.** An action on the Tournaments
page now updates the canonical shared tournaments cache, and Home's "open
tournaments" stat reflects it immediately. The Tournaments feature never
mutates shared state directly — it emits its own local message, and the root
translates it into a `Shared` effect message, exactly as the reference
requires.

### Reference

```text
an action in one page affects shared state
a shared update is dispatched, not reached into
```

Verification: *"An action on one page updates the canonical shared entity
cache, and another page reflecting the same entity shows the updated value."*

The sign-in → Account-clear flow already demonstrated a cross-feature *session*
effect in earlier phases. Phase 14 adds a *data* cross-feature effect: toggling
a tournament's `registrationOpen` in the canonical cache, which Home's "open
tournaments" stat reads.

### Shared effect message

`App/App.fs` `Shared.Msg` gained `ToggleTournament of string`. `Shared.update`
handles it by flipping `registrationOpen` inside the canonical cache — it does
NOT re-fetch from the server, it mutates the single normalized map:

```fsharp
| ToggleTournament tournamentId ->
    let tournaments =
        match shared.tournaments with
        | Loaded m ->
            match m.TryFind tournamentId with
            | Some t ->
                let t' = { t with registrationOpen = not t.registrationOpen }
                Loaded (m.Add(tournamentId, t'))
            | None -> Loaded m
        | other -> other
    { shared with tournaments = tournaments }, Cmd.none
```

Home's `stats` reads the same cache, so the count updates automatically:

```fsharp
let openTournaments =
    match shared.tournaments with
    | Loaded m -> m.Values |> Seq.filter (fun t -> t.registrationOpen) |> Seq.length
    | _ -> 0
```

### Feature-owned page (emits the effect, does not reach in)

`Pages/Tournaments.fs` became feature-owned: it declares its own `Msg` and
takes a dispatch:

```fsharp
module Tournaments =
    type Msg =
        | ToggleRegistration of string

    let row (tournament: Tournament) (dispatch: Msg -> unit) =
        tr {
            td { tournament.name }
            td { tournament.prize }
            td { tournament.startsAt.ToString("yyyy-MM-dd") }
            td {
                if tournament.registrationOpen then
                    button {
                        attr.``class`` "button is-small is-danger"
                        on.click (fun _ -> dispatch (ToggleRegistration tournament.id))
                        "Close registration"
                    }
                else
                    button {
                        attr.``class`` "button is-small is-success"
                        on.click (fun _ -> dispatch (ToggleRegistration tournament.id))
                        "Reopen registration"
                    }
            }
        }
```

The view selects the canonical cache and renders a row per tournament. The
`wwwroot/main.html` Tournaments template gained an **Actions** column.

### Root orchestration (translates the effect)

The root `Message` gained `TournamentsMsg of Tournaments.Msg`. Its `update`
case maps every Tournaments message to a Shared effect — the page dispatches a
shared update by message, never by reaching into shared state:

```fsharp
| TournamentsMsg msg ->
    match msg with
    | Tournaments.ToggleRegistration tournamentId ->
        model, Cmd.ofMsg (SharedMsg (Shared.ToggleTournament tournamentId))
```

`Ui/Layout.fs` passes a dispatch to the Tournaments view:

```fsharp
| Tournaments -> Tournaments.view model.shared (fun msg -> dispatch (TournamentsMsg msg))
```

### MVU verification (browser console)

Clicking "Close registration" on CS2 Summer Cup:

```
New message:: TournamentsMsg (ToggleRegistration "tournament-1")
New message:: SharedMsg (ToggleTournament "tournament-1")
```

The trace proves the ownership boundary: the feature emits a local message, the
root turns it into a shared effect. No `GotTournaments` (no re-fetch) fires —
the cache is mutated in place.

### Browser verification

1. Tournaments page → CS2 Summer Cup shows "Close registration".
2. Click it → button flips to "Reopen registration" (canonical cache mutated).
3. Navigate to Home → "1 tournaments open" (was 2) — Home reflects the
   cross-feature update from the same canonical cache.
4. Navigate back to Tournaments → CS2 Summer Cup still shows "Reopen
   registration" (survives navigate-away-and-back; no re-fetch).

### Verification

```bash
dotnet build Community.Web.sln        # 0 warnings, 0 errors
# dev server http://localhost:5023
curl -I http://localhost:5023/tournaments # 200
```

`verify.sh` reports `VERIFY OK`.

### Files changed

```
src/Community.Web.Client/App/App.fs                 (+ Shared.ToggleTournament, + TournamentsMsg, update)
src/Community.Web.Client/Pages/Tournaments.fs       (feature-owned Msg + toggle row + view)
src/Community.Web.Client/Ui/Layout.fs               (+ Tournaments dispatch)
src/Community.Web.Client/wwwroot/main.html          (+ Actions column)
docs/implementation/progress.yaml                   (phase-14: done)
docs/implementation/index.md                       (regenerated)
docs/implementation/_runbook/phase-14-cross-feature-effects.md  (this file)
```