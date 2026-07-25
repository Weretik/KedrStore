# Git commit batching

Commits describe one reviewable intent and must not mix unrelated work.

## Default rule: split substantial work

Do not put a feature with several independent changes into one large commit. Split it into small, ordered commits when the changes can be reviewed, tested, reverted or cherry-picked separately.

Typical boundaries for a separate commit are:

- feature specification or documentation;
- domain model and invariants;
- application command/query and validation;
- persistence configuration or migration;
- HTTP contract/API exposure;
- tests;
- a separate bug fix, formatting-only change, generated code or dependency update.

Keep related migration, entity-model and EF configuration changes together when separating them would leave the repository unable to build or migrate. Do not split merely to create artificial tiny commits.

## Recommended batches

1. docs(<module>): add <feature> specification
2. feat(<module>): add domain model and invariants
3. feat(<module>): add application use case and validation
4. feat(<module>): add infrastructure adapter or migration
5. feat(<module>): expose API contract
6. test(<module>): cover <feature> scenarios
7. docs(<module>): record <feature> verification

Omit a batch that has no change. For a small, cohesive change, combine adjacent batches only when they cannot be reviewed independently.

## Commit message format

Use a concise, imperative subject:

~~~text
<type>(<module>): <short change>
~~~

Examples:

~~~text
feat(catalog): add admin product list query
fix(identity): rotate refresh session on token reuse
docs(sdd): add operations runbook
~~~

For a substantial commit, add a body after a blank line. A commit is substantial when it changes more than one layer, changes an API/database/security behaviour, has a non-obvious trade-off, or cannot be understood from the subject alone.

~~~text
feat(identity): add administrator user creation

Create the user through the Identity application use case and assign only
the Admin role. The endpoint is protected by CanManageUsers; passwords and
Identity provider errors are never returned in the response.

Tests: unit tests for validation; API authorization cases.
~~~

The body explains **what changed**, **why it changed**, and any important compatibility, security, migration or verification impact. It must not contain secrets, tokens, connection strings or copied logs.

## Rules

- Do not combine formatting, generated files, dependency upgrades or unrelated fixes with a feature.
- If the work spans several reviewable intents, create several commits instead of one omnibus commit. Order them so each commit has a clear purpose and the series is easy to review.
- Migrations are committed with their matching model/configuration change.
- A breaking change commit states BREAKING CHANGE in its body and links the migration plan.
- The final documentation commit records executed checks and known limitations; it must not claim checks that did not run.
