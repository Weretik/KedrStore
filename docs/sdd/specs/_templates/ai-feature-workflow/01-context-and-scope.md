# Context and scope

## Identify the current phase file

- Execute **only** the exact file provided by the user: a main phase
  `tasks/00-*.md`–`tasks/05-*.md` or a subphase such as
  `tasks/application/03.2-create.md`.
- If the user provides only a number, find one matching actual file. If there
  are several (for example, `03.1` and `03.2`), ask for the exact path or name.
- A main file `00`–`05` only selects and creates subphases. After it is
  complete, do not start any generated subphase until the user explicitly names
  its file.
- You may read future phases only to understand dependencies. Do not modify
  their tasks, code, or checks without an explicit user command.

## Read before changing anything

1. Find every applicable `AGENTS.md` from the repository root to each file you
   plan to change, and follow all applicable instructions.
2. Read `<feature>/README.md`, `tasks/README.md`, the current phase file, and
   the applicable documents in `requirements/`, `design/`, `data-model.md`,
   `contracts/`, and `checklist/` in full.
3. For main phase `00` and subphase `00.N`, read the entire feature package,
   including `checklist/spec-readiness.md`. For an HTTP or integration consumer,
   also read `contracts/api-contract.md` and the corresponding file in
   `docs/sdd/contracts/<module>/`.
4. Check dependencies: the prior main phase and every required generated
   subphase must be `[x]`, and the checkpoint must contain no unresolved
   blocker.
5. Read only the relevant stable rules: `docs/sdd/architecture/README.md`,
   `docs/sdd/standards/README.md`, and the necessary domain, database, API,
   security, or testing rules.
6. Inspect existing code, tests, and configuration only as much as the current
   phase requires.

## When to stop

Do not invent a business rule, endpoint, DTO field, role, data schema, file
path, or requirement that is absent from the specification or current code. If a
decision is missing, record `[NEEDS CLARIFICATION: <question>]` in the relevant
feature document, leave the task open, explain the blocker briefly, and wait for
a decision.

Do not make incidental refactors, dependency upgrades, or changes outside the
scope of the current phase.
