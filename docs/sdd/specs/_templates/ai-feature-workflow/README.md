# Backend Feature Implementation Workflow

Execute only the exact phase or subphase file provided by the user. The main
phases `00`–`05` orchestrate the work: they create the required individual
subphases, but do not contain every implementation detail.

1. Find and read the exact file in `<feature>/tasks/` and all applicable
   `AGENTS.md` files.
2. Read `tasks/README.md`, the feature `README.md`, and the documents linked
   by the current file.
3. Complete only the unfinished checkbox tasks in the current file; do not
   start another main phase or subphase without an explicit user command.
4. After completing each task, mark it `[x]` in that same file.
5. Run relevant checks, record the result in the documentation, and provide a
   report.
6. Stop and wait for an explicit command naming the next exact file.

## Phase structure

| Main phase | Role | Where to create concrete subphases |
| --- | --- | --- |
| `00` — Readiness | select scope, use cases, and tooling | `tasks/readiness/00.N-*.md` |
| `01` — Domain | plan Domain slices | `tasks/domain/01.N-*.md` |
| `02` — Infrastructure | plan Infrastructure slices | `tasks/infrastructure/02.N-*.md` |
| `03` — Application | plan contracts and use cases | `tasks/application/03.N-*.md` |
| `04` — API | plan HTTP/API work | `tasks/api/04.N-*.md` |
| `05` — Verification | plan delivery checks | `tasks/verification/05.N-*.md` |

A main phase file creates the required subphases from templates and then ends.
AI executes every generated `NN.N-*.md` file only after a separate user command.

## Detailed workflow

- [Context and scope](01-context-and-scope.md) — identify the phase, check
  dependencies, and find blockers.
- [Execution and result recording](02-execution-and-records.md) — perform
  tasks and update SDD documents.
- [Verification and phase handoff](03-verification-and-handoff.md) — verify
  the result and complete a phase.
