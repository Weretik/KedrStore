# Admin order workspace API contract

**Status:** Operations implemented in commit `b3797c1`. [OpenAPI 0.1.0](../../../../contracts/sales/admin-order-workspace.openapi.yaml) is included in the aggregated [contract](../../../../contracts/openapi.yaml). The existing `admin-order-one-c-sync.openapi.yaml` is unchanged.

| operationId | Method/path | Request/response |
| --- | --- | --- |
| `getAdminWorkspaceOrders` | GET `/api/admin/order-workspace/orders` | `searchTerm?,counterpartyId?,sort?,page?,pageSize?` → `pagedInfo, value[]` |
| `getAdminWorkspaceOrderById` | GET `/api/admin/order-workspace/orders/{orderId}` | number, customer, lines, amount, date, sync |
| `quoteAdminWorkspaceOrder` | POST `/api/admin/order-workspace/quote` | `counterpartyId, lines[{productId,quantity}]` → lines with unitPrice/lineAmount, totalAmount, quoteFingerprint |
| `createAdminWorkspaceOrder` | POST `/api/admin/order-workspace/orders` | same set + `quoteFingerprint`, optional comment, `Idempotency-Key` UUID → `orderId,orderNumber,totalAmount,syncStatus` |

The list has page 1.., pageSize 1..100, defaults 1/20; `sort` is the R-003 whitelist; `searchTerm` is trimmed and maxLength 100. A list row is `orderId,orderNumber,counterpartyId,counterpartyName,createdAtUtc,lineCount,totalAmount,syncStatus`. Details contain saved `productId,productName,quantity,amount`. The server does not accept price or amount in the create body. `counterpartyId` in GET is an exact filter for customer details.

All operations are anonymous at this stage; 400 validation, 404 detail/customer, 409 fingerprint or idempotency conflict, 500 without internal data. Create returns 201 for a new record and 200 for an idempotent replay. `quoteFingerprint` is an irreversible fingerprint of the normalized set, customer, and server-calculated price values, without trusting client-submitted amount; it confirms the displayed price set but is not authorization. The server always recalculates actual prices.

If quantity exceeds current `Product.Stock`, quote returns the expected HTTP 400 with `code=unavailable_product` and a message with product name, available quantity, and requested quantity. `WorkspaceException` passes through the Mediator pipeline to the controller without being logged as an unknown error.
