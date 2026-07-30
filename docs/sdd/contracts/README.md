# API contracts

`docs/sdd/contracts/` is the canonical, versioned source for machine-readable public API contracts.

## Layout

```text
docs/sdd/contracts/
├── openapi.yaml                         # aggregate entry point for all public APIs
└── <module>/
    └── <feature>.openapi.yaml           # contract owned by one feature
```

`openapi.yaml` is the entry point for Swagger UI, frontend client generation, and contract tests. It references the module contracts by `$ref`; do not duplicate operations or schemas in the aggregate file.

## Current frontend handoff

- [Frontend integration guide](frontend-integration.md) explains authentication, CSRF, paging, errors, and endpoint ownership.
- [Aggregate OpenAPI 3.1 contract](openapi.yaml) covers every currently registered controller.
- Module contracts: [catalog products](catalog/products.openapi.yaml), [catalog categories](catalog/category-read.openapi.yaml), [identity](identity/session.openapi.yaml), and [sales](sales/catalog.openapi.yaml).

The OpenAPI files are a source contract for the code as it exists today. They do not grant public access to an endpoint: the authorization annotations and the host fallback policy remain authoritative.

Each feature SDD keeps `contracts/api-contract.md` for human decisions: scope, consumers, compatibility, security, errors, idempotency, and rollout. When a machine-readable contract is agreed, that document links to `docs/sdd/contracts/<module>/<feature>.openapi.yaml` and the aggregate `openapi.yaml` is updated in the same change.

Do not create a public OpenAPI document for an internal-only change. Before adding an HTTP contract, agree the route, method, operation ID, DTOs, examples, errors, security, idempotency for side-effecting writes, and breaking-change migration path.
