# Batched work

> **Status.** Agreed 2026-08-23, not yet exercised. Written down before the first batch so the
> rules exist independently of whoever remembers them. Revise it after the first run rather than
> defending it.

## The problem it solves

Every change needs a human to approve it into `main`, and that human has one throughput and a
day job. One issue per pull request per approval turns the director into a queue: work sits
finished and unmerged, waiting not on a judgement but on availability.

The batch inverts which of those is scarce. Several issues are implemented, reviewed, and merged
into a shared branch without the director present. What reaches them is one pull request
containing a coherent group of finished, reviewed work — a decision worth their attention rather
than six that are not.

**This does not lower the bar for `main`.** Every rule that guards it still guards it. What
changes is how many separate occasions the guarding costs.

## The shape

1. The director names the issues in the batch.
2. The agent opens an **integration branch** from `main` — the branch that collects the batch.
   It also opens the integration pull request into `main` immediately, as a **draft**, so the
   batch is visible while it accumulates rather than arriving fully formed.
3. For each issue in the batch:
   1. a feature branch off the *current tip* of the integration branch;
   2. implement, test, `task verify`, push, open a pull request **into the integration branch**;
   3. Codex reviews it;
   4. address every finding or reply saying why not — reply on the thread and resolve it;
   5. merge into the integration branch, then run `task verify` on the integration branch itself.
4. The agent marks the integration pull request ready for review.
5. The director reviews, and merges to `main`.

Step 3.5's second half is the part that is easy to skip and worth not skipping: each feature
branch was verified against the integration branch as it stood when the branch was cut, which is
not necessarily how it stands now. Two changes can each be correct and still not compose.

### Terminology

**Integration branch** is the standard name for a branch that collects several feature branches
before any of them reaches the trunk. Not a release branch, which is cut *from* the trunk to
stabilise a version, and not a topic branch, which is what each individual issue gets.

## The rules

These exist because each one has already gone wrong, or is one small lapse away from it.

**Merge order is not negotiable.** The integration branch merges to `main` only when every
feature branch in the batch is merged or explicitly dropped. Never merge the integration branch
while one of its own pull requests is still open.

> This has happened. PR #54 targeted `feature/netstandard21`, and that branch merged to `main`
> twenty-two seconds before #54 did. The commits went into a branch that no longer led anywhere,
> `v0.9.0-rc.1` shipped without them, and the work had to be re-landed as #56. Nothing warned;
> both merges succeeded.

**Cut each feature branch from the current tip of the integration branch**, never from `main`
and never from a stale local copy. Merge each one promptly. The longer a branch is out, the more
of the batch it has not seen.

**The branch protection rules do not travel.** The ruleset applies to the default branch, so
pull requests *into* the integration branch have no required build check and no required
conversation resolution. Codex still reviews them — that part is verified below — but nothing
enforces reading it. The gate is real only at the last step, where the whole batch faces it at
once. Between here and there the discipline is the agent's, and stated so plainly that failing
it is a choice rather than an oversight.

**A blocked issue does not block the batch.** If an issue turns out to need a decision, it is
parked: its branch stays open, the specific question goes to the director, and the rest of the
batch proceeds without it. Whether something is parkable or worth waiting on is the agent's
call — the point is that one contentious item never holds five others hostage.

## Where the agent stops and asks

Parking an issue is cheap; guessing on these is not. Break out of the loop for:

- **anything that changes a wire format** — codes already written cannot be renegotiated;
- **anything that changes public API in a way consumers feel**, since `data` is the base of the
  stack and every change ripples into `stories`, `content`, and `digihouse`;
- **any substantive disagreement with the reviewer** — as opposed to the agent simply being
  wrong, which is settled by fixing it;
- **anything needing manual testing** — Unity, the player, a physical device.

## Codex reviews pull requests into any branch

Verified rather than assumed: PR #54 targeted `feature/netstandard21`, not `main`, and received
a full review with a finding. The workflow depends on this, so it was checked before being
written down.

The review protocol in `AGENTS.md` applies unchanged, including the part that matters most here:
**an empty findings list means nothing on its own.** Ask `/reviews` whether a review was
submitted at all before asking `/comments` what it found. Silence is not approval, and with the
required-resolution rule absent on these branches, nothing else will catch the confusion.

## Naming

`integration/<milestone>-<topic>` — `integration/v0.10-tooling`, `integration/v0.10-test-layout`.

Feature branches keep the existing convention: `feature/…`, `fix/…`, `docs/…`.

The milestone belongs in the name because an integration branch outlives the conversation that
created it. In three weeks the branch name is the only context.

## What this does not cover

**One repository per batch.** An integration branch lives in a single repository, and the
BigRedProf work that hurts most is exactly the work that does not: renaming `Models` to `Data`
touches three repositories, and releasing the stack touches four. Cross-repository batching needs
one integration branch per repository and something to coordinate them, which is a harder problem
deliberately left for after this one works.

**Work that is inherently sequential.** A release chain cannot be batched at all: each step needs
the previous package published and indexed before the next can even build.
