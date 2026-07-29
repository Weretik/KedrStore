# Execution and Result Recording

## Completing Checkbox Tasks

1. Perform tasks in the order listed in the phase file, except for explicitly
   marked independent `[P]` tasks after their dependencies are complete.
2. Read the file's current content before changing it; preserve its encoding,
   style, and unrelated user changes.
3. Make the smallest change that fully completes the task. Follow the dependency
   direction `API -> Application -> Domain`; Infrastructure depends only on
   inner layers.
4. Use CQRS through Mediator for each use case. Domain owns invariants,
   Application owns orchestration and FluentValidation, API owns only HTTP
   mapping, and Infrastructure owns persistence and external adapters.
5. After local verification, change only the task marker from `- [ ]` to
   `- [x]`. Do not mark a partially completed, blocked, or unverified task as
   complete.

## Where to Record the Result

| Event | Record it in |
| --- | --- |
| A task ID is completed | The current `tasks/<NN>-*.md`: `- [ ]` → `- [x]`. |
| Scope or a requirement is clarified | `README.md` or the relevant file in `requirements/`. |
| A domain, persistence, or integration decision is made | `design/domain.md`, `design/infrastructure.md`, or `data-model.md`. |
| An HTTP/integration contract is clarified | `contracts/api-contract.md` and, once agreed, `docs/sdd/contracts/<module>/<feature>.openapi.yaml`. |
| Verification is performed or a blocker is found | The current phase or the delivery report, if the task requires it. |

Do not mark tasks in future phases or complete the delivery checklist before
phase `05`, unless the current task explicitly requires it.
