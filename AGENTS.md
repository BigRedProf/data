# Agent Instructions

This repository follows the BigRedProf development environment conventions. It
is the single source of shared instructions for agents and contributors;
`CLAUDE.md` imports this file via `@AGENTS.md`.

`BigRedProf.Data` is the base library of the BigRedProf stack — the sibling
`content`, `stories`, and `digihouse` repositories all consume its published
packages. Changes here ripple outward, so treat public contracts carefully. The
domain concepts (bit, code, schema, datum, pack rat, pied piper, trait, flex
datum, tokenizer, tape) are defined in `README.md`; design notes live under
`docs/`, starting with `docs/ontology-draft-datum.md`.

---

## Authoritative Coding Standards

All formatting, organization, naming, nullability, defensive programming, and
structural code-style rules are defined in `CODING_GUIDELINES.md`, which is the
authoritative source of truth. If anything here conflicts with it, follow
`CODING_GUIDELINES.md`.

---

## Standard Commands

This repository is driven by [Task](https://taskfile.dev). Task is the
orchestration layer and loads the layered environment (`.env.local` then `.env`)
on every invocation, so no shell setup is needed — commands work in a fresh
process for humans and agents alike.

```powershell
task build      # fast inner loop (restore once, then build)
task test       # unit tests, no rebuild
task verify     # everything required before merging — the success criterion
task clean
task doctor     # toolchain/version diagnostics
task pack       # build the NuGet packages locally
```

List everything with `task --list`.

`verify` is the canonical success criterion. It is fast by design (build +
unit tests).

---

## Pull Requests

An automated reviewer (Codex) comments on every pull request, usually within a
minute or two of it opening. **Read that review before calling a pull request
ready, and before merging.** It posts inline review comments rather than a
status check, so nothing blocks on it by default:

```bash
gh api repos/BigRedProf/data/pulls/<number>/comments --jq '.[] | "\(.path):\(.line)
\(.body)"'
```

This is not ceremony. Across the v1 ontology work it caught, on three separate
pull requests, defects that the tests could not: a wire format that quietly
became incompatible, an immutable type handing out its own backing array, and a
package that could not be built from a clean checkout. Twice the finding was in
a path local testing structurally could not reach, because the machine's state
was hiding it.

Address each finding or say why not, then resolve the thread. `main` requires
conversation resolution, so an unread review blocks the merge button rather
than being merged past.

Data specifics:

- The build **target** is `src/Data.sln`. Note the solution lives under `src/`,
  not at the repository root.
- Tests are real and substantial: `task test` runs `src/Core.Test`,
  `src/PackRatCompiler.Test`, and `src/Tape.Test`. They sit under `src/` rather
  than a top-level `tests/` directory, a known deviation from
  `REPO_CONVENTIONS.md`.
- There is **no container image** here — this repository ships libraries and a
  CLI, not a service, so there is no `image` or `publish` task.
- Three packages are published to **nuget.org** by CI, and only when a `v*` tag
  is pushed: `BigRedProf.Data.Core`, `BigRedProf.Data.PackRatCompiler` (the `prc`
  tool that sibling repos restore), and `BigRedProf.Data.Tape`. Merges to `main`
  build and test but publish nothing — a release is a tag, not a merge, because
  nuget.org versions are immutable and effectively permanent. The version itself
  comes from that tag via MinVer, so there is no version number to edit anywhere.
  `task pack` only builds the packages locally and deliberately cannot push.
- There is no `.config/dotnet-tools.json`, so `restore` does not run
  `dotnet tool restore`. Add both together if a local tool is ever introduced.

---

## How It Fits Together

- **`Taskfile.yml`** — the authoritative task graph. Simple verbs (restore,
  build, test, verify, clean) are defined directly here so the graph restores
  once, builds once, and tests without rebuilding.
- **`script/*.ps1`** — only genuinely complex, multi-step behavior (`doctor`).
  Task invokes these in their own process.
- **`.env`** (committed) / **`.env.local`** (gitignored, wins) — per-developer
  environment preferences such as configuration. The authoritative build
  **target is in `Taskfile.yml`**, not in `.env`, so everyone verifies the same
  projects.

Do not reintroduce a general-purpose PowerShell orchestration layer; Task owns
orchestration.

---

## Notes

The canonical/shared version of common PowerShell utilities lives in the
BigRedProf foundation repository:

```text
foundation/templates/dotnet/script/common.ps1
```

Each repository contains its own versioned copy under `script/common.ps1` so
repositories can evolve independently.
