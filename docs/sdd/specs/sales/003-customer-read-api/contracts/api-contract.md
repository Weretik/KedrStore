# Customer read API — API contract

**Status:** agreed

## Consumers and compatibility

The consumer is the Sales administration frontend. This additive read API does not change Sales catalog, order creation/delivery, customer synchronization, or Identity. Both operations allow anonymous access during the current rollout stage. Any caller can read the approved projections of active Sales counterparties; no row-level assignment restriction applies.

## List customers

`GET /api/admin/customers`

Operation ID: `getAdminCustomers`.

Query parameters:

| Parameter | Required | Rules |
| --- | --- | --- |
| `page` | no | Integer `>= 1`; default `1` |
| `pageSize` | no | Integer from `1` through `100`; default `20` |

No search, deletion-state, price-type, or custom-sort filter is included. Results contain active counterparties only and are ordered by `name` ascending and then `counterpartyId` ascending.

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
      "counterpartyId": "000000000",
      "name": "Клієнт",
      "phone": "+380000000000"
    }
  ]
}
```

The list intentionally excludes email, `IdentityUserId`, price rules, audit/deletion fields, order IDs, and order details.

## Get customer detail

`GET /api/admin/customers/{counterpartyId}`

Operation ID: `getAdminCustomerById`. `counterpartyId` is a non-blank, URL-encoded Sales/1C identifier of at most 64 characters and is matched exactly. Unknown and soft-deleted counterparties return `404`.

Successful response:

```json
{
  "counterpartyId": "000000000",
  "name": "Клієнт",
  "phone": "+380000000000",
  "email": "client@example.com",
  "defaultPriceTypeId": 1,
  "categoryPriceTypes": [
    {
      "categoryId": 5513,
      "priceTypeId": 2
    }
  ]
}
```

Category price rules are ordered by `categoryId` ascending. Detail excludes `IdentityUserId`, soft-delete/audit fields, order IDs, order counts, and order details.

## Customer page composition

The customer detail response does not embed orders. When the administration frontend opens a customer page, it requests these resources independently and may run them in parallel:

```text
GET /api/admin/customers/{counterpartyId}
GET /api/admin/orders?counterpartyId={counterpartyId}&page=1&pageSize=20
```

The order collection remains independently paged and uses the manager-order list schema from feature 002. No duplicate nested customer-orders route is introduced.

## Errors and access

- `200 OK`: filled or empty list, or existing active-customer detail.
- `400 Bad Request`: invalid paging or counterparty identifier input.
- `404 Not Found`: no active counterparty exists for a valid `counterpartyId`.

Neither operation requires authentication or authorization during the current rollout stage.

Read operations have no idempotency requirement and never call 1C/Identity or mutate persistence.

## OpenAPI and rollout

Before transport implementation, create `docs/sdd/contracts/sales/customer-read.openapi.yaml` and add `$ref` entries for both operations to `docs/sdd/contracts/openapi.yaml`. Deploy implementation and aggregate contract together and verify runtime OpenAPI against this source contract.
