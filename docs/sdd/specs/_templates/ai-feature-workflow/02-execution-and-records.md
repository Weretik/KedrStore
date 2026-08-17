# Execution and result recording

## Completing checkbox tasks

1. Complete tasks in the order of the current phase or subphase file, except
   explicitly marked independent `[P]` tasks whose dependencies are complete.
2. Read a file's current contents before changing it; preserve its encoding,
   style, and unrelated user changes.
3. Make the smallest change that fully completes the task. Follow the
   dependency direction `API -> Application -> Domain`; Infrastructure depends
   only on inner layers.
4. Implement use cases with CQRS through Mediator. Domain owns invariants;
   Application owns orchestration and FluentValidation; API owns HTTP mapping;
   Infrastructure owns persistence and external adapters.
5. Use CLI-first: for migrations, SQL scripts, OpenAPI validation, restore,
   build, tests, and generators, first find and run the repository's accepted
   command or script. Change output manually only when a generator cannot
   express the required result correctly, and record why.
6. After local verification, change only the task marker from `- [ ]` to
   `- [x]`. Do not mark a partially complete, blocked, or unverified task.

## Where to record results

| Event | Record it in |
| --- | --- |
| Completed task ID | The current main-phase or subphase file: `- [ ]` to `- [x]`. |
| Clarified scope or requirement | `README.md` or the relevant `requirements/` file. |
| Domain, persistence, or integration decision | `design/domain.md`, `design/infrastructure.md`, or `data-model.md`. |
| Clarified HTTP/integration contract | `contracts/api-contract.md` and, once agreed, `docs/sdd/contracts/<module>/<feature>.openapi.yaml`. |
| Completed check or discovered blocker | The current phase or delivery report, when the task requires one. |

A main phase creates the required subphase files from templates, replacing `NN`,
placeholders, and task IDs; it does not implement those subphases in the same
turn. Do not mark future phases complete or fill in the delivery checklist
before subphase `05.N` unless the current task explicitly requires it.
