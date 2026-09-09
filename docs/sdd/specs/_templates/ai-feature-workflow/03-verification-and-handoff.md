# Verification and handoff

## Verify each task and scenario

1. Run the focused check required by the current task.
2. Run the affected regression tests required by
   `docs/sdd/standards/testing-rules.md` and applicable `AGENTS.md` files.
3. When the last task for an `SC-*` scenario is complete, run its acceptance
   verification and update `traceability.md` to `verified`.
4. For full-feature scope, run the agreed restore/build/test commands and
   complete `checklist/delivery-readiness.md`.
5. Review `git diff` for the current scope and preserve unrelated user changes.

If a command fails, record the exact command, failure point, and whether the
failure was introduced by the change or was already present.

## Completion conditions

A technical task is complete only when its checklist, checkpoint, and evidence
are complete. A scenario is complete only when every required `TS-*`/`EN-*`
task and its acceptance evidence are complete. A feature is complete only when
all in-scope scenarios and delivery gates are complete.

## Progress and final report

During an authorized multi-task run, report material findings and continue; do
not pause only because a file or layer boundary was crossed.

The final report contains:

1. **Completed scope** — scenarios and technical task IDs.
2. **Behavior delivered** — observable result.
3. **Changed files** — files changed in the authorized scope.
4. **TDD evidence** — Red, Green, and regression commands/results, plus any
   justified `EN-*` exceptions.
5. **Not verified or blocked** — exact reason and next step.
6. **Residual risks** — only real remaining risks.

End when the authorized scope is complete or blocked. Do not say that a command
naming the next task file is required when the next task is already authorized.
