# Order drafts API contract

**Status:** Operations implemented in commit `b3797c1`. [OpenAPI 0.1.0](../../../../contracts/sales/admin-order-drafts.openapi.yaml) is included in aggregated `docs/sdd/contracts/openapi.yaml`. All routes are new and separate from `/api/admin/orders`.

| operationId | Method/path | Behavior |
| --- | --- | --- |
| `getAdminOrderDrafts` | GET `/api/admin/order-workspace/drafts` | `searchTerm?,counterpartyId?,sort?,page?,pageSize?` → `pagedInfo,value[]` |
| `createAdminOrderDraft` | POST `/api/admin/order-workspace/drafts` | `title,note?,counterpartyId,lines[{productId,quantity}]` → 201, draftId |
| `getAdminOrderDraftById` | GET `/api/admin/order-workspace/drafts/{draftId}` | full data, current quote, version |
| `updateAdminOrderDraft` | PUT `/api/admin/order-workspace/drafts/{draftId}` | replace editable fields + concurrency token → full data |
| `deleteAdminOrderDraft` | DELETE `/api/admin/order-workspace/drafts/{draftId}` | concurrency token → 204 |

A list row is `draftId,title,counterpartyId,counterpartyName,updatedAtUtc,lineCount`. When prices are available, details include the current quote. If price, product, stock, or customer becomes unavailable after saving, GET detail still returns saved fields, `pricingStatus=Unavailable`, `pricingErrorCode`, and `null` prices/amount. The list does not run N+1 price calculations.

Errors: 400 validation, 401/403 access, 404 unknown draft/customer, 409 stale version, 500 safe error. `title` is trimmed, 1..200; `note` maxLength=2000. Editing/deleting does not create an Order or 1C sync. Authorization is `CanManageOrders`; POST requires a UUID `Idempotency-Key`: same key and body return the same draft, different body returns 409.
