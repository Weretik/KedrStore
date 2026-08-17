# Phase 04 — API

> This phase only creates and orders subphases. Do not implement the entire API layer in this file.

- [ ] T017 If the feature has an HTTP/integration surface, view [API Templates](#api-templates) and select the required subphases.
- [ ] T018 Copy each required template to `tasks/api/` as a separate file of the actual subphase: `04.1-<name>.md`, `04.2-<name>.md` and so on; replace `NN' with a number and placeholders with specific names and paths.
- [ ] T019 Execute API subphases in the order: controllers/endpoints → HTTP contracts, authorization and Result mapping → OpenAPI → API tests.
- [ ] T020 Do not create API subphases only for feature without HTTP or integration API.

## API Templates

- [04.NN — Controllers and endpoints](api/04.NN-controllers.template.md)
- [04.NN — HTTP contracts, rights and Result mapping](api/04.NN-http-behavior.template.md)
- [04.NN — OpenAPI documentation](api/04.NN-contract-documentation.template.md)
- [04.NN — API/integration tests](api/04.NN-tests.template.md)

## Checkpoint

For the HTTP/integration feature, the API layer has separate `04.N` subphase files for controllers, HTTP behavior, documentation, and integration tests.

## The next phase

[05 — Verification](05-verification.md)
