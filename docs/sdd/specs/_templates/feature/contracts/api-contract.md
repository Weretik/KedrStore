# <feature name> — API contract

**Status:** draft | agreed

After agreement, create `docs/sdd/contracts/<module>/<feature>.openapi.yaml` and reference its operations from `docs/sdd/contracts/openapi.yaml`. This is the versioned machine-readable contract for the frontend, client generation, and contract testing; Swagger UI only renders the aggregate entry point.

Before agreement, define routes, methods, operation IDs, DTOs, examples, errors, security, idempotency, and the breaking-change migration path. Keep feature-specific consumer and rollout decisions in this document; do not duplicate the OpenAPI YAML under the feature folder.
