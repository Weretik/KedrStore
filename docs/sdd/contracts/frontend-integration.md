# Frontend integration with the KedrStore API

The versioned OpenAPI entry point is [openapi.yaml](openapi.yaml). Generate types and clients from this file; do not copy DTOs from backend source code.

## Authentication

Each client signs in its own way; see the [session contract](identity/session.openapi.yaml).

**Admin and storefront (browser).** Call `POST /api/auth/cookie/login` with `credentials: 'include'`. The API sets the HttpOnly session cookie `__Host-kedr.session` and the readable `kedr.csrf` cookie; no token reaches JavaScript. Send every request with `credentials: 'include'`. Every request that is not `GET`, `HEAD`, or `OPTIONS` must carry `X-CSRF-Token` equal to the current `kedr.csrf` cookie, otherwise `400`. `GET /api/auth/session/me` returns the user and reissues a missing CSRF cookie. Sign out with `POST /api/auth/cookie/logout`.

**Mobile.** Call `POST /api/auth/mobile/login`; keep `accessToken` and `refreshToken` in the device secure store. Send `Authorization: Bearer <accessToken>`. On `401`, call `POST /api/auth/mobile/refresh` with `{ refreshToken }` once and retry; each refresh returns a new pair and the old refresh token stops working. Sign out with `POST /api/auth/mobile/logout` and `{ refreshToken }`. Bearer requests need no CSRF header.

The earlier `/api/auth/session/login|refresh|logout` endpoints were removed on 2026-10-28; only `/api/auth/session/me` remains under that path.

The host has a fallback policy that requires authentication. Public endpoints are those explicitly marked anonymous in a controller. `401` means not signed in: show the login page. `403` means signed in without the needed role: show a "no access" message.

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
| Admin catalog | `GET /api/categories`, `/api/categories/{id}`, `/api/admin/products/**` | Manager or Admin (`CanManageProducts`) |
| Admin orders and customers | `/api/admin/orders/**`, `/api/admin/order-workspace/**`, `/api/admin/customers/**` | Manager or Admin (`CanManageOrders`) |
| Staff accounts | `/api/admin/users/**` | Admin (`CanManageUsers`) |
| Site articles | `GET /api/content/{lang}/articles`, `/api/content/{lang}/articles/{slug}`, `/api/content/articles/{id}/cover` | Anonymous; `coverPath` in responses is relative to the API base URL |
| Admin articles | `/api/admin/articles/**` | Manager or Admin (`CanManageContent`) |
| Company contacts | `GET /api/content/contacts` | Anonymous; phones in E.164, clients format them |
| Admin company contacts | `GET`/`PUT /api/admin/content/contacts` | Manager or Admin (`CanManageContent`) |
| Company requisites | `GET /api/content/requisites` | Anonymous; IBAN without spaces, clients group it |
| Admin company requisites | `GET`/`PUT /api/admin/content/requisites`, `GET …/history` | Admin only (`RequireAdminRole`); each change logged |
| Site documents | `GET /api/content/{lang}/documents/{kind}` | Anonymous; published edition, sanitized HTML with `{site}`/`{email}`/`{phone}` placeholders the site fills |
| Admin site documents | `GET /api/admin/content/documents`, `GET`/`POST …/{kind}/versions`, `GET`/`PUT`/`DELETE …/versions/{id}`, `POST …/versions/{id}/publish` | `CanManageContent`; one draft per document, publish with «Редакція від» |
| Site gallery | `GET /api/content/{lang}/gallery` | Anonymous; photos and YouTube videos in site order, `imageUrl` is the CDN picture or YouTube poster |
| Admin site gallery | `GET /api/admin/content/gallery`, `POST …/photos`, `POST …/videos`, `GET …/youtube-preview?url=`, `PUT`/`DELETE …/items/{id}`, `PUT …/order` | `CanManageContent`; every change answers with the whole gallery |
| Quick order | `POST /api/orders` | Anonymous |
| Session | cookie and mobile login, refresh, logout, current user | Login and refresh anonymous and rate-limited; see Identity contract |
| Sales catalog | `POST /api/sales/{lang}/catalog/products` | Any signed-in user |

For catalog read APIs, `lang` accepts `uk` or `ru`. The category collection is a navigation tree with no paging: use the server-provided `id`, `slug`, `parentId`, `sortOrder`, `level`, and `children` fields; do not reconstruct order or hierarchy from names. Category names are already localized for `lang`; category details add the path from the root to the current node. Product page size is capped at 100. The server currently normalizes invalid lower page values at runtime, but clients must always send `page >= 1` and `pageSize` from 1 to 100.

The Admin category list is a flat hierarchy without paging, ordered by depth-first traversal. It returns the source `name`, both localized short names, `productTypeIdOneC`, and structural fields; build the admin UI tree by `parentId` and preserve the provided order. The individual item route uses numeric `id`, not `slug`.
