# API contracts

`docs/sdd/contracts/` is the canonical versioned source for machine-readable public API contracts.

## Structure

```text
docs/sdd/contracts/
├── openapi.yaml                         # aggregate entry point for public APIs
└── <module>/
    └── <feature>.openapi.yaml           # one feature contract
```

`openapi.yaml` is the entry point for Swagger UI, frontend client generation, and contract tests. It includes module contracts through `$ref`; do not duplicate operations or schemas in the aggregate file.

## Current frontend contract handoff

- [Frontend integration guide](frontend-integration.md) describes authentication, CSRF, pagination, errors, and endpoint owners.
- [Aggregate OpenAPI 3.1 contract](openapi.yaml) covers currently registered controllers.
- Catalog: [products](catalog/products.openapi.yaml), [categories](catalog/category-read.openapi.yaml), [quick orders](catalog/orders.openapi.yaml).
- Sales: [manager catalog](sales/catalog.openapi.yaml), [orders and 1C sync](sales/admin-order-one-c-sync.openapi.yaml), [order reads](sales/manager-order-read.openapi.yaml), [customer reads](sales/customer-read.openapi.yaml).
- Identity: [session](identity/session.openapi.yaml).

The OpenAPI files describe the current code contract. They do not grant endpoint access: authorization attributes and the host fallback access policy remain authoritative.

Record scope, consumers, compatibility, security, errors, idempotency, and deployment decisions in each feature's `contracts/api-contract.md`. For new or changed HTTP behavior, agree on these decisions and, where practical, create or update the machine-readable contract before implementing the transport layer. Add targeted API/contract Red tests against the agreed contract, then implement the transport. The feature document links to `docs/sdd/contracts/<module>/<feature>.openapi.yaml`; update aggregate `openapi.yaml` in the same change.

Do not create a public OpenAPI document for an internal-only change. Before adding an HTTP contract, agree on route, method, operation ID, DTOs, examples, errors, security, write idempotency for side effects, and a migration path for breaking changes.

## Admin workspace

- [Admin order workspace](sales/admin-order-workspace.openapi.yaml).
- [Admin order drafts](sales/admin-order-drafts.openapi.yaml).

These operations are implemented in a separate Admin API and included in the aggregate `openapi.yaml`.
