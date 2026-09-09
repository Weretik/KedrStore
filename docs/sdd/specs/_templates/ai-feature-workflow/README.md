# AI feature implementation workflow

Use this workflow only for an accepted feature specification. The user may
authorize the complete feature, a phase, selected `SC-*` scenarios, or named
`TS-*`/`EN-*` tasks.

```text
Use `docs/sdd/specs/_templates/ai-feature-workflow/`.
Feature: `docs/sdd/specs/<module>/<NNN>-<feature-slug>`.
Scope: <complete feature | phase | SC-IDs | TS/EN IDs>.
```

AI continues through every ready task inside the authorized scope. Crossing a
task-file or layer boundary does not require a separate command.

## Workflow

1. [Resolve scope and select ready tasks](01-context-and-scope.md).
2. [Execute tasks and record results](02-execution-and-records.md).
3. [Verify behavior and report the result](03-verification-and-handoff.md).
