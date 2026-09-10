# Manager order read API — API contract

**Status:** agreed

## Consumers and compatibility

The consumer is the Sales administration frontend. This additive read API does not change `POST /api/admin/orders`, the sync-status/retry routes, or Catalog `POST /api/orders`. Every operation requires `PolicyNames.CanManageOrders`, which permits Manager and Admin. An authorized caller can read all Sales manager orders; no row-level assignment restriction applies.

## List manager orders

`GET /api/admin/orders`

Operation ID: `getAdminOrders`.

Query parameters:

| Parameter | Required | Rules |
| --- | --- | --- |
| `counterpartyId` | no | Non-blank Sales/1C counterparty identifier, maximum 64 characters; exact match |
| `page` | no | Integer `>= 1`; default `1` |
| `pageSize` | no | Integer from `1` through `100`; default `20` |

No search, date, sync-status, amount, or custom-sort filter is included. Results are ordered by `createdAtUtc` descending and then `orderId` descending. An unknown, deleted, or active counterparty ID with no matching orders returns `200` with an empty page. Historical orders of a soft-deleted counterparty remain visible.

Successful response:

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
      "orderId": 12345,
      "orderNumber": "SO-20260817-12345",
      "counterpartyId": "000000000",
      "counterpartyName": "Клієнт",
      "createdAtUtc": "2026-08-17T10:30:00Z",
      "lineCount": 2,
      "totalAmount": 25000.00,
      "syncStatus": "Accepted",
      "oneCDocumentNumber": "000000123"
    }
  ]
}
```

`lineCount` is the number of persisted `OrderLine` rows. `totalAmount` is the sum of their `amount` values. The list does not contain comments, line arrays, contact details, sync diagnostics, or retry/notification history.

## Get manager-order detail

`GET /api/admin/orders/{orderId}`

Operation ID: `getAdminOrderById`. `orderId` is the positive local `OrderId` (`int64`).

Successful response:

```json
{
  "orderId": 12345,
  "orderNumber": "SO-20260817-12345",
  "createdAtUtc": "2026-08-17T10:30:00Z",
  "counterparty": {
    "counterpartyId": "000000000",
    "name": "Клієнт",
    "phone": "+380000000000"
  },
  "comment": "optional manager comment",
  "lines": [
    {
      "productId": "000000001",
      "productName": "Product",
      "quantity": 2,
      "amount": 25000.00
    }
  ],
  "totalAmount": 25000.00,
  "sync": {
    "status": "Accepted",
    "oneCDocumentNumber": "000000123",
    "acceptedAtUtc": "2026-08-17T10:31:00Z"
  }
}
```

Lines are ordered by their persisted `OrderLineId` ascending. `OrderLine.amount` is the total for the complete line quantity, not a unit price; the API does not manufacture a unit-price field. `totalAmount` is the sum of line amounts. The counterparty summary uses the currently stored name and phone and remains available for historical orders when the counterparty is soft-deleted.

The response excludes email, `IdentityUserId`, soft-delete/audit fields, SOAP bodies, hashes, HTTP/error diagnostics, attempt details, notification state, and retry-audit records.

## Errors and security

- `200 OK`: filled or empty list, or existing detail.
- `400 Bad Request`: invalid paging, counterparty identifier, or order identifier input.
- `401 Unauthorized`: missing or invalid authentication.
- `403 Forbidden`: authenticated caller does not satisfy `PolicyNames.CanManageOrders`.
- `404 Not Found`: no manager order exists for a valid `orderId`.

Read operations have no idempotency requirement and never call 1C or mutate persistence.

## OpenAPI and rollout

Before transport implementation, create `docs/sdd/contracts/sales/manager-order-read.openapi.yaml` and add `$ref` entries for both operations to `docs/sdd/contracts/openapi.yaml`. Deploy implementation and aggregate contract together and verify runtime OpenAPI against this source contract.
