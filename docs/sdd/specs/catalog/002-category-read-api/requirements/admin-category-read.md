# Category read API — admin category read

## Behavior

- The internal admin UI requests `GET /api/categories` anonymously and receives every category as a flat array for tables, selectors, and hierarchy editors.
- The internal admin UI requests `GET /api/categories/{id}` anonymously and receives one category by its numeric backend identity.
- An admin row exposes `id`, `name`, `shortNameUk`, `shortNameRu`, `slug`, `productTypeIdOneC`, nullable `parentId`, `sortOrder`, and `level`. Technical `Path` remains internal.

## Rules and invariants

- Both operations are temporarily anonymous, consistent with the current admin-products read surface. They remain an internal UI surface and must be protected by a future access-control change before external exposure.
- The list is unpaged because it is the finite category reference set used by the administration UI. It is a depth-first flattening of the hierarchy; each sibling set is ordered by `sortOrder`, then `id`. `parentId`, `level`, and `sortOrder` remain available for the client to build its own tree.
- `id` is a positive integer. A non-positive id is a `400` validation error. An absent positive id returns bodyless `404`.
- The endpoints are safe read-only GET operations and need neither a request body nor an idempotency key.

## Acceptance scenarios

1. Given an anonymous request, when `GET /api/categories` is requested, then the response is `200` with all category rows and both Ukrainian and Russian short names.
2. Given an anonymous request, when either admin route is requested, then it is not challenged or forbidden by endpoint authorization.
3. Given an existing category id, when `GET /api/categories/{id}` is requested anonymously, then it returns that full admin row.
4. Given a positive category id not present in the catalog, when the item route is requested, then it returns `404` with no body.
