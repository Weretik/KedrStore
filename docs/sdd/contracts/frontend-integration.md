# KedrStore API — frontend integration guide

The versioned OpenAPI entry point is [openapi.yaml](openapi.yaml). Generate types and clients from that file; do not copy DTOs from backend source.

## Authentication

`POST /api/auth/session/login` returns an opaque Bearer access token and sets two cookies: the HTTP-only refresh cookie `kedr.rt` (sent only to the refresh route) and the readable CSRF cookie `kedr.csrf`.

Send `Authorization: Bearer <accessToken>` to protected operations. To renew a session, call `POST /api/auth/session/refresh` with `credentials: 'include'` and set header `X-CSRF-Token` to the current `kedr.csrf` cookie value. The refresh endpoint rotates both cookies and returns a new access token. `POST /api/auth/session/logout` also requires Bearer authentication and clears both cookies.

The host has an authenticated fallback policy. The endpoints explicitly marked anonymous in their controller are public today; the sales catalog and authenticated session endpoints are not.

## HTTP conventions

- JSON uses ASP.NET Core camelCase property names.
- List responses use `{ pagedInfo, value }`. `pagedInfo` contains `pageNumber`, `pageSize`, `totalPages`, and `totalRecords`; `value` is the array of rows.
- Validation failures return `400` with the existing Ardalis validation-error array. Do not depend on localized error-message text.
- `404` has no response body. `409` returns an array of conflict messages. `401` means missing/invalid authentication and `403` means authenticated but forbidden.
- Decimal values are JSON numbers. IDs are integers unless a schema explicitly says `uuid` or `string`.

## Endpoint map

| Area | Operation | Access |
| --- | --- | --- |
| Public catalog | `GET /api/catalog/{lang}/products`, product details | Anonymous |
| Admin product read model | `GET /api/admin/products`, `/all` | Anonymous in current code; treat as an internal/admin UI surface |
| Quick order | `POST /api/orders` | Anonymous |
| Session | login, refresh, logout, current user | Mixed; see identity contract |
| Sales catalog | `POST /api/sales/{lang}/catalog/products` | Bearer token required |

`lang` accepts `uk` or `ru` for the catalog read APIs. Product list page sizes are limited to 100. The backend currently normalizes invalid lower page values at runtime, but frontend clients should always send `page >= 1` and `pageSize` from 1 through 100.
