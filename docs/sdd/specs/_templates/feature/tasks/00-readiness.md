# Phase 00 — Refinement and Readiness

> This phase only creates and orders subphases. Do not execute all readiness decisions in this file.

- [ ] T001 View subphase templates in the section [Readiness Templates](#readiness-templates) and select the desired ones.
- [ ] T002 Copy each required template to `tasks/readiness/` as a separate file of the actual subphase: `00.1-<name>.md`, `00.2-<name>.md` and so on; replace `NN' with a number, placeholders with specific data.

## Readiness templates

- [00.NN — Scope, solutions and CQRS use cases](readiness/00.NN-scope.template.md)
- [00.NN — CLI, scripts and generators](readiness/00.NN-tooling.template.md)

## Checkpoint

Do not start the Domain until all created sub-phases `00.N` are completed, model decisions are agreed, a complete list of required and excluded use cases and integration/API-operations is completed. CLI/scripts are defined for automated operations; a complete machine-readable contract is not a condition of this checkpoint for a code-first workflow.

## The next phase

After completing all created sub-phases `00.N`, go to [01 - Domain Planning](01-domain.md).
