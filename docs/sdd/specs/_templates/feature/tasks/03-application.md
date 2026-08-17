# Phase 03 — Application

> This phase only creates and orders subphases. Do not implement all Application use cases in this file.

- [ ] T013 Check agreed in phase 00 CQRS use cases and results of Domain/Infrastructure phases.
- [ ] T014 View the subphase templates in the [Application Templates](#application-templates) section and select only those that match the agreed use cases.
- [ ] T015 Copy each required template to `tasks/application/` as a separate file of the actual subphase: `03.1-<name>.md`, `03.2-<name>.md` and so on; replace `NN' with a number, placeholders with specific names and paths.
- [ ] T016 Assign a specific use case or joint Application responsibility to each subphase; do not mix several independent use cases in one file.

## Application templates

- [03.NN — Contracts and data access](application/03.NN-contracts.template.md)
- [03.NN — Get list](application/03.NN-get-list.template.md)
- [03.NN — Get by id](application/03.NN-get-by-id.template.md)
- [03.NN — Create](application/03.NN-create.template.md)
- [03.NN — Update](application/03.NN-update.template.md)
- [03.NN — Delete](application/03.NN-delete.template.md)
- [03.NN — Other use case](application/03.NN-other-use-case.template.md)

## Checkpoint

For each agreed Application use case or joint Application-responsibility, a separate `03.N' subphase file is created; unnecessary templates are not copied.

## The next phase

[04 — API](04-api.md) — only if the feature has an HTTP or integration API; otherwise go to [05 - Verification](05-verification.md).
