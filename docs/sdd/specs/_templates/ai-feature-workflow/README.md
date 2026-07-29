# Backend Feature Implementation Workflow

Execute the feature phase provided by the user in this order:

1. Find and read the current phase file in `<feature>/tasks/` and all
   applicable `AGENTS.md` files.
2. Read the feature `README.md` and the documents referenced by the current
   phase.
3. Complete only unfinished checkbox tasks in the current phase; do not start
   the next one.
4. After completing each task, mark it `[x]` in the current phase file.
5. Run relevant checks, record their result in the documentation, and provide a
   report.
6. Stop and wait for an explicit command to move to the next phase.

## Detailed Workflow

- [Context and Scope](01-context-and-scope.md) — how to identify the phase,
  check dependencies, and find a blocker.
- [Execution and Result Recording](02-execution-and-records.md) — how to
  perform tasks and update SDD documents.
- [Verification and Phase Handoff](03-verification-and-handoff.md) — how to
  verify the result and complete a phase.
