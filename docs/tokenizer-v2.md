# Tokenizer v2

What the tokenizer framework needs before it can serve as digihouse's identity backbone —
the mechanism by which story events reference entities (agents, goods, SKUs) by small
stable tokens instead of embedded object graphs. The concept is already right: **a token
is a foreign key, and a tokenizer is the lookup table that resolves it.** This doc is
about closing the gap between that concept and the current implementation in
`Tokenizer<TModel>`, `TokenizedModelPackRat<TModel>`, and `PiedPiper`.

The companion doc `v1-models.md` (in the digihouse repo) covers how digihouse consumes
all of this; the "tokens for identity, copied fields for history" rule lives there.

## The end state in one paragraph

A tokenizer is an **aggregate of a story**. The story that creates entities (the agent
story, the catalog story) assigns each new entity the next token from a canonical
allocator; every consumer replaying that story derives the identical token table with
zero coordination. Tokenizers key models by *identity* (an id selector), not by each
model class's hand-written `Equals`. They can be enumerated, they are safe to read while
a story listener appends, and redefining a token (an entity was updated) is an explicit,
consistent operation. Hand-authored binary token strings disappear.

## 1. Bug: `DefineToken` redefinition corrupts the reverse map

`DefineToken` doc says "Defines, or redefines, a token," but redefinition leaves the two
dictionaries inconsistent:

- Redefine token `T` from model `A` to model `B`: `_tokenToModelMap[T]` becomes `B`, but
  `_modelToTokenMap` still holds `A → T`. `GetToken(A)` happily returns a token that no
  longer means A.
- Re-tokenize model `A` from `T1` to `T2`: `_tokenToModelMap[T1]` still resolves to `A`.

Nothing redefines tokens today, so it's latent — but a story-fed tokenizer redefines on
every entity update (`AgentRenamed` replaces the model behind the agent's token). Fix:

- `DefineToken` **throws** if the token or the model is already defined (catches the
  hand-allocation collision class of bug too), and
- a separate explicit `RedefineToken(Code token, TModel model)` removes the stale
  entries from *both* maps before inserting.

## 2. Token allocation: the framework owns it

Every digihouse Magic class hand-authors binary strings (`"10001"`, `"10010"`, …) that
are really ordinals encoded by hand, guarded only by a comment telling humans not to
renumber. The failure mode is real: `ProductMagic` already declares tokens out of
numeric order, and a duplicate wouldn't throw — it would silently redefine (bug #1). Two
tokenizers even derive tokens from declaration order (`nextToken++`), so inserting an
entry mid-list silently breaks the wire.

Add allocation to `Tokenizer`:

```csharp
/// Assigns the next token in the canonical sequence to this model and returns it.
public Code AllocateNextToken(TModel model);

/// The number of tokens defined — also the next ordinal to be allocated.
public int Count { get; }
```

with **one canonical ordinal → Code encoding** shared by all tokenizers (the existing
efficient-whole-number style encoding is fine; what matters is that it's fixed forever
and documented as wire format). Determinism is the point: if the rule is "the *n*th
`AgentCreated` event gets token `encode(n)`," every consumer replaying the story assigns
identical tokens with no coordination. The created-event should still *record* the
assigned token explicitly — the allocator makes it impossible to get wrong, the recorded
token makes it auditable.

`DefineToken` stays public for pinning legacy tokens (the Magic classes' existing
hand-authored codes must keep decoding forever), but new entities never hand-pick.

## 3. Identity-keyed lookup: stop requiring `Equals` on every model

`_modelToTokenMap` is keyed by the model object, so `GetToken` works only if `TModel`
has value equality. That's why Good, Product, Sku, and RoomPlan each carry ~30 lines of
boilerplate `Equals`/`GetHashCode`/`==`/`!=` — and why Agent, which forgot, only works
because the Magic singletons are reference-identical. Decode an Agent from a story and
`GetToken(thatAgent)` silently fails.

The framework should own identity:

```csharp
public Tokenizer();                                        // legacy: model equality
public Tokenizer(IEqualityComparer<TModel> comparer);      // or:
public Tokenizer(Func<TModel, object> identitySelector);   // e.g. a => a.Id
```

With `new Tokenizer<Agent>(a => a.Id)`, all four hand-written equality implementations
in digihouse become deletable, and the "forgot to override Equals" bug class disappears.

Note the sharp edge this exposes deliberately: two entries sharing an id (digihouse's
`Dad1`/`Dad2` share a Guid) collide at `DefineToken` time and throw under rule #1 —
which is correct: same person, one token, and the collision becomes a decision instead
of an accident.

## 4. Enumeration

`Tokenizer` keeps both maps private and exposes no way to list what's defined, so every
consumer maintains a parallel `All` list *and* its own model→token dictionary —
triplicating state the tokenizer already holds. Expose:

```csharp
public int Count { get; }
public IEnumerable<TModel> Models { get; }
public IEnumerable<KeyValuePair<Code, TModel>> Tokens { get; }
```

Roughly half of every digihouse Magic class evaporates, and future store/catalog UIs
("list every product") read straight from the tokenizer.

## 5. Concurrency: hydrate, then freeze (or synchronize)

Dynamic usage — the whole point of story-fed tokenizers — means a story listener calling
`DefineToken`/`RedefineToken` while request threads encode and decode. Plain
`Dictionary` will corrupt or throw. Two supportable contracts; offer both:

- **Freeze-after-hydrate** (matches replay semantics): `Freeze()` makes reads lock-free
  forever and writes throw. Right for reference data that only changes at startup.
- **Synchronized** for live-updating tokenizers: internal lock (or swap-on-write
  immutable maps) so a listener can append while encoders read.

The default constructor should pick one documented behavior; silent non-thread-safety
is the only wrong answer.

## 6. Hydration ordering is a rule, not a hope

`TokenizedModelPackRat`'s remarks say the client "must take care" that a tokenizer has
a model before its token is encountered. In a story world this is a cross-story
dependency graph: the room story can't decode until the goods tokenizer is hydrated
from the catalog/goods stories, which need the agent tokenizer, and so on.

The framework can't know the app's graph, but it can (a) document the rule loudly, and
(b) fail well: `UnpackModel`'s error should name the tokenizer id, not just the token,
so a mid-replay failure says *which* story wasn't hydrated. Digihouse's ordering
(agents → catalog → goods → houses/rooms) lives in `v1-models.md`.

A future nicety — a "pending token" mode that parks unresolved tokens and completes
them when defined — is explicitly out of scope until a real consumer needs it.

## 7. Perf: `Code.GetHashCode` is ready to be fixed

Tokens are dictionary keys, so every token pack/unpack hashes a `Code` — and
`Code.GetHashCode` still hashes via `ToString()`, building a spaced binary string per
lookup. The TODO says the byte-array hash "doesn't work (since there can be extra
unused bytes)" — but commit `1c715a2` canonicalized the byte-array constructors to zero
trailing bits, so a bytes+length hash is now correct and allocation-free. Tokenizer-
heavy digihouse traffic makes this worth taking.

## 8. Small stuff

- Doc typos: "definied" (`IsModelTokenized`), `TryGetToken`'s `<returns>` says "The
  token." for a bool.
- `RegisterTokenizer` also registering a `TokenizedModelPackRat` under the same id is a
  good trick that deserves a sentence of XML doc — it's why a tokenizer id works as a
  `PackField` schema id at all, and nothing currently says so.
- Downstream naming convention (digihouse, but worth blessing here): constant-bag
  classes holding ids end in `Id` (`DigihouseTokenizerId`), so
  `[PackField(2, DigihouseTokenizerId.Sku)]` reads as "token reference" at a glance.

## Sequencing

1. ✅ Bug fix (#1) + enumeration (#4) + doc fixes (#8) — done July 2026: `DefineToken`
   throws on conflicts, `RedefineToken` keeps both maps consistent, `Count`/`Models`/
   `Tokens` exposed.
2. ✅ Identity-keyed lookup (#3) — done July 2026: comparer and identity-selector
   constructors; digihouse's equality boilerplate is now deletable.
3. Allocator (#2) — needs the canonical encoding decision; prerequisite for story-fed
   tokenizers and for retiring hand-authored tokens.
4. Concurrency contract (#5) — before the first live-updating tokenizer ships. (The
   #6 error-message half is done: `UnpackModel` failures now name the model type and
   point at hydration.)
5. ✅ `Code.GetHashCode` (#7) — done July 2026: bytes+length hash, allocation-free.

All of 1–4 are additive or fix-broken, so nothing here breaks existing wire format:
pinned tokens keep decoding forever, and the allocator only governs tokens that don't
exist yet.
