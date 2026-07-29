# Git commit batching

A commit represents one reviewable intent and must not mix unrelated changes.

## Core rule: split substantial work

Do not put functionality containing several independent changes into one large commit. Split it into small, ordered commits when the changes can be reviewed, tested, reverted, or cherry-picked independently.

Typical boundaries for a separate commit:

- feature specification or documentation;
- domain model and invariants;
- application command/query and validation;
- persistence configuration or migration;
- HTTP contract or API exposure;
- tests;
- an independent bug fix, formatting-only change, generated code, or dependency update.

Keep a related migration, entity model, and EF configuration together if splitting them would leave the repository unable to build or execute the migration. Do not split work solely to create artificially small commits.

## Recommended batches

1. `docs(<module>): add <feature> specification`
2. `feat(<module>): add domain model and invariants`
3. `feat(<module>): add application use case and validation`
4. `feat(<module>): add infrastructure adapter or migration`
5. `feat(<module>): expose API contract`
6. `test(<module>): cover <feature> scenarios`
7. `docs(<module>): record <feature> verification`

Skip a batch that has no changes. For a small cohesive change, combine adjacent batches only when they cannot be reviewed independently.

## Commit-message format

Use a short imperative subject:

```text
<type>(<module>): <short change>
```

Examples:

```text
feat(catalog): add admin product list query
fix(identity): rotate refresh session on token reuse
docs(sdd): add operations runbook
```

For a substantial commit, add a body after a blank line. A commit is substantial when it changes more than one layer, API/database/security behavior, has a non-obvious trade-off, or cannot be understood from the subject alone.

```text
feat(identity): add administrator user creation

Create the user through the Identity application use case and assign only
the Admin role. The endpoint is protected by CanManageUsers; passwords and
Identity provider errors are never returned in the response.

Tests: unit tests for validation; API authorization cases.
```

The body explains **what changed**, **why it changed**, and any important effect on compatibility, security, migration, or verification. It must not include secrets, tokens, connection strings, or copied logs.

## Commit-size control

Keep commit size balanced:

- Split substantial functionality by independent, reviewable intent rather than by files or methods.
- After every commit, the branch must build; each commit must be understandable, independently testable, and safe to revert.
- Do not create one oversized commit if domain, migration, application, API, and test changes can be verified separately.
- Do not turn one cohesive large feature into dozens of micro-commits (for example, 20 or more) when they cannot be independently reviewed, tested, or reverted.
- Combine inseparable changes, such as an entity property, its EF configuration, and the required migration. Split changes only where a genuine responsibility or behavior boundary exists.

Before creating commits, describe the proposed change batches and briefly explain any unusually large or small batch.

## Rules

- Do not combine formatting, generated files, dependency updates, or unrelated fixes with functionality.
- If work covers several reviewable intents, create several commits rather than one aggregate commit. Order them so each has a clear purpose and the series is easy to review.
- Commit migrations with the corresponding model/configuration change.
- A commit with an incompatible change includes `BREAKING CHANGE` in the body and a link to the migration plan.
- The final documentation commit records completed verification and known limitations; it must not claim checks that were not run.
