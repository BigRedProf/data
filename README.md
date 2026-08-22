# BigRedProf.Data

The **BigRedProf.Data** library is a simple, flexible .NET library for defining and serializing models. 

## Development

This repository is driven by [Task](https://taskfile.dev). Provision the
required .NET SDK, Task, and PowerShell 7 from the repository root:

```powershell
# Windows
powershell -ExecutionPolicy Bypass -File .\script\bootstrap\windows.ps1
```

```bash
# Ubuntu
bash ./script/bootstrap/ubuntu.sh
```

The scripts install only missing or incompatible tools, diagnose the resulting
environment, and run `task verify`. See
[script/bootstrap/README.md](script/bootstrap/README.md) for check-only and
non-interactive options.

For day-to-day development:

```powershell
task --list      # see available tasks
task verify      # build + unit tests — everything required before merging
task build       # fast inner loop
task doctor      # toolchain/version diagnostics
task pack        # build the NuGet packages locally
```

Task loads the layered environment (`.env.local` then `.env`) on every
invocation, so no shell setup is required — commands work in a fresh process for
humans and agents alike. Note the solution lives at `src/Data.sln`, not at the
repository root.

`BigRedProf.Data.Core`, `BigRedProf.Data.PackRatCompiler` and
`BigRedProf.Data.Tape` are published to GitHub Packages by CI on a push to
`main`. `task pack` builds them locally and deliberately cannot push. See
[script/README.md](script/README.md) for the (short) script layer.

## Glossary

The full account, and the reasoning behind these names, is in
[docs/ontology-draft-datum.md](docs/ontology-draft-datum.md).

**bit** - precisely one of two discrete states; uses the symbols **0** and **1**

**code** - a sequence of one or more bits (think cryptographic messages, NOT computer instructions); a code by itself does not mean anything

**schema** - the agreement that gives a code meaning: a correspondence between codes and the things those codes stand for

**datum** - a **code** together with the **schema** under which that code is to be read; many data, collectively, are **data**

**subject** - the thing a datum represents; a datum *represents*, or *models*, its subject

---

**model** - a representation of a subject at runtime, made of objects rather than bits; a runtime model and a **datum** are two representations of the same subject, and **packing** converts between them

---

**packing** - the act of turning a **model** into a **code** (think serialization)

**unpacking** - the act of turning a **code** into a **model** (think deserialization)

**pack rat** - the actor responsible for **packing** and **unpacking** a specific **model**

**pied piper** - the actor who organizes all the **pack rats** (consider creating a singleton pied piper in your startup code if you're using dependency injection)

---

**trait** - a named aspect a subject may have; a trait identifier designates both the question being asked and the schema of the answer (eg: "What is your name?", answered "Memorial Stadium")

A trait identifier is a *global* agreement, and three rules about it are permanent: it is bound to its schema **forever**; a trait is present or absent, with absence being how you say no; and a subject has **at most one** answer per question, so multiplicity lives inside the answer's schema.

**trait value** - a trait identifier together with a code answering that trait; the trait's schema is what makes that code a datum

**flex datum** - a **datum** whose subject is represented by independently identified **traits**; a consumer can use the traits it recognizes without understanding the rest, and unrecognized traits are skipped, preserved, and carried forward untouched

---

There are two ways to compose a representation, and neither is the lesser one.

A **structured schema** -- what `[GeneratePackRat]` and `[PackField]` declare -- is a *form*: numbered blanks in a fixed order, where a part is identified by its position and by nothing else. It refuses to write down what both parties already know, which is why it is compact. It asks in return that both parties hold the identical form.

A **flex datum** is a *card*: labeled entries, where a part is identified globally. It pays an identifier per entry to buy the ability to be read by someone who does not hold your form.

Use a form when you control every writer and reader and they change together. Use a flex datum when you do not know who will read this, when writers and readers will change at different times, and when records must survive being exported, archived, and read back by strangers.

---

**token** - a small **code** representing a larger model (think the heap address of a reference type or the foreign key of a database row)

**tokenized model** - a model that's **packed** as its **token** value rather than as its full value

**tokenizer** - a mapping of tokens to models and models to tokens; can be hard-coded if known at compile-time or dynamically loaded at runtime

## License

BigRedProf.Data is licensed under the MIT License. See LICENSE for more information.

## Contact

For questions, suggestions, or issues, please contact Professor at [BigRedProf@outlook.com](BigRedProf@outlook.com).
