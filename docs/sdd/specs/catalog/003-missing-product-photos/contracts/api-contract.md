# Missing product photos — API and integration contract

**Status:** agreed

## HTTP operation

```http
GET /api/admin/products/missing-photos?page=1&pageSize=20
```

- **Operation ID:** `getAdminProductsWithUnavailablePhotos`
- **Security:** explicitly anonymous (`AllowAnonymous`, no security scheme)
- **Idempotency:** safe read-only GET; no idempotency key
- **Data source:** PostgreSQL only; no CDN request occurs in the request path
- **Ordering:** product `id` ascending
- **Paging:** `page` defaults to `1`; `pageSize` defaults to `20` and is limited
  to `1..100`

The route is additive under the existing `AdminProductsController`. Before API
implementation, add it to
`docs/sdd/contracts/catalog/products.openapi.yaml` and add the operation path
reference to `docs/sdd/contracts/openapi.yaml`.

## Response

`200 OK` returns the established `PagedResult` envelope. Each row contains all
fields of the existing `AdminProduct` contract plus:

| Field | Type | Meaning |
| --- | --- | --- |
| `photoAvailable` | boolean | Always `false` for this endpoint. |
| `photoStatus` | enum | `Missing`, `InvalidContentType`, or `CheckFailed`. |
| `photoCheckedAtUtc` | string/date-time | Completion time of the latest check. |
| `photoHttpStatusCode` | integer or null | Final HTTP status when a response existed. |

The existing `photo` field remains the URL that was checked. Internal diagnostic
messages, exception text, headers, and response bodies are not returned.

Example:

```json
{
  "pagedInfo": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1,
    "totalRecords": 1
  },
  "value": [
    {
      "id": 8190,
      "nameUk": "Товар",
      "nameRu": "Товар",
      "productSlug": "tovar-8190",
      "photo": "https://images-kedr.cdn.express/products/8190.jpg",
      "categoryId": 10,
      "inStock": true,
      "isSale": false,
      "isNew": false,
      "exportToSite": false,
      "price": 1000.00,
      "stock": 2.00,
      "quantityInPack": 1,
      "photoAvailable": false,
      "photoStatus": "Missing",
      "photoCheckedAtUtc": "2026-09-23T12:00:00Z",
      "photoHttpStatusCode": 404
    }
  ]
}
```

An empty result is `200` with an empty `value` and zero totals. Invalid paging
returns the established `400` validation-error body. The operation does not use
`404`, and CDN failures never become an HTTP error for this read endpoint.

## Host.Jobs CLI contract

```text
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=check-product-photos
```

The command accepts no `rootId`, runs one full active-product cycle, logs its
summary, and exits `0` when orchestration and persistence complete. Per-product
remote failures are saved as `CheckFailed` and do not make the process fail.
Invalid configuration, database failure, cancellation, or an unhandled failure
returns the host's non-zero failure exit.

## Cloud Run integration contract

The workflow creates or updates `catalog-product-photo-check` from the current
Host.Jobs image with the CLI argument above, one task, parallelism one, platform
retries zero, `30m` timeout, and only the Catalog database secret. Deployment
does not execute or schedule it.

## Compatibility and rollout

The HTTP route, table, command, and Cloud Run Job are additive. Existing product
routes and DTOs do not change. Consumers may adopt the endpoint after the first
completed job run; before that, `Unknown` rows are intentionally absent.
Authorization is explicitly deferred and will be a future contract change.
