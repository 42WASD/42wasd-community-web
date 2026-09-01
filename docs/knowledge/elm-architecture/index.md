# The Elm Architecture — Model, Update, View, and Talking to the Outside

> Source: *Elm in Action* (Richard Feldman) — ch 1 (Welcome to Elm),
> ch 2 (Your first Elm application), ch 3 (Compiler as assistant),
> ch 4 (Talking to servers), ch 5 (Talking to JavaScript).

Elm is a functional language compiled to JavaScript whose entire value
proposition is **reliability**: no runtime exceptions in practice, a compiler
that catches errors before users feel them, and one architecture — **The Elm
Architecture (TEA)** — for data flow in every application. Code never lies;
comments sometimes do — type annotations are documentation the compiler
guarantees.

```mermaid
mindmap
  root((The Elm Architecture))
    Language basics
      expressions only
      no truthiness
      if-expressions
      let-expressions
      lists & records & tuples (immutable)
      currying & partial application
      operators are prefix functions
    TEA core
      Model = single source of state
      view : Model -> Html Msg
      update : Msg -> Model -> (Model, Cmd Msg)
      messages describe what happened
      Browser.sandbox → element → document → application
    Compiler as assistant
      type annotations as docs
      type variables & aliases
      case-expressions
      custom types
        Maybe & Result
        Msg as custom type
        missing-patterns error
      commands for effects
      Random.generate
    Talking to servers
      managed effects (no side effects)
      Http.get & expectJson
      decoders validate JSON
        primitives & list & field
        pipeline: succeed + required
        keyValuePairs & intermediate types
        lazy for recursive JSON
      Status custom type
        Loading | Loaded | Errored
    Talking to JavaScript
      custom elements (node)
      ports
        out: port x : a -> Cmd msg
        in: port x : (a -> msg) -> Sub msg
      flags via init
      Html.Events.on + Decoder
      requestAnimationFrame timing
```

## Chapter 1 — The language

Elm replaces in-browser JavaScript (or complements it, via interop). Key
properties: functions are not objects (no fields, prototypes, or state — they
accept values and return values); collections are **always immutable**; the
compiler infers all types; friendly error messages; an ecosystem built on a
small set of primitives (expressions, immutable values, managed effects) all
verified by the compiler. Adoption tip: "planting the seed" — rewrite one
small part of an existing JS/TS codebase, low-risk and reversible, then grow.

Choose Elm for: feature-rich web apps that are (or will grow) large, features
maintained long-term, functionality mostly in-house code. Choose a familiar
stack for: time-crunched projects (unrealistic to learn a new language under
deadline), gluing off-the-shelf components,
throwaway prototypes. Learning loop: `elm repl` (read-eval-print loop) is the
playground for building intuition — every expression shows its type
(`"Ahoy, World!" : String`), and experimentation ("a tiger cub building
intuition for physics") is the intended way to learn the language.

### Expressions

**An expression is anything that evaluates to a single value** — literals
included. Elm has *no statements*: `if` is an if-*expression* (ternary-like,
requires an `else`, always yields a value); `else if` is just nesting an
if-expression in the else branch. No truthiness — conditions are exactly
`True`/`False`. `++` appends; `+` adds (separate operators, each "does one
thing well"). Strings are double-quoted; characters single-quoted. Names are
assigned with `let`/top-level definitions (like `const` — never reassigned in
a scope); camelCase, no snake_case. `let … in` adds locally scoped named
values (the whole thing is an expression — usable anywhere an expression
goes).

### Functions

- Defined as `name param1 param2 = body`; **no `return` keyword** — the body
  is one expression whose value is the result. Early `return`s refactor away
  into conditionals.
- Arguments separated by spaces, **no commas**: `pluralize "elf" "elves" 3`.
  (Commas mean tuples: `( foo, bar )` is a tuple; `( foo bar )` is a call.)
- **All Elm functions are curried** — partial application works everywhere:
  `String.padLeft 9` gives a `Char -> String -> String` function. Common idiom:
  replace `\photo -> viewThumbnail model.selectedUrl photo` with the partial
  application `viewThumbnail model.selectedUrl`; add new information to a
  function by adding an argument at the *front*.
- Anonymous functions: `\w h -> w * h`.
- **Operators are functions**: exactly two arguments, infix style; wrap in
  parens for prefix style (`(+) 3 4`); normal function calls bind tighter
  than any operator; arithmetic is left-associative; `==` is nonassociative
  (can't chain).
- Module functions are the only organization — `String.length "storm"` not
  `"storm".length`. Methods never; plain functions always. Modules are named
  collections (`module PhotoGroove exposing (main)`); import with `exposing`
  for unqualified names (prefer *qualified* style as the readable default —
  unqualified risks ambiguity and hides where things come from); `as` aliases
  modules (`import Json.Encode as Encode`).

### Collections

| | List | Record | Tuple |
| --- | --- | --- | --- |
| Size | variable | fixed fields | fixed |
| Iterate | yes | no | no |
| Mixed types | no (compiler error — catches JS's `[1,"0","+02"]` nonsense) | yes | yes |
| Access | first element / iteration only (linked list) | named fields by dot | positional (`Tuple.first`) |

- `List.filter`, `List.map` are higher-order functions; consistent element
  types make them predictable.
- Records: `{ name = "Li", cats = 2 }` (fields with `=`, lowercase names, no
  inheritance, no `__proto__`, no field listing). **Record updates** copy:
  `{ catLover | cats = 3 }` — original untouched (structure sharing makes it
  cheap). This is how model changes are expressed.
- Tuples: concise unnamed records, ≤3 elements; destructure in parameters:
  `multiply3d (x, y, z) = x * y * z`.
- Arrays (ch 3): no literal, created by `Array.fromList`; better than lists
  for *arbitrary positional access* (`Array.get`).

## Chapter 2 — Your first application: TEA

### Declarative rendering

Elm doesn't touch the DOM directly. The `Html` module builds a **virtual DOM
description**: `div [ class "content" ] [ h1 [] [ text "Photo Groove" ] ]` —
every element function takes exactly two arguments (attributes list, children
list); `node "tagname"` creates any element. The compiled program embeds the
**Elm Runtime** (event listeners, scheduling, state management); the JS entry
is `Elm.PhotoGroove.init({node: …})` rendering into a div, with the module's
top-level `main` value as the entry point (name not negotiable). Build with
`elm make src/PhotoGroove.elm --output app.js`; serve with `elm reactor` (dev)
or any static server; commas-first style in multiline literals makes missing
commas visually obvious.

### Model → view → Msg → update → model

- **Model** — the application's entire state as one value (a record):
  `initialModel = { photos = […], selectedUrl = "1.jpeg" }`. State lives
  *outside* the DOM (the old "store state in DOM classes" approach doesn't
  scale).
- **view : Model -> Html Msg** — a pure function from model to a DOM
  description. Helpers like `viewThumbnail` split the work; `List.map
  (viewThumbnail model.selectedUrl) model.photos` renders collections;
  `classList [ ("selected", selectedUrl == thumb.url) ]` conditionally styles.
- **Msg** — a value describing *what happened* ("the user clicked a photo").
- **update : Msg -> Model -> Model** — given a message and the current model,
  returns a new model. **Always returns a model**, even for unrecognized
  messages (return the old one).
- Wiring: `main = Browser.sandbox { init = initialModel, view = view,
  update = update }` — the sandbox program, good for pure UI (no effects).

The loop: event → Msg → `update` → new Model → `view` → diff → DOM patch
(virtual DOM: batched updates, state less likely to desync, replaying state
changes replays UI changes).

| Interaction | JavaScript approach | Elm approach |
| --- | --- | --- |
| Changing the DOM | alter DOM nodes directly | return `Html` from `view` |
| Reacting to input | attach a listener to an element | specify a `Msg` to send to `update` |
| Changing state | mutate an object in place | return a new model from `update` |

## Chapter 3 — Compiler as assistant

### Type annotations as documentation

Comments drift; annotations can't — the compiler checks the entire code base
against every claim. `urlPrefix : String`, `isEmpty : String -> Bool`,
`initialModel : Model`. Annotations on the model are the single most useful
orientation doc for new teammates.

- **Type variables** (lowercase, e.g. `elementType`) are placeholders:
  `Array.fromList : List elementType -> Array elementType`. Same variable =
  same type on both sides; different variables mean independence. Reserved
  constrained variables: `number` (Int or Float), `appendable` (String or
  List), `comparable` (Int/Float/Char/String/List/tuples of these).
- **Type aliases** name existing types (like constants name values):
  `type alias Photo = { url : String }`, `type alias Model = { photos : List
  Photo, selectedUrl : String }`. A record type alias also *generates a
  constructor function*: `Photo "1.jpeg" == { url = "1.jpeg" }` (argument
  order = field order — reorder fields as carefully as you'd reorder function
  arguments!).
- Multi-argument functions read left-to-right with `->`:
  `String.padLeft : Int -> Char -> String -> String`; each partial
  application strips one argument.
- `Html` carries a type variable for the messages it sends: `view : Model ->
  Html Msg`; likewise `Cmd Msg`, `Sub Msg`, `Program flags Model Msg`
  (`Program () Model Msg` = no flags; `()` is *unit*, the type whose only
  value is `()`).

### case-expressions and custom types

A **case-expression** is the multiway conditional (no fall-through, no
`break`); `\_ ->` is the default branch (use sparingly — see below). Branch
indentation must align.

A **custom type** (`type`) defines a brand-new type by listing its values:

```elm
type ThumbnailSize = Small | Medium | Large
```

`Small`/`Medium`/`Large` are *not* ints or strings in disguise — comparing a
`ThumbnailSize` to anything else is a compile error. Values without data are
constants; **variants can be functions carrying data** (`ClickedPhoto :
String -> Msg`).

- **Maybe** — the container of at most one element:
  `type Maybe value = Just value | Nothing`. `Array.get 2 photos` returns
  `Maybe Photo`, never `undefined` — you *can't forget* to handle absence
  (no defensive null checks exist or are needed). Destructure in
  case-expressions: `Just photo -> …` extracts and names the payload.
- **Result** — the success/failure container:
  `type Result errValue okValue = Err errValue | Ok okValue` (used heavily in
  ch 4).
- **Msg as a custom type** beats a record of
  `{description, data}`: the compiler catches typos at build time (a typo'd
  variant name is an error, not a silently ignored string), each variant
  holds only the data it needs, and adding variants is cheap.

**The missing-patterns error** is a gift: if `update`'s case-expression
doesn't cover a variant, the compiler refuses to build. Avoiding `\_ ->`
default branches maximizes this safeguard.

### Random numbers via commands

Elm functions are guaranteed: same arguments → same return value, and no side
effects. So randomness *cannot* be a plain function. A **command (`Cmd`)** is
a value *describing* an operation for the Elm Runtime to perform — running
the same command twice may differ. Flow: `update` returns
`( Model, Cmd Msg )`; `Random.generate GotSelectedIndex randomPhotoPicker`
creates a command that generates the random value and sends it back to
`update` wrapped in the given Msg-constructor. `update : Msg -> Model ->
(Model, Cmd Msg)` requires upgrading `Browser.sandbox` → **`Browser.element`**
(`init : flags -> (Model, Cmd Msg)`, plus a `subscriptions` field).

## Chapter 4 — Talking to servers

### Managed effects

An **effect** modifies external state; a function that performs one has a
*side effect*. Elm functions never do. All effects run in the Elm Runtime;
application code only *describes* them by returning values from `update`
(call `update` a hundred times and it just hands back a hundred tuples).
Consequences: pure data transformations throughout — which is exactly what
makes Elm code testable ([testing-practices](../testing-practices/index.md)).

### Modeling server data: the Status custom type

Once photos load from a server, the model has three states — encode them
exactly:

```elm
type Status
    = Loading
    | Loaded (List Photo) String   -- photos + selectedUrl
    | Errored String               -- error message
```

Data exists **only in the states where it's valid** — accessing
`selectedUrl` while `Loading` is a compile error, not a null check. Compiler
errors after the model change walk you through every affected site (`view`
branches on `model.status`; `selectUrl : String -> Status -> Status` is a
no-op except when `Loaded`). This is the runtime cousin of DMMF's "make
illegal states unrepresentable"
([functional-design-and-types](../functional-design-and-types/index.md)).

Handy patterns en route: `(firstPhoto :: otherPhotos)` matches non-empty
lists (a natural "non-empty list" encoding — no `NonEmptyList` type needed;
return one via `( elem, List elem )`); `Loaded [] _` must be handled
separately (missing-patterns error!); `(firstUrl :: _) as urls` binds both
the head and the whole list. Refactors: `<|` replaces a final argument's
parentheses (`div [ … ] <| case … of`); `|>` pipelines data through
transformations (purely stylistic — compiles to the nested calls).

### HTTP with commands

```elm
initialCmd : Cmd Msg
initialCmd =
    Http.get
        { url = "http://elm-in-action.com/photos/list.json"
        , expect = Http.expectJson GotPhotos (list photoDecoder)
        }
```

- `Http.get : { url : String, expect : Expect msg } -> Cmd msg`; `init` is
  the only other place besides `update` that can return commands — run the
  first request at startup: `init = \_ -> ( initialModel, initialCmd )`.
- `Http.expectString` yields `Result Http.Error String`;
  `Http.expectJson` takes a `Decoder val` and yields
  `Result Http.Error val` (decode failure → `Err BadBody`).
- `Http.Error` is a custom type — `BadUrl String | Timeout | NetworkError |
  BadStatus Int | BadBody String` — so error handling is a case-expression
  per failure mode. `Http.request` for deeper customization; `Http.post` +
  `Http.jsonBody` (+ `Json.Encode`) to send data.

### Decoders: validate and translate JSON

`Json.Decode.decodeString : Decoder val -> String -> Result Error val` —
decoding is *validation*: the wrong shape is an `Err`, never a crash.

- Primitives: `bool`, `int`, `float`, `string`, `null` (JSON has no
  `undefined`).
- Combinators: `list : Decoder a -> Decoder (List a)`;
  `field "email" string` (must be an object, must have the field, field must
  match); `map2`…`map8` to combine; `oneOf` for alternatives;
  `at ["detail","userSlidTo"] int` for nested paths;
  `keyValuePairs decoder` → `List (String, val)` from any object.
- **Pipeline decoding** (`NoRedInk/elm-json-decode-pipeline`):

  ```elm
  photoDecoder : Decoder Photo
  photoDecoder =
      succeed Photo                       -- the generated constructor
          |> required "url" string
          |> required "size" int
          |> optional "title" string "(untitled)"
  ```

  `succeed Photo` starts with `Decoder (String -> Int -> String -> Photo)`;
  each `required` peels one argument off the function, one field off the
  JSON. `optional` defaults on missing/null fields. A hand-written
  `buildPhoto` is unnecessary — the type-alias constructor *is* the builder.

- **Intermediate representations**: when JSON and model shapes differ (photo
  URL living in the *key* of the enclosing object), decode into a `JsonPhoto`
  holding what the JSON gives, then `Decode.map fromPairs` to convert
  (`keyValuePairs jsonPhotoDecoder |> Decode.map fromPairs`). The decoder's
  job is to decouple your model from whatever shape the outside world throws.
- **Recursive decoders** for recursive JSON (folders in folders): a decoder
  defined in terms of itself is a *cyclic definition* (expands forever) —
  fix with `Decode.lazy (\_ -> list folderDecoder)`, which defers expansion
  until runtime.
- **Accumulating while decoding**: a second decoder over the same JSON can
  fold every nested photo dict into one source of truth —
  `List.foldl Dict.union folderPhotos subfolderPhotos` (`foldl` = "reduce":
  `(element -> state -> state) -> state -> List element -> state`, an update
  function just like TEA's). Join the two decoders' outputs with
  `Decode.map2`.

Dictionaries: `Dict k v` — iterable like a list, key-unique like a record,
efficient lookup by key (`Dict.get` → `Maybe`). Lists vs dicts: list wins
for iteration and order; dict wins for "find by key" (a 1000-photo list
scan vs O(log n) lookup). Keys must be `comparable`. Related:
`Dict.union`, `Dict.keys`, `Dict.empty`, `Dict.fromList`; `Maybe.andThen`
flattens nested lookups (`Maybe.andThen photoByUrl model.selectedPhotoUrl`
replaces a nested case-expression; `andThen` is strictly more powerful than
`map` — it can *change the outcome* — but `map` is preferred when
sufficient).

## Chapter 5 — Talking to JavaScript

Two mechanisms: **custom elements** (rendering) and **ports** (data). Rule
of thumb: write as little JavaScript as possible — runtime exceptions will,
if anywhere, come from the JS side.

### Custom elements

Web Components: register a JS class
(`window.customElements.define("range-slider", RangeSlider)`), whose
`connectedCallback` builds/initializes the widget (works with React, jQuery,
anything DOM-based — warning: custom elements *can* throw runtime
exceptions). From Elm, use them exactly like built-ins:
`rangeSlider attributes children = node "range-slider" attributes children`.
Pass configuration via element *properties*:
`Attr.property "val" (Encode.int magnitude)` — `Json.Encode` builds JS
`Value`s (also used for ports and test fixtures). Custom *events* are
listened to with `Html.Events.on : String -> Decoder msg -> Attribute msg` —
the same JSON decoders from HTTP decode the event object
(`at ["detail","userSlidTo"] int |> Json.Decode.map toMsg |> on "slide"`),
with a `toMsg` parameter so each slider maps to its own `Msg` variant.

**Data-modeling decision framework**: three separate `Int` fields vs a list
of `{name, amount}` records. The list is more concise and easier to extend —
but the three-fields approach **rules out more bugs**: a typo'd variant
(`SlidRippl`) is a compile error, while a typo'd string
(`SlidFilter "Rippl"`) compiles and silently fails; renames are compiler-
guided vs hope-and-tests. *Ask "which approach rules out more bugs?"* —
in Elm, weigh this ahead of conciseness. (Also: record-typed arguments give
"named arguments," preventing swapped same-typed params.)

Worked example (the Photo Groove filter sliders, `Hue`/`Ripple`/`Noise`,
each an `Int` 0–11): three `Msg` variants `SlidHue`/`SlidRipple`/`SlidNoise`
chosen over one `SlidFilter String Int`; each `viewFilter toMsg name
magnitude` receives its own `Int -> Msg` constructor, and every slide branch
re-runs `applyFilters` so the JS filter library re-renders in real time.

### Ports

Calling JS could have side effects, so Elm talks to JavaScript exactly as it
talks to servers: **send via a command, receive via a message** (decoded if
needed). The module becomes a `port module`:

```elm
port setFilters : FilterOptions -> Cmd msg           -- Elm → JS
port activityChanges : (String -> msg) -> Sub msg    -- JS → Elm
```

- Outgoing port: one argument, returns `Cmd msg` (lowercase — *no* message
  comes back; `Cmd.none` has the same type). "Fire and forget." JS side:
  `app.ports.setFilters.subscribe(fn)`; the Elm record arrives as a JS
  object. Mutable JS values can't come in — Elm values are copied to
  immutable structures. Sending can't fail, so no error handling is lost.
- Incoming port: takes a `String -> msg` (or `(Value -> msg)` for decoded
  safety), returns `Sub msg`. A **subscription** translates events outside
  the program into messages to `update` (also used for `Browser.onResize`
  etc.). Wire it in `subscriptions : Model -> Sub Msg` — the model argument
  lets subscriptions change dynamically. JS side:
  `app.ports.activityChanges.send(activity)`.
- Elm `Cmd`s are always *asynchronous* effects — JS may run before data
  returns.

**Timing with the DOM**: the runtime batches view calls to the browser's
repaint — a `Cmd` from `GotPhotos` can run before the new `<canvas>` exists.
Fix on the JS side: wrap port work in `requestAnimationFrame`, which runs
just before the next repaint (i.e. after Elm's next DOM update). General
tip for "run after the next render."

### Flags

Initialization data available *before the first render* — no flicker:

```elm
init : Float -> ( Model, Cmd Msg )   -- flags typed by init
init flags = …
main : Program Float Model Msg
```

JS: `Elm.PhotoGroove.init({node: …, flags: Pasta.version})`. For production,
prefer `Program Value Model Msg` + a decoder (`Json.Decode.decodeValue`) so
malformed flags degrade gracefully instead of throwing.

## Cross-links

- `update` is a pure state machine over messages — the exact shape DMMF
  models with types and state machines: [functional-design-and-types](../functional-design-and-types/index.md),
  [workflows-and-error-handling](../workflows-and-error-handling/index.md).
- The event→update→view unidirectional loop reappears as Blazor component
  events and MVVM binding: [blazor-components](../blazor-components/index.md),
  [mvvm-patterns](../mvvm-patterns/index.md).
- Decoders are the inbound twin of DMMF's DTO→domain `toDomain` functions:
  [persistence-and-evolution](../persistence-and-evolution/index.md).
- Testing update/view/decoders: [testing-practices](../testing-practices/index.md);
  SPA structure (routing, page delegation, lazy): [elm-in-production](../elm-in-production/index.md).
