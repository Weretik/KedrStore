# API-контракт робочої області замовлень

**Статус:** операції реалізовано в коміті `b3797c1`. [OpenAPI 0.1.0](../../../../contracts/sales/admin-order-workspace.openapi.yaml) включено до агрегованого [контракту](../../../../contracts/openapi.yaml). Чинний `admin-order-one-c-sync.openapi.yaml` не змінено.

| operationId | Method/path | Запит/відповідь |
| --- | --- | --- |
| `getAdminWorkspaceOrders` | GET `/api/admin/order-workspace/orders` | `searchTerm?,counterpartyId?,sort?,page?,pageSize?` → `pagedInfo, value[]` |
| `getAdminWorkspaceOrderById` | GET `/api/admin/order-workspace/orders/{orderId}` | номер, клієнт, позиції, сума, дата, sync |
| `quoteAdminWorkspaceOrder` | POST `/api/admin/order-workspace/quote` | `counterpartyId, lines[{productId,quantity}]` → позиції з unitPrice/lineAmount, totalAmount, quoteFingerprint |
| `createAdminWorkspaceOrder` | POST `/api/admin/order-workspace/orders` | той самий набір + `quoteFingerprint`, optional comment, `Idempotency-Key` UUID → `orderId,orderNumber,totalAmount,syncStatus` |

Список має page 1.., pageSize 1..100, defaults 1/20; `sort` — whitelist R-003; `searchTerm` trim і maxLength 100. Рядок списку: `orderId,orderNumber,counterpartyId,counterpartyName,createdAtUtc,lineCount,totalAmount,syncStatus`. Деталі містять збережені `productId,productName,quantity,amount`. Сервер не приймає ціну чи суму у create body. `counterpartyId` у GET є точним фільтром для деталей клієнта.

Усі операції: bearer авторизація чинного Admin transport, `CanManageOrders`; 400 validation, 401/403 access, 404 detail/customer, 409 fingerprint або idempotency conflict, 500 без внутрішніх даних. Create повертає 201 для нового запису та 200 для ідемпотентного повтору. `quoteFingerprint` є незворотним відбитком нормалізованого набору, клієнта та значень цін із серверного розрахунку, без довіри до переданої клієнтом суми; відбиток підтверджує бачений набір цін, але не є засобом авторизації; сервер завжди перераховує фактичні ціни.
