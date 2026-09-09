# Execution and result recording

## Execute a testable technical task

1. Confirm the task's `TS-*`, `Covers`, dependencies, exact paths, test level,
   and checkpoint.
2. **Red**: add the smallest focused test for the selected rule or scenario.
   Run it and confirm that it fails because the behavior is absent or wrong.
3. **Green**: implement the smallest complete change that passes that test.
   Preserve the dependency direction `API -> Application -> Domain`;
   Infrastructure implements interfaces owned by inner layers.
4. **Refactor**: improve structure within the agreed design and rerun the
   focused tests.
5. **Regression**: run the affected suite and record the result.
6. Mark checklist steps only after their evidence exists. Update the scenario
   row in `traceability.md`, then select the next ready in-scope task.

Do not accept a compilation failure, broken fixture, missing dependency, or
unrelated test failure as valid Red evidence.

## Execute an enabler or exception

For an `EN-*` task, record why Red-first behavior is not meaningful, which
scenarios it enables, the accepted command or generator, and replacement
verification. Generated output is inspected but changed manually only when the
generator cannot express the agreed result; record that reason.

## Where to record results

| Event | Record it in |
| --- | --- |
| Red/Green/refactor/regression evidence | Current `TS-*` file |
| Enabler exception and replacement check | Current `EN-*` file |
| Completed technical task | Current task file and `traceability.md` |
| Completed acceptance scenario | `traceability.md` |
| Clarified behavior | `README.md` or relevant `requirements/` file |
| Technical decision | Relevant design or data-model document |
| Contract change | Feature contract and versioned OpenAPI |
| Blocker or deviation | Current task and affected scenario row |

## Scope discipline

Make the smallest change that fully completes the current task. Do not perform
incidental refactors or upgrades. After completion, continue to the next ready
task only while it remains inside the user's authorized scope.
