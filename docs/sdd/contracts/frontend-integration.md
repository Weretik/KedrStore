# Frontend integration with the KedrStore API

The versioned OpenAPI entry point is [openapi.yaml](openapi.yaml). Generate types and clients from this file; do not copy DTOs from backend source code.

## Authentication

`POST /api/auth/session/login` returns an opaque Bearer access token and sets two cookies: the HTTP-only refresh cookie `kedr.rt` (sent only to the refresh route) and the readable CSRF cookie `kedr.csrf`.

For protected operations, send `Authorization: Bearer <accessToken>`. To refresh the session, call `POST /api/auth/session/refresh` with `credentials: 'include'` and send the current `kedr.csrf` cookie value in the `X-CSRF-Token` header. The refresh endpoint changes both cookies and returns a new access token. `POST /api/auth/session/logout` also requires Bearer authentication and clears both cookies.

The host has a fallback policy that requires authentication. Public endpoints today are those explicitly marked anonymous in a controller; the Sales catalog and authenticated session endpoints are not public.

## HTTP rules

- JSON property names use ASP.NET Core camelCase.
- Lists have the shape `{ pagedInfo, value }`. `pagedInfo` contains `pageNumber`, `pageSize`, `totalPages`, and `totalRecords`; `value` is an array of rows.
- Validation errors return `400` with the existing Ardalis validation error array. Do not couple logic to localized error text.
- `404` has no response body. `409` returns an array of conflict messages. `401` means authentication is missing/invalid; `403` means an authenticated user lacks permission.
- Decimal values are JSON numbers. Identifiers are integers unless the schema explicitly defines `uuid` or `string`.

## Endpoint map

| Area | Operation | Access |
| --- | --- | --- |
| Public catalog | `GET /api/catalog/{lang}/products`, product details | Anonymous |
| Public category navigation | `GET /api/catalog/{lang}/categories`, `GET /api/catalog/{lang}/categories/by-slug/{categorySlug}` | Anonymous |
| Admin category reads | `GET /api/categories`, `GET /api/categories/{id}` | Anonymous (temporary internal UI surface) |
| Admin product read model | `GET /api/admin/products`, `/all` | Anonymous in current code; treat as internal/admin UI surface |
| Quick order | `POST /api/orders` | Anonymous |
| Session | login, refresh, logout, current user | Mixed; see Identity contract |
| Sales catalog | `POST /api/sales/{lang}/catalog/products` | Bearer token required |

For catalog read APIs, `lang` accepts `uk` or `ru`. The category collection is a navigation tree with no paging: use the server-provided `id`, `slug`, `parentId`, `sortOrder`, `level`, and `children` fields; do not reconstruct order or hierarchy from names. Category names are already localized for `lang`; category details add the path from the root to the current node. Product page size is capped at 100. The server currently normalizes invalid lower page values at runtime, but clients must always send `page >= 1` and `pageSize` from 1 to 100.

The Admin category list is a flat hierarchy without paging, ordered by depth-first traversal. It returns the source `name`, both localized short names, `productTypeIdOneC`, and structural fields; build the admin UI tree by `parentId` and preserve the provided order. The individual item route uses numeric `id`, not `slug`.
