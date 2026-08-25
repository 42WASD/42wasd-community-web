# Understand the architecture

Part I is the conceptual foundation of the **42WASD Community Web** design. It
defines *why* the architecture is shaped the way it is, before any code is
written.

This part is `tracked: false` — it is narrative/conceptual and does not appear
on the Implementation progress page.

## Reading order

- **The one-sentence idea** — the entire architecture in a single sentence.
- **Why tempting architectures fail** — the pitfalls that motivate the design.
- **The six-layer mental model** — App, Routing, Shared, Page, Effects, View.
- **The state ownership model** — who owns each kind of state.
- **Message organization** — keeping the root message small.
- **Page and feature organization** — route is not always feature.
- **Recommended repository structure** — feature-oriented layout.
- **Keeping the Ui folder small** — don't recreate folder-by-type.
- **Developer vs gaming community** — same architecture, different vocabulary.
- **Design language** — visual theme is independent from MVU architecture.

## Core idea

Build one **Bolero `ProgramComponent` / Elmish program** whose root model stays
small. Keep cross-page data in a persistent `Shared.Model`, temporary state
close to the page that owns it, use Bolero routing and `PageModel<'T>` for
route state, `Cmd` for effects, and pure view functions for ordinary UI.

The rule that governs everything:

```text
route state != shared domain state
page-local state != persistent application state
visual component != independent MVU program
effect != direct mutation
framework shell != application architecture
```