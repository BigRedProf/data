# Foundations — Draft B: *Model*

> One of two parallel drafts written for [issue #33](https://github.com/BigRedProf/data/issues/33).
> Draft A and Draft B say the same things about the same world in two different vocabularies.
> They share a skeleton, examples, and wherever possible sentences, so that the only variable is
> the naming. Nothing here is decided.
>
> **This draft is preserved as originally written.** Draft A has since been developed further —
> it gained a data-first design goal, the runtime-model/datum distinction, a single verb pair, and
> a v1 mapping table. Read it for the current thinking; read this one for the fair comparison.

---

## Part 1 — The lecture

*Delivered without reference to computers, runtimes, programming languages, or this library.*

### 1. The bit

A **bit** is precisely one of two discrete states.

Nothing about a bit says which two states, or what they mean. Heads and tails. Present and
absent. Left and right. We write them **0** and **1** by convention, and that convention is all
they are.

### 2. The code

A **code** is a sequence of one or more bits.

`01000001` is a code. So is `1`. So is a mile of Morse tape. A code has a length and an order,
and that exhausts what a code is. In particular, **a code does not mean anything**. Anyone who
tells you `01000001` is the letter *A* has smuggled in an agreement you have not made yet.

### 3. The schema

A **schema** is that agreement: a correspondence between codes and the things those codes stand
for.

A schema is not a document, a file, or a type. It is a *relation*, and it is prior to any
particular recording of it. One schema says `01000001` stands for the letter *A*. A different
schema says the same code stands for the number 65, or for the sixty-fifth chair in a warehouse.
The code did not change. What changed is which agreement you brought to it.

Schemas are identified, because you must be able to say *which* agreement you mean.

### 4. The model

Bring a code and a schema together and you have made something new.

> A **model** is a code together with the schema under which that code is to be read.

This is the first thing in our account that means anything. A code is inert; a model stands for
something definite. And the schema is **constitutive, not decorative** — the same code under two
schemas is two different models, not one model with two labels. Take the schema away and you do
not have a model with something missing; you have a code, which is a different kind of thing
entirely.

The word carries its ordinary weight. A model is not merely an interpreted code; it is a
*representation*, in the same sense as a model of a building or a model of the atmosphere. Saying
that layer three is a model says why the schema matters: the schema is what makes these bits
stand for something beyond themselves.

### 5. What a model is *of*

Every model is a model **of** something. We call that thing its **subject**.

> A model **represents** its subject.

A model represents the letter *A*. A model represents a chair, a room, a photograph, a sale, a
person's name. The schema is what establishes the representation — it is the schema that makes
this code stand for *that*.

So there are three questions you can ask of any model:

- What are its bits? — its **code**
- Under what agreement are they read? — its **schema**
- What does it represent? — its **subject**

### 6. The trait

Some subjects are simple enough to model whole. Most interesting ones are not. A chair has a
name, a colour, a location, an owner, a purchase price, a photograph — and no two people will ever
agree on the complete list.

So we model such subjects piecewise.

> A **trait** is a named aspect a subject may have. A trait's identifier designates both the
> question being asked and the schema of the answer.

"What is your name?" is a trait. "Where are you located?" is a trait. The identifier is a *global*
agreement, not a local label — it is what lets two parties who have never met, and who share no
common design, nonetheless mean the same thing by the same question.

Two rules follow, and they are permanent:

1. A trait's identifier is bound to its schema **forever**. To change the schema of an answer is
   to ask a different question, and a different question needs a different identifier.
2. A trait identifier is minted, never reused. There is no recycling.

A **trait value** is a trait identifier together with a model: the question, and the answer given
under the schema that question fixed.

### 7. The flex model

Now the useful part.

> A **flex model** is a model whose subject is represented by a set of independently identified
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
discipline, versioning, or negotiation between the parties.

Note that a flex model **is a model**. It has a schema — the schema that establishes the
trait-composition convention — and under that schema its code is read as a sequence of trait
values. Nothing new is stacked on top of the account. Layer three simply has a distinguished
member, and the hierarchy explains itself:

```
model
├── ordinary model
└── flex model
```

### 8. Codes all the way down

One more observation, which the account handles without strain.

A model's subject can be *anything*, including a code. There is a schema whose subject matter is
codes; a model under that schema represents a code. So a code may be modelled, just as a chair or
a sale may be — and, since a flex model is itself a subject like any other, a model may represent
a model. Recursive representation is legitimate and useful, not a defect in the account.

### The account in full

| term | definition |
|---|---|
| **bit** | one of two discrete states |
| **code** | a sequence of one or more bits |
| **schema** | a correspondence between codes and what they stand for |
| **model** | a code together with the schema under which it is read |
| **subject** | the thing a model represents |
| **trait** | a named aspect: a question, and the schema of its answer |
| **flex model** | a model whose subject is represented by independently identified traits |

---

## Part 2 — digihouse Takeout

*The export documentation, written for someone who has just downloaded their archive.*

### What is in your archive

Your archive contains **models of your things** — everything digihouse knows about you, in a form
that does not require digihouse to read.

Each entry in the archive is a **model**: a block of bits, plus the identifier of the schema that
says how those bits are to be read. There are 4,312 models in this archive. One represents your
dining room. One represents the chair in it. One represents the photograph you took of that chair
in 2027.

Nothing here is a digihouse-specific format in disguise. A model is a model, the schema
identifiers are public, and a program that has never heard of digihouse can read any model whose
schema it knows.

### The manifest

`manifest.dat` is itself a model. It represents the archive: when it was produced, which account
it belongs to, and the list of entries it contains.

### Reading an entry

To read an entry you need two things: its bits, and its schema identifier. The archive gives you
both. Look up the schema, apply it to the code, and you have recovered what that model represents.

If you do not recognise a schema identifier you cannot read that model — but you can still copy
it, count it, hash it, and hand it to something that can. An unreadable model is opaque, not
corrupt.

### Traits, and reading things you do not fully understand

Most of your possessions are recorded as **flex models**: models assembled from independently
identified traits.

Your chair, for example, carries traits for its name, its room, its purchase date, its photograph,
and whatever else digihouse had learned about it by the time you exported. A tool that understands
names and rooms can list every possession by name and room *today*, without understanding purchase
dates or photographs, and without being updated when digihouse learns to record something new.

This is the property that makes the archive durable. A reader written five years from now against
a five-year-old vocabulary will still read the traits it knows out of models written today. So
will a reader written today against models exported five years from now.

### Entries you do not understand

You will encounter trait identifiers and schema identifiers your tooling does not recognise. This
is expected and is not an error. Skip them. The models around them remain readable, and the entry
as a whole remains valid.

---

## Where this vocabulary is load-bearing

- The stack of *things* is `bit → code → model`. All nouns; each made of the one before it.
- `subject` is what a model represents. It is not a layer; it is whatever the world contains.
- A flex model is a member of layer three, not a fourth layer — and the existing name is already
  the right one, derived from the layer above it with no coinage required.
- The hierarchy is self-explaining: a flex model is a kind of model, and the names say so.
