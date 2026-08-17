# Phase 04 — API

- [ ] [T110 — Admin order controller](api/04.1-orders-controller.md)
- [ ] [T120 — HTTP contract, authorization, and result mapping](api/04.2-http-behavior.md)
- [ ] [T130 — OpenAPI contract](api/04.3-openapi.md)
- [ ] [T140 — API integration tests](api/04.4-api-tests.md)

## Checkpoint

`POST /api/admin/orders` is protected with `PolicyNames.CanManageOrders`, documented in the versioned OpenAPI contract, and verified through HTTP integration tests.
