# <feature name> — API contract

**Status:** draft | agreed

This document contains solutions for people: contract boundaries, consumers, compatibility, security, errors, idempotency, and deployment.

Once agreed, create a machine-readable OpenAPI contract for the frontend in `docs/sdd/contracts/<module>/<feature>.openapi.yaml`. Do not store `.openapi.yaml` in this feature folder. Add `$ref` from the aggregated `docs/sdd/contracts/openapi.yaml` to the contract within the same change. It is a single versioned source for frontend, client generation and contract tests; Swagger UI only displays it.

Rules for the structure and support of contracts are given in [`docs/sdd/contracts/README.md`](../../../../contracts/README.md).

Define routes, HTTP methods, operation identifiers, DTOs, examples, errors, security, idempotence for side-effect records, and a migration path for incompatible changes prior to approval.
