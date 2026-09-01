---
name: batch-stories
description: Implement several data GitHub issues behind one integration branch, so Russell signs off once on a coherent group rather than once per issue. Use when asked to batch, group, or run several issues together.
---

# Batch Stories

This skill has a runtime twin under the other agent discovery tree; keep substantive instructions synchronized.

`docs/batched-work.md` is the reasoning and the record — why this workflow exists, and what the first run
of it taught. This skill is the operating summary. When they disagree, the document is right and this
file needs updating.

Implement several issues behind one **integration branch**. Russell is the director and retains merge
authority over `main`; what changes is how many separate occasions that costs him.

## When to choose it

Use this for several small-to-medium independent issues where Russell is the bottleneck rather than the
difficulty. It has no second agent and no signed handoff: the review is Codex plus the CI build check,
and the only other gate is what the agent runs locally. Say so when proposing it.

Digihouse also has a signed two-agent baton (`lead-story` / `develop-story`) for a single story that
wants a genuine second reviewer. That has not been brought over here.

## Inputs

- Issues: required, the GitHub issue numbers Russell names.
- Topic: optional short name for the integration branch; derive one if not given.

## The shape

1. Russell names the issues.
2. Open `integration/<milestone>-<topic>` from `main`, and a **draft** PR from it into `main` so the
   batch is visible while it accumulates. GitHub will not open a pull request with no diff, so this goes
   up once the first change lands rather than at the start.
3. For each issue:
   1. a branch off the *current tip* of the integration branch;
   2. implement, `task verify`, push, open a PR **into the integration branch**;
   3. Codex reviews it — request one with a `@codex review` comment if none arrives within a few
      minutes, because the automatic trigger is unreliable;
   4. address every finding or reply saying why not; reply on the thread and resolve it;
   5. **wait for the `build` check to pass.** It runs on these branches, but no ruleset covers them, so
      nothing stops a merge while the check is pending or red. The waiting is the agent's;
   6. merge, then run `task verify` on the integration branch itself.
4. Mark the integration PR ready.
5. Russell reviews and merges to `main`.

Step 3.6's second half matters: each branch was verified against the integration branch as it stood when
the branch was cut, which is not how it stands now. Two changes can each be correct and still not compose.

## Rules

**Merge order is not negotiable.** The integration branch merges to `main` only when every issue branch
is merged or explicitly dropped, and never while one of its own PRs is open. Merging an integration
branch out from under an open PR orphans that PR's commits silently — this happened here: #54 targeted
`feature/netstandard21`, that branch merged to `main` twenty-two seconds first, `v0.9.0-rc.1` shipped
without the work, and it had to be re-landed as #56. Nothing warned; both merges succeeded.

**Cut each branch from the current tip** of the integration branch, never from `main` or a stale local
copy, and merge promptly.

**Sync before merging, then verify again.** A branch cut before an earlier merge must take the
integration tip and be verified on the result. Git's rename detection usually carries edits across a
moved file correctly, which is a thing to confirm rather than assume — it came up on the first run, when
one branch edited a test file at a path another branch had moved.

**A blocked issue does not block the batch.** Park it: its branch stays open, the specific question goes
to Russell, the rest proceeds. Whether something is parkable is the agent's call.

**Not every issue produces a PR.** An issue asking whether something is true is answered by evidence,
and answering it closes it.

## What the branch protection does and does not cover

**The ruleset applies to `main` alone.** There it requires the `build` check, a pull request, and
resolved conversations. Pull requests *into* the integration branch have none of that: no required
resolution, nothing enforcing that a review was read. The gate is real only at the last step, where the
whole batch faces it at once. Between here and there the discipline is the agent's.

**CI does travel.** `.github/workflows/dotnet.yml` runs `task verify` in Debug and Release on every pull
request into `main` or an `integration/**` branch, so `gh pr checks` is worth reading. Unlike Digihouse
there is no platform-gated step that silently skips, so a green check here means the whole solution
built and the whole suite ran. That makes the check *run*; step 3.5 is what makes it a gate.

**An empty findings list means nothing on its own.** Follow the review protocol in `AGENTS.md` — ask
`/reviews` whether Codex submitted a review at all *before* asking `/comments` what it found. With
required resolution absent on these branches, nothing else catches the confusion.

## Where to stop and ask Russell

- anything that changes a **wire format** — schema identifiers, field ordinals, trait identifiers, token
  values, or an enum whose numeric value is written to a trait. Codes already written cannot be
  renegotiated;
- anything that changes **public API in a way consumers feel**, since this is the base of the stack and
  every change ripples into `content`, `stories`, and `digihouse`;
- a **substantive disagreement with Codex**, as opposed to being plainly wrong, which is settled by
  fixing it;
- anything a person must test by hand;
- **a release.** A release here is a `v*` tag, not a merge, and nuget.org versions are permanent. It
  never belongs inside a batch.
