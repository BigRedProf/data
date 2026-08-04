# BigRedProf.Data

The **BigRedProf.Data** library is a simple, flexible .NET library for defining and serializing models. 

## Development

This repository is driven by [Task](https://taskfile.dev). Install it once per
machine:

```powershell
choco install go-task
```

Then, from the repository root:

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

**bit** - a discrete binary value; uses the symbols **0** and **1**

**code** - a sequence of one or more bits used to represent something (think cryptographic messages, NOT computer instructions)

---

**model** - a software developer's representation of a domain object

---

**packing** - the act of turning a **model** into a **code** (think serialization)

**unpacking** - the act of turning a **code** into a **model** (think deserialization)

**pack rat** - the actor responsible **packing** and **unpacking** a specific **model**

**pied piper** - the actor who organizes all the **pack rats** (consider creating a singleton pied piper in your startup code if you're using dependency injection)

---

**flex model** - a flexible **model** composed of one or more models called traits; the advantage of the flex model is that clients don't need to understand its full schema to work with it--they can use the traits they understand and ignore the rest

**trait** - a model and its purpose (eg: "My name is Memorial Stadium." or "My location is Lincoln, Nebraska.")

---

**token** - a small **code** representing a larger model (think the heap address of a reference type or the foreign key of a database row)

**tokenized model** - a model that's **packed** as its **token** value rather than as its full value

**tokenizer** - a mapping of tokens to models and models to tokens; can be hard-coded if known at compile-time or dynamically loaded at runtime

## License

BigRedProf.Data is licensed under the MIT License. See LICENSE for more information.

## Contact

For questions, suggestions, or issues, please contact Professor at [BigRedProf@outlook.com](BigRedProf@outlook.com).
