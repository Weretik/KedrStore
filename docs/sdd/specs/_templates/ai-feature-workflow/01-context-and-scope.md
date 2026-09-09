# Context, scope, and task selection

## Resolve the authorized scope

The user's request may authorize a complete feature, a phase, one or more
`SC-*` scenarios, or named `TS-*`/`EN-*` tasks. Use the broadest scope clearly
authorized by the request. Do not shrink feature-level authorization to one
file, and do not expand a named-task request to the full feature.

Within the authorized scope, no separate instruction is required to open and
execute the next ready task file.

## Read before changing anything

1. Find every applicable `AGENTS.md` from the repository root to each planned
   change.
2. Read the feature `README.md`, `requirements/`, `traceability.md`,
   `tasks/README.md`, selected task files, and relevant design, data-model,
   contract, and checklist documents.
3. For HTTP or integration behavior, read both the human-readable contract and
   corresponding versioned OpenAPI file.
4. Read only the relevant architecture and standards documents, including
   `docs/sdd/standards/testing-rules.md`.
5. Inspect existing code, tests, commands, and configuration needed to verify
   that the next task is ready.

## Select the next task

A task is ready when its dependencies are complete, its covered scenarios are
unambiguous, its paths and checkpoint are concrete, and the required test level
can be executed or has a valid documented exception.

Prefer this order within a scenario:

1. shared `EN-*` prerequisites;
2. inner-layer `TS-*` behavior required by outer layers;
3. Application orchestration;
4. persistence or external adapters;
5. API contract and transport behavior;
6. scenario acceptance and regression verification.

This dependency order does not require finishing all tasks in one layer across
the entire feature.

## When to stop

Stop only when the authorized scope is complete or when progress requires an
unresolved product decision, a failed prerequisite, external authorization, a
destructive action outside the authorized scope, or another concrete blocker.
Record `[NEEDS CLARIFICATION: <question>]` in the relevant document when product
behavior is missing. Do not invent a rule, endpoint, DTO field, role, schema, or
requirement.
