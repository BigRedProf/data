# Foundations — Draft A: *Datum*

> One of two parallel drafts written for [issue #33](https://github.com/BigRedProf/data/issues/33).
> Draft A and Draft B say the same things about the same world in two different vocabularies.
> Draft B is preserved as written; this draft has since been developed further.
> **Status.** The ontology was accepted and most of Part 4 has landed on `feature/v1-ontology`;
> see Part 4 for what shipped and what is still open. Parts 1 and 2 stand as written. Part 3's
> *Where the implementation diverges from the lecture* is kept in the present tense as the record
> of what the abstract account found — several of the files and line numbers it cites no longer
> exist, which is the point.

**Design goal.** `BigRedProf.Data` should encourage developers to think **data-first**: to design
the durable, portable record before designing the classes that happen to hold it in memory today.
The vocabulary is the cheapest lever we have on that, so the names below are chosen to reward
that habit and to make the opposite habit feel slightly wrong.

**Where this came from.** The library exists because human-readable formats stamp their labels on
every copy. `"ownerAgentId"` travels with every record, forever, to say something that never
varies. Separating the agreement from the data — holding the schema once, on both sides, and
writing only the answers — is both more elegant and dramatically smaller, and it costs nothing you
cannot recover with a tool that renders a code back into readable form. Forward and backward
compatibility came later as a second motivation, and the flex datum is the answer to it. Both
motivations survive intact in what follows, and §8 says when each one wins.

Part 1 is deliberately free of computers. If a concept in it cannot be explained without a runtime,
it does not belong in the foundation.

---

## Part 1 — The lecture

*Delivered without reference to computers, runtimes, programming languages, or this library.*

### 1. The bit

A **bit** is precisely one of two discrete states.

Nothing about a bit says which two states, or what they mean. Heads and tails. Present and
absent. Left and right. We write them **0** and **1** by convention, and that convention is all
they are.

### 2. The code

A **code** is a sequence of bits.

`01000001` is a code. So is `1`. So is a mile of Morse tape. A code has a length and an order,
and that exhausts what a code is. In particular, **a code does not mean anything**. Anyone who
tells you `01000001` is the letter *A* has smuggled in an agreement you have not made yet.

The sequence may be **empty**, and that is worth saying out loud because it is easy to rule out
by accident. The empty code is the code that says nothing. It has a length, zero, and no order to
speak of, and it is as much a code as any other — in the way that an empty shelf is still a shelf.

What makes it worth allowing is not tidiness. Some subjects have nothing to record. An event that
means *this happened*, and nothing further, is answered completely by naming which event it was;
there is nothing left for bits to carry. Forbid the empty code and such an event cannot be written
down at all without inventing a bit that means nothing, which is a worse outcome than admitting
that sometimes there is nothing to say.

### 3. The schema

A **schema** is that agreement: a correspondence between codes and the things those codes stand
for.

A schema is not a document, a file, or a type. It is a *relation*, and it is prior to any
particular recording of it. One schema says `01000001` stands for the letter *A*. A different
schema says the same code stands for the number 65, or for the sixty-fifth chair in a warehouse.
The code did not change. What changed is which agreement you brought to it.

Schemas are identified, because you must be able to say *which* agreement you mean.

### 4. The datum

Bring a code and a schema together and you have made something new.

> A **datum** is a code together with the schema under which that code is to be read.

This is the first thing in our account that means anything. A code is inert; a datum has a
definite interpretation. And the schema is **constitutive, not decorative** — the same code under
two schemas is two different data, not one datum with two labels. Take the schema away and you do
not have a datum with something missing; you have a code, which is a different kind of thing
entirely.

Many data, collectively, are **data**. This is what the word has always meant.

#### An unread datum is still a datum

A datum says how its code is *to be* read. It does not promise that you are able to read it.
Someone who has never heard of the schema still holds the datum entire: they can carry it, copy
it, count it, take a fingerprint of it, and hand it to someone who knows the agreement. What they
cannot do is interpret it, and that is the honest outcome.

So interpretation is something a reader **brings** to a datum, never something stored inside one.
A datum is its schema and its code, and nothing else — in particular, it does not contain the
subject, or any rendering of the subject. This is the same shape the trait value takes in §7, and
for the same reason.

### 5. What a datum is *about*

A datum is not merely meaningful. It is meaningful *about something*.

> A datum **represents** its subject. Equivalently: a datum **models** its subject.

Note that both verbs are unavoidably two-placed: nothing simply *is* a representation; a thing
represents some other thing. A datum represents the letter *A*. A datum represents a chair, a
room, a photograph, a sale, a person's name. The schema is what establishes the representation —
it is the schema that makes this code stand for *that*.

So there are three questions you can ask of any datum:

- What are its bits? — its **code**
- Under what agreement are they read? — its **schema**
- What does it represent? — its **subject**

### 6. The form

Most schemas do not read a code all at once. They read it in **parts**, one after another, each
part under a schema of its own.

> A **structured schema** reads its code as an ordered sequence of parts, and names, for each
> position in that sequence, the schema under which that part is read.

Picture a printed form. It has numbered blanks running down the page: blank 1, blank 2, blank 3.
Blank 2 takes a date; blank 3 takes an amount of money. A filled-in form carries the answers and
nothing else — the blanks are not labelled on the copy you receive. You know what blank 2 means
because you hold the blank form, not because the filled one told you.

That is the whole idea, and three consequences follow from it.

**Position is identity.** Nothing in a filled form marks where one answer ends and the next
begins except the agreement itself. A part is identified by *being second*, and by nothing else.

**The parts are anonymous.** "Blank 2" is not a question anyone can ask in general. You cannot
walk up to a chair and ask it for its blank 2. The meaning of a part lives entirely inside its own
form and has no existence outside it.

**Agreement must be total.** Because parts are located only by counting, a reader who
misunderstands any part misunderstands everything after it. There is no partial credit: either you
hold the same blank form as the writer, or you are reading noise with confidence.

And so the rules of change are strict, and they are strict for a reason a filing clerk would
recognise.

*A form's sequence of parts is fixed forever.* Insert a blank, remove one, reorder two, or change
what notation a blank takes, and every previously filled copy now reads wrong — not unreadable,
which would be survivable, but **wrong**, which is not. A changed form is a different form and
needs a different identity.

*A blank cannot be quietly retired.* It is tempting to think a blank could be abandoned while the
ones after it keep their numbers, the way a trait identifier can be retired and never reused. It
cannot, and the reason is the difference this section is about. A card's entries name themselves,
so removing one leaves the others exactly where they were. A form's blanks are found by counting,
so **a hole in the numbering is not a hole on the page**: strike out blank 2 and blank 3 simply
becomes the second thing anyone reads. The only way to retire a blank while keeping the form's
identity is to leave it physically present and agree that it is to be ignored — and that is a
different form's worth of agreement, so it may as well be a different form.

*Adding a blank at the end is not free either.* An old reader stops after the blanks it knows,
and unless something independently marks where the filled form ends, it has no way to tell that
more was written — and if forms are stored end to end, it will read the next form's first blank as
part of this one. Appending is safe only when a form's extent is marked from outside.

### 7. The trait

Some subjects are simple enough to represent whole. Most interesting ones are not. A chair has a
name, a colour, a location, an owner, a purchase price, a photograph — and no two people will ever
agree on the complete list.

So we represent such subjects piecewise.

> A **trait** is a named aspect a subject may have. A trait's identifier designates both the
> question being asked and the schema of the answer.

"What is your name?" is a trait. "Where are you located?" is a trait. The identifier is a *global*
agreement, not a local label — it is what lets two parties who have never met, and who share no
common design, nonetheless mean the same thing by the same question.

Three rules follow, and they are permanent.

**Permanence.** A trait's identifier is bound to its schema **forever**. To change the schema of
an answer is to ask a different question, and a different question needs a different identifier.
Trait identifiers are minted, never reused or recycled.

**Presence.** A subject either has an answer to a question or it does not. There is no empty
answer, and no answer meaning "none" — **absence is how you say no**. A trait is present or it is
absent, and nothing in between.

**Multiplicity.** A subject has **at most one answer per question**. If a chair may have three
photographs, then the question is not "what is your photograph" but "what are your photographs,"
and its answer's schema is one of lists. Multiplicity lives inside the answer, never in repeated
entries. This keeps every question total and unambiguous: asking a subject a question it
recognises yields exactly one answer or none.

> A **trait value** is a trait identifier together with a code answering that trait. The trait's
> schema is what makes that code a datum.

Note the shape of that, and compare it with §4. A trait is to its answer what a schema is to a
code: bring the two together and meaning appears. A trait is, in effect, a schema with a question
attached.

Defining a trait value this way rather than as "an identifier together with a datum" is not
fussiness. The schema is fixed by the identifier, so carrying a datum alongside would state it
twice and admit ill-formed pairs whose datum disagreed with the trait. More importantly, it would
make an **unrecognised** entry impossible to describe: you cannot form a datum without knowing the
schema, so a reader holding a card full of questions it does not recognise would be holding things
that were not trait values at all. As codes, they are perfectly ordinary trait values that simply
have not been interpreted — which is exactly the situation the flex datum exists to handle.

### 8. The flex datum

Now the useful part.

> A **flex datum** is a datum whose subject is represented by a set of independently identified
> traits.

Picture a physical card. Down its face are entries, each one a trait identifier followed by the
answer to that question. A reader picks up the card and works down it. For each entry the reader
either recognises the identifier — in which case it knows the question, knows the schema of the
answer, and can read it — or does not, in which case it skips that entry and continues.

The consequences are strong, and entirely independent of any machine:

- A writer may record traits that a reader does not understand.
- A reader can use every trait it recognises **without** understanding the rest.
- Unrecognised traits do not prevent interpretation of the recognised ones.
- A reader from 2028, one from 2030, and one from 2040 can all read the same card, each getting
  as much as its vocabulary allows.

That is a compatibility guarantee obtained from the structure of the thing itself, not from
discipline, versioning, or negotiation between the parties. Note that there is no computer
anywhere in that account, and that this is the point: the flex datum is a data-architecture idea
that a filing clerk in 1890 could have adopted.

#### The form and the card

Set §6 beside this and the two ways of composing a representation stand out clearly.

| | the form | the card |
|---|---|---|
| a part is found by | counting | its name |
| a part's identity is | its position, local to this form | a trait identifier, global to everyone |
| an unknown part | cannot be detected, let alone skipped | is visible, skippable, and copyable |
| agreement required | total | only about the parts you use |
| cost | nothing beyond the answers | an identifier carried with every entry |
| change | a new form is a new form | add questions freely |

Neither is better. The form is compact and demands that both parties hold the identical blank; the
card costs an identifier per entry and asks the parties to agree only about the questions they
both care about. A shipping label and a passport are forms. An index card in a library catalogue,
where later librarians kept adding new kinds of note, is a card.

The choice between them is the single most consequential one a designer makes about a
representation, and it is a choice about *how much agreement can be assumed to last*.

Which gives a rule plain enough to apply without deliberation:

> Use a **form** when you control every writer and every reader, when they change together, and
> when space matters. Use a **card** when you do not know who will read this, when writers and
> readers will change at different times, and when records must survive being exported, archived,
> and read back by strangers.

Note that the form is not the compromise and the card is not the upgrade. A form refuses to write
down what both parties already know, which is the whole reason to prefer it over a format that
repeats its own field names in every record. A card knowingly pays that cost back — an identifier
per entry, never a prose name — buying the ability to be read by someone who does not hold your
blank form. Neither is a lesser version of the other.

#### Knowledge is bundled at the trait

Notice what the reader gets when it recognises an identifier: the question *and* the notation of
the answer, together, as one indivisible piece of knowledge. This is deliberate, and it is worth
saying why the alternative is worse rather than merely more expensive.

Suppose the card also recorded, beside each entry, the *notation* of the answer — "this one is a
number." A reader that did not recognise the question could now decode `0.42` out of an entry it
still cannot interpret. But knowing a value is a number tells you nothing about whether it is a
degree of latitude, a price, or a person's mass in kilograms. The reader has converted honest
opacity into a number it is likely to misread.

So the card records **what is being asked**, never **how to read an answer you were not asked to
understand**. Meaning travels with the question, or it does not travel at all.

#### An unread entry is still an entry

A reader that does not recognise an entry still *holds* it: it can see the question was asked, and
it can see exactly where the answer begins and ends. Two things follow, and both matter.

It can **set the entry aside** and go on reading the rest of the card. Nothing about an
unrecognised entry impedes the entries around it.

And it can **copy the entry forward unchanged**. A reader that takes a card, revises one answer,
and writes out a new card must carry every entry it did not understand across to the new card,
exactly as written. Otherwise the act of updating a record destroys whatever a better-informed
writer had recorded on it — and a reader with an older vocabulary becomes a hazard to everyone
with a newer one. Copying forward is what makes the compatibility guarantee run in both
directions.

#### Every card has a reading order

Two cards bearing the same entries are the same record, and should be indistinguishable — so the
entries must have a **canonical order**, fixed by the trait identifiers themselves rather than by
the order in which someone happened to write them down. Without that, the same record has many
possible faces, and you can no longer say that two records are identical by comparing them, nor
summarise a record by a fingerprint taken of it.

#### The card is recognisable as a card

A reader who has never seen this particular card still recognises it *as a flex datum* — that much
is given by the datum's own schema. It needs nothing further. If a writer wishes to record what
kind of thing the subject is, that too is simply a question with an answer, and takes its place on
the card as an ordinary trait. There is no privileged entry, and no second way of saying what
something is.

#### A flex datum is a datum

It has a schema — the one that establishes the trait-composition convention — and under that
schema its code is read as a sequence of trait values. Nothing new is stacked on top of the
account. Layer three simply has a distinguished member.

### 9. Codes all the way down

One more observation, which the account handles without strain.

A datum's subject can be *anything*, including a code. There is a schema whose subject matter is
codes; a datum under that schema represents a code. Nothing recursive breaks and no sentence gets
awkward, because *represents* is a relation, and a relation may take any subject at all —
including the kinds of things we have been building with.

### The account in full

| term | definition |
|---|---|
| **bit** | one of two discrete states |
| **code** | a sequence of bits, possibly empty |
| **schema** | a correspondence between codes and what they stand for |
| **structured schema** | a schema that reads its code as an ordered sequence of parts, naming a schema for each position |
| **part** | one position in a structured schema's sequence; identified by position alone |
| **datum** | a code together with the schema under which it is read |
| **represents** | the relation a datum bears to its subject |
| **trait** | a named aspect: a question, and the schema of its answer |
| **trait value** | a trait identifier together with a code answering that trait; the trait's schema is what makes that code a datum |
| **flex datum** | a datum whose subject is represented by independently identified traits |

---

## Part 2 — digihouse Takeout

*The export documentation, written for someone who has just downloaded their archive.*

### What is in your archive

Your archive contains **your data** — everything digihouse knows about you, in a form that does
not require digihouse to read.

Each entry in the archive is a **datum**: a block of bits, plus the identifier of the schema that
says how those bits are to be read. There are 4,312 data in this archive. One represents your
dining room. One represents the chair in it. One represents the photograph you took of that chair
in 2027.

Nothing here is a digihouse-specific format in disguise. A datum is a datum, the schema
identifiers are public, and a program that has never heard of digihouse can read any datum whose
schema it knows.

### The manifest

`manifest.dat` is itself a datum. It represents the archive: when it was produced, which account
it belongs to, and the list of entries it contains.

### Reading an entry

To read an entry you need two things: its bits, and its schema identifier. The archive gives you
both. Look up the schema, apply it to the code, and you have recovered what that datum represents.

If you do not recognise a schema identifier you cannot read that datum — but you can still copy
it, count it, hash it, and hand it to something that can. An unreadable datum is opaque, not
corrupt.

### Traits, and reading things you do not fully understand

Most of your possessions are recorded as **flex data**: data assembled from independently
identified traits.

Your chair, for example, carries traits for its name, its room, its purchase date, its photograph,
and whatever else digihouse had learned about it by the time you exported. A tool that understands
names and rooms can list every possession by name and room *today*, without understanding purchase
dates or photographs, and without being updated when digihouse learns to record something new.

This is the property that makes the archive durable. A reader written five years from now against
a five-year-old vocabulary will still read the traits it knows out of data written today. So will
a reader written today against data exported five years from now.

### Entries you do not understand

You will encounter trait identifiers your tooling does not recognise. This is expected and is not
an error. You can see that they are there, and you can skip them exactly. You cannot read them —
and deliberately so, because a value you were not asked to understand is a value you would be
likely to misread. Skip them. The data around them remain readable, and the entry as a whole
remains valid.

---

## Part 3 — Mapping onto BigRedProf.Data

*Everything above holds without computers. This part is where the computers come back.*

### Two representations of one subject

The chair in your dining room is the subject. A `Chair` instance in memory is not the chair — it
is a representation of it, made of objects and fields. A datum is not the chair either — it is a
representation of it, made of bits.

> A **runtime model** and a **datum** are two representations of the same subject: one in objects,
> one in bits.

That is what packing is for, and it is the entire job of a pack rat:

```
        pack                          unpack
model ────────► code          code ────────► model
```

This is also why `model` survives as a .NET word without competing with `datum`. They name
representations in two different materials.

But there is a third branch, and missing it is what makes the consumer-directory question hard:

```
SUBJECT ──represented durably by──►  DATUM   (schemaId + code)
        └─represented at runtime──►  MODEL   (a C# object)

SCHEMA  ──────────declared by─────►  a type and its pack-field attributes
                                     (the pack rat is what connects the two branches)
```

A schema is not a representation of the subject at all. It is the agreement that makes one
possible, and in this library it is *authored* as a C# type declaration. So a class like
`GoodMinted` sits on two branches at once: its attributes declare the schema, its properties
declare the runtime model, and the generated pack rat is the bridge. That fusion is deliberate and
convenient. It is also why naming the place where such classes live is genuinely hard — see
*What consumers call their data*, below.

### One verb pair, not two

The library currently offers both `PackModel`/`UnpackModel` and `EncodeModel`/`DecodeModel`. These
are not two operations. `EncodeModel<M>(model, schemaId)` returns a standalone `Code`;
`PackModel<M>(writer, model, schemaId)` writes the same bits into a writer. Same operation,
different destination — an overload, not a concept.

**`pack` / `unpack` is the only verb pair.** It is the library's own idiom, and the pack rat and
the pied piper are built on it. `encode` is not a useful contrast, because *both* operations
produce a code; the word promises a distinction it cannot deliver.

```csharp
void PackModel<M>(CodeWriter writer, M model, AttributeFriendlyGuid schemaId);
Code PackModel<M>(M model, AttributeFriendlyGuid schemaId);   // was EncodeModel
```

Producing a self-describing schema-and-code pair needs **no verb of its own**. A `Datum` is itself
a thing with a schema, so you pack a datum with the same verb you pack a chair. The recursion does
the work the second verb pair was faking — which is a small piece of evidence that the ontology is
carrying its weight.

### Why `FlexDatum`, not `FlexModel`

The concept is the card in §8, and there is no runtime anywhere in it. The C# class is how you
build and read one, not what one is. Naming the class after the container inverts the design
principle this document exists to follow.

The consistency argument runs the same way: a C# `Datum` is not bits in a file either — it is an
object holding a schema identifier and a `Code` — and nobody finds that name dishonest. C# types
name *concepts*, not materials.

And the names teach different habits, which is the design goal at the top of this document:

- `FlexModel` invites *"a container I put things in while my program runs."*
- `FlexDatum` invites *"a durable record that will outlive my program and be read by strangers."*

The second is what the thing actually is.

### The v1 surface

| today | under this ontology | note |
|---|---|---|
| `ModelWithSchema` | **`Datum`** | `(SchemaId, Code)` — holds the code, not a decoded `object` |
| `ModelWithSchemaAndLength` | **deleted** | length is derivable from the code; framing moves to the pack rat |
| `FlexModel` | **`FlexDatum`** | immutable; see below |
| `TraitDefinition` | **`Trait`** | the question and its answer's schema — §7's trait |
| `Trait<M>` | **`TraitValue<M>`** | an answer; `.Model` becomes `.Value` |
| `EncodeModel` / `DecodeModel` | **deleted** | overloads of `PackModel` / `UnpackModel` |
| `PackRat<M>`, `PackModel`, `UnpackModel` | unchanged | they act on runtime models |
| `IPiedPiper`, pack rat, pied piper | unchanged | no ontological content |
| digihouse `Models/`, `*.Models` assemblies | **under review** | see *What consumers call their data*, below |

Migration cost worth naming up front: `FlexDatum` touches `Tape` (`TapeHeader`, `TapeLabel`,
`TapeHelper`) and the sibling `content`, `stories`, and `digihouse` repositories, which consume
the published packages.

### Decisions taken, and why

These look like omissions to a first-time reader, so they are recorded as decisions rather than
left to be rediscovered.

**The answer's schema does not go on the wire.** A trait identifier already implies its schema, so
transmitting the schema buys only the ability to decode values one has no vocabulary for — and
knowing a value is a number does not tell you whether it is a latitude, a price, or a mass. It
converts honest opacity into a number likely to be misread, and costs bits to do it. Trait
identifiers remain the unit of meaning; the registry remains the place the binding lives.

**There is no privileged "kind."** What something is, is a question like any other, and belongs on
the card as an ordinary trait. A structural kind would create a second typing mechanism competing
with traits and reintroduce the closed-world type the flex datum exists to escape. If a shared
vocabulary item is wanted for interoperability, it belongs in `CoreTrait` beside `Id` and `Name` —
as convention, never as structure.

**Versioning is not a separate mechanism, and is not planned.** It was long intended and never
built, and the ontology explains why it was never missed. A version resolves into one of two
things that already exist. Either a changed form is a *different form* — §6's rule — in which case
versioning means minting a new schema identifier rather than adding a field. Or one reader must
handle several generations at once, in which case what is wanted is a form whose **first part
selects which form the rest is**; the version is simply part 1. Nothing else is needed, and a
record that must tolerate open-ended change should be a flex datum rather than a versioned form.

That second case does expose a composition style §6 does not cover: *choice* rather than sequence
— "read one of these alternatives, discriminated by what comes first." The library has no explicit
support for it today. Whether it deserves to be first-class is an open question for #33, not a
decision taken here.

**Traits are single-valued.** See §7, *Multiplicity*. Repetition lives inside an answer's schema.

**A flex datum is a value, not a container.** Immutable, with structural equality, so that equal
traits imply equal codes imply equal data. Immutable *reference* type — a forty-trait flex datum
has no business being copied by value — and the same reasoning applies to `Code`.

### Where the implementation diverges from the lecture

Three of these are defects rather than improvements, and the first is the most serious thing in
this document.

**Unknown traits are not skipped — they throw.** `UnpackModel` eagerly decodes every trait
([FlexModelPackRat.cs:90](../src/Core/Internal/PackRats/FlexModelPackRat.cs:90)):

```csharp
Guid schema = PiedPiper.GetTraitDefinition(untypedTrait.TraitId).SchemaId;
object decodedModel = ((PiedPiper)PiedPiper).DecodeModel(encodedModel, schema);
```

`GetTraitDefinition` throws on an unknown trait
([PiedPiper.cs:246](../src/Core/PiedPiper.cs:246)). There is no guard and no catch, so **a flex
datum containing a single unrecognised trait fails to unpack in its entirety** — and no test in
the suite covers the case. The wire format supports skipping perfectly well; each entry's length
is right there. The reader simply does not use it. The property this whole design exists to
provide is, at present, aspirational.

The fix follows the corrected §7 definition: hold each trait value as `(traitId, code)` and
interpret **on demand**, when a caller asks for a trait by identifier. Unpacking then never needs
the registry at all, and `GetTrait<M>` is the only place a missing definition can be reported —
where it is actionable.

**`ModelWithSchema` holds an `object`, not a code — and so has the same defect.**
`UnpackModel` reads the schema identifier and then immediately decodes
([ModelWithSchemaPackRat.cs:31](../src/Core/Internal/PackRats/ModelWithSchemaPackRat.cs:31)),
which requires a registered pack rat for that schema. Its own documentation says the type is
"useful when you store multiple models together and won't otherwise know what schema each is" —
and it fails in exactly that case. It is worse than the flex datum here: with no length on the
wire, a reader cannot even step over the unreadable datum, so a single unknown schema poisons the
rest of the stream.

`ModelWithSchemaAndLength` exists *solely* so unknown models can be skipped, puts the length on
the wire, and then decodes eagerly anyway
([ModelWithSchemaAndLengthPackRat.cs:37](../src/Core/Internal/PackRats/ModelWithSchemaAndLengthPackRat.cs:37)).
It has everything it needs to keep its promise and does not use it.

Holding a `Code` fixes all of it, and settles two further things. **Length stops being a field and
becomes a fact:** today `Length` is whatever the caller supplied and is never checked against what
is actually written, so a wrong value silently produces an unskippable stream — a corruption
vector in the one type whose whole job is safe skipping. Derived from `Code.Length`, it cannot
disagree. And **equality becomes exact:** `ModelWithSchema.Equals` currently bottoms out in
`Model.Equals(other.Model)`, so it is correct only when whatever landed in that `object` happens
to implement value equality — the same trap `tokenizer-v2.md` §3 documents for its model map. Two
codes always compare structurally.

The cost is that a datum can no longer be assembled by hand without the pied piper, since
producing the code requires a pack rat. That is worth paying: it makes the encode step visible
instead of deferring it silently to pack time. A helper pair keeps it pleasant —
`piedPiper.PackDatum<M>(model, schemaId)` and `datum.Unpack<M>(piedPiper)`.

**Unknown traits are not preserved across a round trip.** The same eager decoding means there is
no path by which a consumer could unpack a flex datum, change one trait, repack it, and carry
forward the traits it did not understand. A service running an older vocabulary would silently
destroy data written by a newer one — forward compatibility inverted into data loss. Holding trait
values as codes makes preservation automatic; the corrected definition is what buys it.

**Trait order is accidental.** Traits live in a `Dictionary<Guid, UntypedTrait>` and are packed via
`.Values.ToList()` ([FlexModelPackRat.cs:23](../src/Core/Internal/PackRats/FlexModelPackRat.cs:23)).
Dictionary enumeration order is unspecified and shifts after removals and resizes, so the same
flex datum can pack to different codes. That breaks content addressing — and `ContentDigest`,
`SeriesHeadDigest`, and `SeriesParentDigest` are all core traits, so digests are load-bearing here
— along with equality by code, dedup, caching, and any future signing.
`PackModelAndUnpackModel_ShouldWorkForModelWithTraits` asserts an exact code for three traits and
passes only because `Dictionary` happens to preserve insertion order absent removals, which .NET
does not guarantee. **Fix: sort by trait identifier at pack time.**

**`Code` is mutable and structurally equal at the same time.** [Code.cs:161](../src/Core/Code.cs:161)
sets bits in place; [Code.cs:283](../src/Core/Code.cs:283) hashes the bytes; and
[Tokenizer.cs:22](../src/Core/Tokenizer.cs:22) keys a dictionary on `Code`. Mutating a code after
it has been used as a key makes the entry unreachable. Nothing does this today — it is correct
only by everyone's continued good behavior, the same latent class as the `DefineToken` bug in
`tokenizer-v2.md`.

**`FlexModel` is a mutable bag.** No `Equals`/`GetHashCode` at all, while `ModelWithSchema` has
them; `AddTrait`/`RemoveTrait` mutate in place; and the protected clone constructor at
[FlexModel.cs:63](../src/Core/FlexModel.cs:63) shares `UntypedTrait` instances with the original
rather than copying them.

**`TapeLabel` subclasses `FlexModel`**, using .NET inheritance to express "this flex datum is a
tape label" — an expectation the wire cannot carry. Containment, or a typed view over a
`FlexDatum`, says the same thing honestly.

**Trait values cannot be enumerated.** You can get trait identifiers and then fetch each one; there
is no `IEnumerable<TraitValue>`. This is the natural job for the renamed `TraitValue<M>`, whose
only role today is as an argument to `AddTrait`.

**Wire inefficiency.** Trait count uses `EfficientWholeNumber31`, but each trait's length uses a
fixed 32-bit `Int32` ([FlexModelPackRat.cs:46](../src/Core/Internal/PackRats/FlexModelPackRat.cs:46)).
Separately, a 128-bit identifier per trait means a six-trait chair spends 96 bytes on identifiers
before any content — which is the case for tokenizing **trait** identifiers, now that schema
identifiers have been ruled off the wire.

**Undocumented good property.** All trait identifiers and lengths are written together ahead of all
payloads, so a reader can learn what a flex datum *has* without decoding what it *says*. That is
a real guarantee and should be stated as one.

**Exception types.** `GetTrait` on a missing trait throws `ArgumentException`
([FlexModel.cs:105](../src/Core/FlexModel.cs:105)) where `KeyNotFoundException` is meant. By
contrast `DefineTrait` correctly throws on redefinition
([PiedPiper.cs:237](../src/Core/PiedPiper.cs:237)) — the lesson `tokenizer-v2.md` §1 learned about
`DefineToken`, already applied here.

### What consumers call their data

Earlier this document said a `Models/` directory in a consuming repository holds runtime models,
and that the name could stand. Looking at one closely says otherwise.

Here is a representative file, `digihouse/src/Models/Events/GoodMinted.cs`:

```csharp
[GeneratePackRat(DigihouseSchemaId.GoodMinted)]
public class GoodMinted
{
    [PackField(1, DigihouseSchemaId.Good, IsNullable = false)]
    public Good Good { get; set; } = default!;

    [PackField(2, CoreSchema.Guid)]
    public Guid OwnerAgentId { get; set; }
}
```

That class does two jobs. Its **instances** are runtime models. Its **type**, through those
attributes, is the authoritative declaration of a wire schema. And the two jobs have wildly
different lifespans: field ordinal `2` is wire format forever, while the property name
`OwnerAgentId` can be changed tomorrow by a rename refactor with nothing downstream noticing.

Directories and namespaces name types, not instances. So:

> When one artifact serves two roles, name it for the role that **cannot be refactored**.

By that rule `Models/` names the disposable half — the same slip as `ModelWithSchema`, one level
up and one repository over.

The strongest evidence is that the culture in those directories is already data-first and only the
label lags. `digihouse/src/Models/DigihouseTrait.cs` carries a wire-format changelog, recording a
retired trait identifier and why it was replaced. `digihouse/src/Models/Magic/AgentMagic.cs` opens
by warning that tokens are wire format, pinned explicitly, never renumbered or reused, "just like
schema GUIDs." Nobody writes those comments in a folder of runtime objects.

**The complication is that the directory is not homogeneous.** Of 150 files, 92 carry
`[GeneratePackRat]`, and several more are enums packed through integral schemas. But
`Marks/MarkRasterizer.cs` draws marks onto a texel buffer — its own remarks note the buffer is a
cache, never the marks themselves — and `Messaging/MessageService.cs` takes an `IPiedPiper` and
builds messages. Those are behavior. `View/FrustumExtents.cs` and `Physical/Rgba32.cs` are
runtime-only structs with no pack rat at all. Conversely `Unity/UMVector3.cs` does carry
`[GeneratePackRat]`, so it is data despite the folder it sits in.

A blanket rename would therefore be more accurate for roughly two thirds of the contents and less
accurate for the rest. The remedy is to move the handful of misfits out rather than let them hold
the name hostage.

These are two separate claims, and the first is far stronger than the second. That `Models/` names
the wrong half is, I think, settled. What to call it instead is not.

**The candidates are not data.** This is the trap worth naming explicitly, because it is the
mirror image of the mistake this whole document is about:

```text
runtime-first:            "a C# class representing something"  →  Models/
data-first overcorrection: "involved in durable data"           →  Data/
```

By this document's own §4, `GoodMinted.cs` is neither a datum nor a collection of data. No data
exists until something is packed. It is a **form declared** — a structured schema in the sense of
§6 — with a runtime model fused onto the same declaration for convenience. So the precise question
is not "is this data?" but:

> What do we call the place where fused schema-and-runtime-model declarations live, alongside the
> trait and token agreements that go with them?

Three candidates, judged against that question.

**`Schemas`** is the most precise for the bulk of the contents — `GoodMinted` really does declare a
schema, and §6 gives the word an exact meaning. It fails only at the edges: `DigihouseTrait` is
trait vocabulary and `AgentMagic` is a token table, and neither is a schema.

**`Data`** is the most aligned with the philosophy and with the parent library, and
`BigRedProf.Digihouse.Data` reads naturally as digihouse's contribution to the data layer. Its
weakness is the one above — the contents are agreements *about* data rather than data — plus a
practical collision: in ASP.NET, `Data/` conventionally means `DbContext` and repositories, and
digihouse is a web application.

**`Vocabulary`** is the one that covers all three durable categories without strain, because
schemas, traits, and tokens are all agreements about *what digihouse can say and understand*. It
also fits the fusion rather than fighting it: a vocabulary entry has always been both a thing you
can utter and a thing you can recognise, which is exactly the two roles `GoodMinted` plays. Its
weakness is that "vocabulary" ordinarily suggests words rather than structures, and that it says
nothing about permanence — though one does not un-mint a word either.

```text
BigRedProf.Digihouse
    Vocabulary/
        Events/  Goods/  Rooms/  Catalog/  Traits/  Tokenizers/
    Services/
    Rendering/
```

**Decided: `Data`.** The deciding argument is one none of the three descriptions above captures.
`Data` is the *library's own name*, so it makes the choice a convention rather than a one-off:
every application and library in the stack grows its own `*.Data`, and
`BigRedProf.Digihouse.Data` sits under `BigRedProf.Data` the way it reads. `Vocabulary` describes
the contents slightly better and `Schemas` describes the bulk of them more precisely, but neither
scales into a pattern the whole stack can follow, and a name that repeats itself across every
repository teaches the habit far more effectively than a name that is merely accurate once.

The objection about ASP.NET's `Data/` convention stands and is accepted: these directories hold
schema declarations and durable vocabulary, never a `DbContext` or a repository, and that
distinction should be stated wherever the convention is written down.

Whichever wins, the misfits move out first: `MarkRasterizer`, `MessageService`, and the
runtime-only view structs belong with behavior, not with agreements.

Three repositories are in scope: `digihouse/src/Models`, `stories/src/Models`, and
`content/src/Core/Models`. The pack rat compiler is **not** an obstacle — it discovers types by
walking namespaces rather than matching a `Models` convention, so generated pack rats follow
whatever namespace is chosen.

---

## Part 4 — Proposed work items

Filed as sub-issues of #33, numbered [#34](https://github.com/BigRedProf/data/issues/34) through
[#49](https://github.com/BigRedProf/data/issues/49).

**Landed** on `feature/v1-ontology`: unknown traits skippable and preservable (#34), canonical
trait ordering (#35), `Datum` (#36), `FlexDatum` (#37), `Trait`/`TraitValue` (#38), one verb pair
(#39), immutability and a builder (#40), `TapeLabel` as a view (#41), trait value enumeration
(#42), variable-length trait lengths (#43), retired field positions and the rules of schema change
(#46), documentation and exception types (#48).

Also landed: immutable `Code` with a `CodeBuilder` (#49).

**Still open**, and deliberately so. Tokenized trait identifiers (#44) wait on `tokenizer-v2.md`.
The consumers' directory rename (#45) is decided — `Data` — but lands in the `digihouse`,
`stories`, and `content` repositories rather than this one.

`CoreTrait.Kind` (#47) **was** deferred here, on the strength of the trait rules themselves: an
identifier is minted once and bound to its schema forever, so minting a core one with no consumer
spends a permanent identifier on a guess. That deferral has since been reversed, because the two
things it feared being guesses turned out to be derivable.

*What schema does the answer take?* A kind must be nameable by two parties who have never met, by
anyone, without a central registry — which is the same requirement that already made trait
identifiers and schema identifiers `Guid`s. It is not a name, because the library exists to keep
human-readable labels off every copy; and it is not a schema identifier, because "which agreement
reads this code" and "what is this a record of" are different facts, and conflating them would
smuggle back the structural type a flex datum exists to escape.

*One kind or several?* One. The multiplicity rule settles it: a subject has at most one answer per
question, and "what categories does this belong to" is a **different question** rather than a
second answer to this one. A consumer needing several classifications mints a trait for that
question; it does not overload this one.

What remains true is that nothing consumes it yet. But that cost falls the other way: without a
shared identifier each downstream repository invents its own, and the interoperability the item
exists for is gone before it is available. An unused trait identifier costs nothing at rest.

### Defects — fix regardless of the naming outcome

**0. Unknown traits must be skippable and preservable.**
Hold each trait value as `(traitId, code)` and interpret lazily, on `GetTrait<M>`. Unpacking stops
consulting the trait registry entirely.
*Done when:* a flex datum containing traits with no registered definition unpacks successfully;
its known traits are readable; asking for an unknown one reports a missing definition rather than
failing the whole datum; and unpack-modify-repack reproduces every unknown trait bit-for-bit.
Tests must cover all four, since none exist today.

**1. Canonical trait ordering in the flex datum wire format.**
Sort traits by identifier at pack time so a flex datum's code is a function of its content.
*Done when:* packing the same traits in any insertion order yields identical codes, with a test
that inserts in several orders and removes an entry in between.

**2. `Code` is mutable and used as a dictionary key.**
Make `Code` immutable — construction through `CodeWriter` or a builder, read-only indexer — or, if
that is too large for v1, document the hazard and stop keying dictionaries on it.
*Done when:* a code cannot be modified after construction, and `Tokenizer` is safe by
construction rather than by convention.

### Ontology and naming

**3. Introduce `Datum` as `(SchemaId, Code)`; retire `ModelWithSchema` and
`ModelWithSchemaAndLength`.**
The type holds a code, not a decoded `object`, and unpacking never consults the pack rat registry
— interpretation happens when a caller asks, via `datum.Unpack<M>(piedPiper)`. Length becomes a
framing option on the pack rat rather than a type or a caller-supplied field. Add
`piedPiper.PackDatum<M>(model, schemaId)` so building one stays convenient.
*Done when:* a datum whose schema has no registered pack rat unpacks, compares, and repacks
bit-for-bit; a framed stream can be walked past such a datum; and unpacking one with a known
schema still yields the model. Note the equality bug this also removes:
`ModelWithSchemaAndLength.Equals(object)` casts to `ModelWithSchema`
([ModelWithSchemaAndLength.cs:56](../src/Core/ModelWithSchemaAndLength.cs:56)), so it always
returns false.

This is item 0's defect at a different layer; the two should be designed together and probably
land together.

**4. Rename `FlexModel` to `FlexDatum`.**
Touches `Tape` and the sibling repositories; coordinate with their package upgrades.

**5. Rename `TraitDefinition` to `Trait`, `Trait<M>` to `TraitValue<M>`, and `.Model` to `.Value`.**
Then document §7's three rules — permanence, presence, multiplicity — on the types themselves,
replacing "a model with a specific purpose or intent."

**6. Collapse `EncodeModel`/`DecodeModel` into `PackModel`/`UnpackModel` overloads.**
Five call sites in `src/`, plus `TextTrail.cs:69`.

### Semantics

**7. Make `FlexDatum` immutable, with a builder and structural equality.**
Depends on item 1: canonical ordering is what makes equal traits imply equal codes.

**8. Convert `TapeLabel` from subclass to a typed view over a `FlexDatum`.**
Removes the protected clone constructor and its shared-`UntypedTrait` aliasing.

**9. Add trait-value enumeration to `FlexDatum`.**
Gives the renamed `TraitValue<M>` a real job and removes the get-ids-then-fetch-each dance.
Enumeration must include traits with no registered definition — they are trait values like any
other, holding a code awaiting a schema.

### Wire format

**10. Use `EfficientWholeNumber31` for per-trait lengths.**
A pure win: same expressiveness, fewer bits. Breaking wire change, so land it with items 1 and 4.

**11. Evaluate tokenized trait identifiers.**
128 bits per trait is the dominant cost in a small flex datum. Depends on the `tokenizer-v2.md`
work; explicitly *not* accompanied by putting schema identifiers on the wire.

### Vocabulary and documentation

**11a. Rename consumers' `Models` directories, namespaces, and assemblies to `Data`.**
Every application and library in the stack grows its own `*.Data`, matching the base library's own
name; see *What consumers call their data* for why that beat `Vocabulary` and `Schemas`.
Scope: `digihouse/src/Models`, `stories/src/Models`, `content/src/Core/Models`. Move the files
that are neither schema declarations nor durable vocabulary — `MarkRasterizer`, `MessageService`,
and the runtime-only view structs — into a behavioral project first, so the rename is honest
rather than merely approximate. Not a `BigRedProf.Data` change, but it belongs to this ontology
and should be decided alongside it.
*Done when:* every type in the renamed tree either declares a schema, defines wire vocabulary, or
maps tokens; and nothing that does one of those three lives anywhere else.

**11b. Write down the rules for evolving a structured schema.**
§6 states them abstractly — a form's sequence of parts is fixed forever, and appending is safe only
when the code's extent is marked from outside — but nothing in the library, the compiler, or the
`[PackField]` documentation says so. Today it is folklore.

The compiler enforces one rule — `ValidatePackRatFields` requires positions to be exactly `1..n`
— and that rule is **correct**, though for a reason worth stating. It was briefly relaxed here to
allow gaps, on the theory that a retired position should behave like a retired trait identifier:
spent forever, nothing taking its place. That was wrong. A gap in the declaration is not a gap on
the wire. Positions 1 and 3 generate two sequential parts, so the part declared 3 simply becomes
the second thing written, and every code already packed under that schema is misread. The
relaxation made the dangerous edit *look* sanctioned. It has been reverted, and the case is now
covered by a test asserting the compiler rejects a gap.

What the compiler still cannot catch is the same edit done the compliant way: delete a field,
renumber the rest, keep the schema identifier. Only documentation and discipline prevent that
today. A shape fingerprint — a hash over the ordered part schemas, declared alongside the schema
identifier and checked at compile time — would catch it, and is worth considering.
*Done when:* the rules appear in the pack rat compiler's documentation and on
`GeneratePackRatAttribute`/`PackFieldAttribute`, and the append-under-framing rule is stated with
its precondition rather than as general advice.

**12. Consider a shared `CoreTrait.Kind`.** *Done.*
Convention only, beside `Id` and `Name`. Never structural, never required. The answer is a single
`Guid`; see the reversal recorded above for why each half of that is derived rather than chosen.

**13. Document the index/payload split as a guarantee**, and fix `GetTrait` to throw
`KeyNotFoundException`.

---

## The recurring failure mode

Every defect and awkwardness in this document traces to a single slip: reasoning from the .NET
types outward instead of from the data inward.

| what went wrong | the runtime-first move |
|---|---|
| `ModelWithSchema`, `ModelWithSchemaAndLength` | named after the C# holder; a framing detail promoted to a type |
| the case against `FlexDatum` | read the class first, concluded "it is a dictionary, so not a datum" |
| two verb pairs | justified an existing API pair instead of deriving the operation |
| accidental trait order | the data structure chose the wire format |
| `TapeLabel : FlexModel` | inheritance expressing what the wire cannot carry |
| mutable `FlexModel`, mutable `Code` | container thinking rather than record thinking |
| "a trait value is an identifier and a datum" | mirrored what the decoded object holds |
| `ModelWithSchema` holding an `object` | the same, one layer down |
| consumers' `Models/` directories | a two-role artifact named for the role that can be refactored |
| unknown traits and unknown schemas throwing on unpack | an object wants its fields populated; a card reader never would |

That last row is the pattern at its sharpest. **Every construct in the library whose stated
purpose is "you may not understand this" decodes eagerly and throws:** `FlexModel`,
`ModelWithSchema`, and `ModelWithSchemaAndLength` — the last two carrying doc comments that
promise precisely the behavior they do not have, and the third putting a length on the wire for a
skip it never performs. Three independent types, one instinct, three times.

The converse held as well. The one correction that came from the abstract account — noticing that
a definition stated the schema twice — walked directly into the most serious defect in the
library. The ontology found the bug that the tests could not.

Which is what makes this worth writing down rather than trusting to vigilance: **runtime-first
mistakes pass tests.** Everything in that table compiles, and the suite is green. The failure
surfaces years later in an archive nobody can reach.

The check is cheap, but it has to be stated precisely, because the loose version proves too much.

> If a proposed **data concept** cannot be stated on a card with no computer in the room, it does
> not belong in the data ontology.
>
> Runtime constructs are *derived* from that ontology and may add whatever machinery the runtime
> requires — properties, setters, generics, reflection — but must never redefine the durable
> meaning.

`GoodMinted` needs a runtime. So does `PackRat<M>`, and so does the pack rat compiler. None of
that is impure; that is what a runtime is for. What went wrong in every row above is narrower and
worse: a runtime fact was allowed to *decide* something durable — what a concept is, what goes on
the wire, what order bits appear in, what a directory is called.

---

## Where this vocabulary is load-bearing

- The stack of *things* is `bit → code → datum`. All nouns; each made of the one before it.
- `represents` is the relation a datum bears to its subject. It is a verb, and it stays one.
- `model` keeps its ordinary meaning — a representation of something — and names the runtime
  representation, the other half of the pair that packing converts between.
- A flex datum is a member of layer three, not a fourth layer.
- Meaning travels with the question. A trait identifier is the unit of meaning, and nothing on the
  wire invites a reader to interpret what it was not asked to understand.
- Every concept in Part 1 is stated without computers, which is the test of whether it belongs in
  the foundation at all.
- The library's name becomes literal: `BigRedProf.Data` is a library of data.
