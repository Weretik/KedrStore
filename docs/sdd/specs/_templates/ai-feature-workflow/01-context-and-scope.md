# Context and Scope

## Identify the Current Phase

- Execute **only** the `tasks/<NN>-*.md` file whose number the user provided.
- If the user names a specific phase file, verify that it matches the phase number.
  If it does not, stop and request clarification.
- You may read later phases only to understand dependencies. Do not change their
  tasks, code, or verification without an explicit user instruction.

## Read Before Making Changes

1. Find every `AGENTS.md` from the repository root to each file you plan to
   change, and follow all applicable instructions.
2. Read the entire `<feature>/README.md`, the current `tasks/<NN>-*.md`, and
   the applicable files in `requirements/`, `design/`, `data-model.md`,
   `contracts/`, and `checklist/`.
3. For phase `00`, read the entire feature package, including
   `checklist/spec-readiness.md`. For an HTTP or integration consumer, also
   read `contracts/api-contract.md` and the corresponding file in
   `docs/sdd/contracts/<module>/`.
4. Check preceding phases: every task must be marked `[x]`, and their
   checkpoints must not contain an unresolved blocker.
5. Read only the relevant stable rules:
   `docs/sdd/architecture/README.md`, `docs/sdd/standards/README.md`, and the
   required rules for domain, database, API, security, or testing.
6. Inspect existing code, tests, and configuration only as much as the current
   phase requires.

## When to Stop

Do not invent a business rule, endpoint, DTO field, role, data schema, file
path, or requirement that is not present in the specification or existing code.
If a decision is missing, record `[NEEDS CLARIFICATION: <question>]` in the
relevant feature document, leave the task open, briefly explain the blocker, and
wait for a decision.

Do not perform incidental refactoring, dependency upgrades, or changes outside
the scope of the current phase.
