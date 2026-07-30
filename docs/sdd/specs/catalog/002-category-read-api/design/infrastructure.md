# Category read API — design: Infrastructure

## Persistence and integrations

Reuse the existing `ProductCategory` EF mapping and category read repository. Add read specifications under `Catalog.Application/Features/Category/` for the public tree/slug reads and admin list/id reads; all use no tracking and request only the fields required by their DTO. Add a public `CategoriesController` under `Catalog.Api/Controllers` with route prefix `api/catalog/{lang}`, and an `AdminCategoriesController` with route prefix `api/categories`. Both controllers are temporarily anonymous; admin routes are limited to the internal UI until a future access-control feature protects them. Both map HTTP to Mediator, preserve cancellation tokens, and use the established Ardalis.Result mapping.

## Migration and rollout

No migration, backfill, external adapter, or configuration change is required. Rollout is additive: deploy the controller and application queries together with the OpenAPI contract. Rollback removes only the new routes; it does not alter stored categories or existing product routes.
