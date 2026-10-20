# Tutorial and Distillation Rules

**Status:** binding for `docs/knowledge/` pages and for any book → notes
distillation in this repo. Evidence-graded: every rule below carries a
confidence marker, because treating a practitioner heuristic and a
replicated experiment as equally true is how style guides become folklore.

**Evidence grades used in this file:**

| Grade | Meaning |
| --- | --- |
| **A** | Multiple controlled experiments / meta-analysis; effect sizes reported |
| **B** | Consistent empirical support, fewer or narrower studies |
| **C** | Widely used practitioner method, plausible mechanism, thin direct evidence |
| **D** | Popular but the evidence is mixed or absent — use with caution |

---

## Part 0 — The one-paragraph theory

People have **two separate channels** for processing (visual and verbal),
each with **limited capacity**, and learning only happens when the learner
**actively selects, organises, and integrates** the material
(Mayer's cognitive theory of multimedia learning, **Grade A**). Every rule
below is one of three moves:

1. **Reduce extraneous load** — remove anything that isn't teaching.
2. **Manage essential load** — chunk it, and pre-teach the vocabulary.
3. **Support generative processing** — make the learner connect it to
   something they already know.

If a rule doesn't do one of those three things, it isn't in this file.

---

# Part A — Tutorial format for a layman

## A1. Segment: learner-paced chunks, one idea each — **Grade A**

Mayer's *segmenting principle*: people learn more deeply when a message is
presented in **learner-paced segments** rather than as a continuous unit.
Supported in **10 of 10** experiments with large effect sizes.

- **Rule:** one chunk = one idea. A chunk should be readable in under
  ~90 seconds. If a section has two "therefores", split it.
- **Rule:** put the chunk boundary where the *question changes*, not where
  the word count says.
- **Violation symptom:** a reader finishes a section and can't say what it
  was about in one sentence.
- **Check:** can you write a 6-word heading for this chunk? If not, it's
  two chunks.

## A2. Pre-train: define the vocabulary before using it — **Grade A**

Mayer's *pre-training principle*: learning improves when the **names and
characteristics of key concepts are taught before** the main explanation.
Median effect size **d = 1.39** (large) across comparisons.

- **Rule:** the first time a technical term appears, define it in **one
  plain sentence** — not a parenthetical, not a footnote.
- **Rule:** never use a term in a sentence before the sentence that
  defines it.
- **Rule:** a definition answers "what is it *like*?" before "what is it
  formally?" — analogy first, precision second.
- **Violation symptom:** the reader can follow the grammar but not the
  meaning.
- **Check:** scan for every noun that a non-programmer wouldn't know. Each
  one must have a definition *at or before* first use.

## A3. Coherence: delete everything that isn't teaching — **Grade A**

Mayer's *coherence principle*: people learn more deeply when extraneous
material is **excluded**. Supported in **23 of 23** experiments, median
**d = 0.86**.

- **Rule:** no decorative anecdotes, no "fun facts", no history unless the
  history *is* the explanation.
- **Rule:** no restating the same idea in three registers ("in other
  words…", "put simply…", "essentially…"). Say it once, well.
- **Rule:** a second example earns its place only if it tests a *different*
  edge of the concept. Two examples of the same point is redundancy (A4).
- **Violation symptom:** the page is long but the reader can't name what
  they learned.
- **Check:** delete a paragraph; if nothing is lost, it was extraneous.

## A4. Redundancy: don't say the same thing twice — **Grade A**

*Redundancy principle*: duplicated content (same message in two forms
simultaneously) **hurts**. 16 of 16 experiments, median **d = 0.86**.

- **Rule:** prose **or** code **or** diagram — pick one carrier per point.
  A diagram plus a paragraph that narrates the diagram is redundancy.
- **Rule:** no comment in code that repeats what the code says.
  (`// increment count` above `count <- count + 1`.)
- **Rule:** the Revise flowchart is the *only* sanctioned repeat — and it
  is a different task (recall check, not first explanation), so it sits at
  the end, never inline.
- **Check:** would a reader who read only the code miss anything the prose
  adds? If no, the prose is redundant.

## A5. Signalling: mark what matters — **Grade A**

*Signalling principle*: cues that highlight organisation improve learning.
24 of 28 experiments, median **d = 0.41** (moderate but consistent).

- **Rule:** every chunk gets a heading that **states the claim**, not the
  topic. "Make the phase a choice" beats "Phase modeling".
- **Rule:** one bolded phrase per paragraph **maximum** — bold is a
  highlighter, not decoration. Bolding a whole sentence destroys the signal.
- **Rule:** a one-line takeaway at the end of a chunk, only when the chunk
  is longer than ~5 sentences.
- **Violation symptom:** everything looks equally important, so the reader
  treats nothing as important.
- **Check:** cover the page and read only the headings — do they tell the
  story? They should.

## A6. Spatial contiguity: keep the explanation next to the thing — **Grade A**

*Spatial contiguity principle*: related words and pictures should be
**near each other**. 22 of 22 experiments, median **d = 1.10**.

- **Rule:** the explanation of a code block sits **immediately** above or
  below it — never a section away, never "see above".
- **Rule:** in code, the `❌` and its `✅` are adjacent, not separated by
  prose.
- **Rule:** the diagram is not a substitute for proximity — don't put a
  diagram at the top and expect it to explain a section at the bottom.
- **Check:** can a reader understand each code block without scrolling?

## A7. Concreteness fading: concrete → general, then name the abstraction — **Grade B**

*Concreteness fading* (Fyfe, McNeil, Son & Goldstone, 2014; 600+ citations):
start with a **concrete instance**, move to a **pictorial/structured**
representation, then to the **abstract rule**. This sequence outperforms
starting abstract, and outperforms staying concrete.

- **Rule:** order is always: **story → worked example → named rule**.
  Never definition → example.
- **Rule:** the concrete instance should be *real and specific* — a named
  type, a real value, an actual bug.
- **Rule:** the abstract statement comes **after** the reader has already
  understood it; its job is to *name* what they know, not to introduce it.
- **Violation symptom:** the page opens with a definition and the reader
  has nothing to attach it to.
- **Check:** does the first sentence of the page contain a noun the reader
  can picture?

## A8. Worked examples, not exercises — **Grade A**

*Worked-example effect* (Sweller, cognitive load theory — described in the
literature as the most widely studied of the cognitive-load effects):
novices learn **more** from studying fully worked solutions than from
solving equivalent problems. (Boundary condition: the advantage fades and
can reverse for experts — see A12.)

- **Rule:** every concept is demonstrated **fully worked**, end to end.
- **Rule:** no exercises, no "now you try", no call-to-action. The page's
  job is to teach, not to assign.
- **Rule:** show the *failure* before the fix. The mistake is the worked
  example's first half.
- **Check:** could the reader reproduce the solution from the page alone,
  without inventing a step?

## A9. Plain language: short sentences, common words, active voice — **Grade B**

Plain-language standards (ISO 24495-1:2023; plainlanguage.gov; NN/g) agree
on the mechanics. Oppenheimer (2006, *Applied Cognitive Psychology*) found
that needless complex vocabulary **reduces** judged competence and
comprehension — "no one has ever complained that a text was too easy to
understand" (NN/g).

- **Rule:** one idea per sentence. If a sentence has three commas, split it.
- **Rule:** prefer the common word (`use` not `utilise`, `start` not
  `commence`) unless the technical term is the concept being taught.
- **Rule:** active voice, present tense: "the compiler rejects it", not
  "it will have been rejected by the compiler".
- **Rule:** expand every acronym at first use; then use the short form.
- **Rule:** target roughly a **6th–8th grade reading level** for
  explanatory prose. Technical *terms* stay — they get defined (A2), not
  removed.
- **Violation symptom:** you have to re-read a sentence to parse it, even
  though you know the subject.

## A10. Conversational, second person — **Grade B**

*Personalization principle* (Mayer): conversational style ("you", "we")
improves learning versus formal style.

- **Rule:** address the reader as "you"; use "we" when walking through a
  derivation together.
- **Rule:** no third-person institutional voice ("it should be noted
  that…").
- **Rule:** this is not slang and not "dumbing down" — it is removing
  distance, not removing precision.

## A11. Match support to prior knowledge — **Grade A (boundary condition)**

The most consistent boundary condition across all the multimedia
principles: **experts can handle presentations that would overload
novices.** A principle "failing" for an expert audience is not evidence
against it.

- **Rule:** write for **one declared audience** at the top of the page
  (here: the repo owner, competent in F# but new to the topic).
- **Rule:** don't hedge by explaining for both audiences in the same
  paragraph — that's redundancy (A4) and it dilutes signalling (A5).
- **Rule:** when a page must serve two levels, split it, don't interleave.

## A12. Retrieval beats re-reading — **Grade B**

Testing effect / retrieval practice: **recalling** material produces
better retention than **re-reading** it. Note: the popular **SQ3R**
method as a whole has **weak and mixed empirical support** (The Learning
Scientists); what *is* supported is the core **read → recall → review**
cycle, and only after training.

- **Rule:** end the page with a **Revise** section (the flowchart) that
  prompts recall — but never require the reader to produce answers.
- **Rule:** don't claim a summary "teaches"; summaries **support**
  retention. The teaching happens in the worked examples.
- **Do not** build the page around SQ3R steps — the evidence doesn't
  support the full method. Use the supported core: read, recall, review.

---

# Part B — Distilling books and large text

**Goal:** reduce volume so the material can be understood quickly —
**without** destroying the reasoning. This is the failure mode of most
summaries: they keep the *conclusions* and delete the *causal chain*, so
the result is a list of true statements you cannot use or remember.

## B1. Read in passes, not once — **Grade B**

Adler's *How to Read a Book* (1940/1972) distinguishes **inspectional**
reading (structure, fast) from **analytical** reading (full understanding,
slow), and prescribes inspectional first. Practitioner consensus, aligned
with the segmenting evidence (A1).

- **Pass 0 — inspectional (10–15 min):** title, headings, intro, conclusion,
  figures, and the first sentence of every paragraph. Output: **a
  one-paragraph statement of what the book is about** and a list of its
  parts. Do not read linearly yet.
- **Pass 1 — structural map:** turn headings into questions. This becomes
  the skeleton of the notes (and later, of the page).
- **Pass 2 — analytical read:** read fully, marking only material that
  answers a question from Pass 1.
- **Pass 3 — distil:** apply the macrorules (B2) to the marks.

## B2. The five macrorules — **Grade A**

Brown & Day (1983), *Macrorules for summarizing texts*, is the canonical
empirical account. Their key finding matters more than the list: **novices
use only copy-and-delete; experts use superordination and invention.**
Copy-delete produces exactly the "scattered nuggets" failure this repo
already suffered. So:

| # | Rule | What you do | Example |
| --- | --- | --- | --- |
| 1 | **Deletion of trivia** | Drop detail that carries no argument | Remove tool version numbers, page references |
| 2 | **Deletion of redundancy** | Drop the second and third telling | Three paragraphs restating "types prevent bugs" → one |
| 3 | **Superordination** | Replace a list of specifics with its general term | "`map`, `filter`, `fold`, `collect`…" → "the standard collection vocabulary" |
| 4 | **Selection** | Keep the topic sentence that already states the idea | Keep "everything is a trade-off" |
| 5 | **Invention** | **Write** the topic sentence the author never wrote | Author shows 3 examples of coupling; you write "coupling is the cost of a boundary" |

- **Rule:** rules 1–2 are **mandatory**; rules 3–5 are what separate a real
  distillation from a highlight dump. If your notes contain no invented
  sentence, you have only copy-deleted.
- **Rule:** every kept item must be traceable to *where* it came from
  (chapter/section), so it can be re-checked.

## B3. The compression budget — **Grade C (practitioner, mechanism plausible)**

Over-compression is a real failure: Kintsch's work on macroprocesses shows
summaries built by **selection + deletion alone lose coherence** — the
causal links between ideas are exactly what gets deleted first.

- **Target ratio: 20–35% of the source** for a topic page. Not 5%.
  (Forte's progressive summarization deliberately goes much lower —
  to ~5% executive summaries — but only for notes that *earn* it through
  repeated use; **Grade C**, practitioner method, not peer-reviewed.)
- **Rule:** compress **examples harder** than explanations. One example
  per concept is enough; explanations keep their causal steps.
- **Rule:** if a section's *reasoning* is what you're cutting, stop —
  cut a different section instead.
- **Rule:** when two chapters make the same point, keep the better
  explanation and cite the other (this is B2 rule 2 applied across
  chapters).

## B4. Never delete these — **Grade B**

Four things carry the understanding; cutting them produces a summary that
reads well and teaches nothing:

1. **The causal chain** — why A leads to B. If you keep the conclusion,
   keep the step that made it necessary.
2. **The failure case** — what goes wrong without the technique. This is
   the anchor for the whole concept (A8).
3. **The boundary conditions** — when the rule stops applying. A rule
   without its boundary is misinformation.
4. **The trade-off** — what the technique costs. Especially in
   architecture material, where "it depends" is the actual content.

- **Rule:** if a claim has no stated cost or limit in your notes, you have
  deleted the part that makes it usable.

## B5. Structure the output as questions, not topics — **Grade B**

Text-structure research (Bogaerds-Hazenberg et al., 2021 meta-analysis)
finds **structure-based summarization improves comprehension**, especially
when the summarizer learns explicit rules for identifying structure.

- **Rule:** the note's headings are the *questions the source answers*
  ("How do I stop illegal states?"), not the source's own topic labels
  ("Choice Types").
- **Rule:** one note per **claim**, not per chapter. Chapters are the
  source's structure; claims are the reader's.
- **Rule:** keep a **source map** — a table of claim → book/chapter — so
  the note is verifiable and extensible.

## B6. Distillation pipeline (repo-specific)

```
knowledge/md/*.md  (full source, git-ignored)
      │  Pass 0: inspectional — headings, intro, conclusion
      ▼
notes/<book>/outline.md        ← structure map, ~1% of source
      │  Pass 1–2: analytical read, mark answers to questions
      ▼
notes/<book>/<topic>.md        ← macrorules applied, 20–35% of relevant chapters
      │  Pass 3: reorganise by question, invent topic sentences
      ▼
docs/knowledge/<topic>/index.md ← tutorial (Part A), example-dense, ≤10-line blocks
```

Use `build_book_index.py search "<term>"` to locate canonical passages and
quote them **verbatim** rather than paraphrasing from memory — paraphrase
drift is how a distillation becomes fiction.

---

# Part C — The page skeleton (derived from A1–A12)

This is the only sanctioned shape for a `docs/knowledge/` page. Each step
cites the rule it satisfies.

1. **Source header** — where the material comes from. *(B2: traceability)*
2. **Story opening** — a real, specific instance and the pain it invites.
   *(A7 concrete-first, A8 show failure first)*
3. **Numbered chunks** — each one idea, heading states the claim.
   *(A1 segmenting, A5 signalling)*
   - short point, plain language *(A9)*
   - `❌` the mistake, in real code *(A8)*
   - `✅` the correct form, worked, ≤10 lines *(A8, A3 coherence)*
   - every term defined at first use *(A2)*
   - the why in one prose sentence, never a comment wall *(A4, A3)*
4. **Revise** — the flowchart, byte-identical, at the end. *(A12 recall,
   A5 signalling, A4 the one sanctioned repeat)*
5. **Cross-links** — where the idea recurs. *(B5 question-structure)*

## Pre-flight checklist

Before publishing any knowledge page, verify:

- [ ] **A1** Every chunk is one idea, < ~90s read, 6-word heading possible
- [ ] **A2** Every technical term is defined at or before first use
- [ ] **A3** Nothing decorative; deleting any paragraph loses something
- [ ] **A4** No point is told twice; no comment restates its code
- [ ] **A5** Headings state claims; ≤1 bold phrase per paragraph
- [ ] **A6** Every code block's explanation is adjacent to it
- [ ] **A7** Opens concrete, ends abstract — never the reverse
- [ ] **A8** Every concept fully worked; zero exercises
- [ ] **A9** Sentences short; active voice; acronyms expanded
- [ ] **A10** Second person; no institutional voice
- [ ] **A11** One declared audience
- [ ] **A12** Revise section at the end; nothing assigned
- [ ] **B4** Causal chain, failure case, boundary condition, and cost all present

---

# Part D — What the evidence does *not* support

Stated explicitly, because an honest ruleset is more useful than a
confident one:

| Claim | Status |
| --- | --- |
| "SQ3R is a proven study method" | **Grade D** — weak, mixed evidence; only the read-recall-review core has support, and only after training |
| "Learning styles (visual/auditory/kinesthetic) should drive format" | **Grade D** — repeatedly failed to replicate; do not tailor pages to a "style" |
| "Progressive summarization is research-validated" | **Grade C** — practitioner method with a plausible mechanism (elaborative processing), not a tested protocol |
| "Shorter is always better" | **False** — over-compression destroys coherence (B3); the target is 20–35%, not minimal |
| "Experts benefit from the same supports as novices" | **False** — the worked-example and segmenting advantages shrink or reverse with expertise (A11) |
| "Multimedia always beats text" | **False** — coherence and redundancy (A3, A4) mean added media can *hurt* |

---

## References

- Mayer, R. E. (2020). *Multimedia Learning* (3rd ed.). Cambridge University
  Press. — segmenting, pre-training, coherence, signalling, redundancy,
  spatial/temporal contiguity, personalization.
- Sweller, J., & Cooper, G. A. (1985). The use of worked examples as a
  substitute for problem solving in learning algebra. *Cognition and
  Instruction*, 2(1), 59–89. — worked-example effect.
- Brown, A. L., & Day, J. D. (1983). Macrorules for summarizing texts: The
  development of expertise. *Journal of Verbal Learning and Verbal
  Behavior*, 22, 1–14. — the five summarization rules.
- Fyfe, E. R., McNeil, N. M., Son, J. Y., & Goldstone, R. L. (2014).
  Concreteness fading in mathematics and science instruction: A systematic
  review. *Educational Psychology Review*, 26(1), 9–25.
- Bogaerds-Hazenberg, S. T. M., et al. (2021). A meta-analysis on the
  effects of text structure instruction on reading comprehension.
- Adler, M. J., & Van Doren, C. (1972). *How to Read a Book* (rev. ed.).
- Oppenheimer, D. M. (2006). Consequences of erudite vernacular utilized
  irrespective of necessity. *Applied Cognitive Psychology*, 20, 139–156.
- ISO 24495-1:2023 *Plain language — Part 1: Governing principles and
  guidelines*; plainlanguage.gov; Nielsen Norman Group, "Plain Language Is
  for Everyone, Even Experts".
- Forte, T. (2017). Progressive Summarization. Forte Labs. *(practitioner;
  Grade C)*
- Diátaxis (Procida) — tutorial vs how-to vs reference vs explanation.
  *(framework, not an empirical claim)*
