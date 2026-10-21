# Планований API-контракт

**Статус:** рішення SDD; операцій у коді й поточному OpenAPI ще немає. Плановий [OpenAPI 0.1.0](../../../../contracts/sales/admin-order-workspace.openapi.yaml) створено для SDD. До реалізації його не включено в поточний агрегований `docs/sdd/contracts/openapi.yaml`; підключити в тій самій зміні, що й API. Не змінювати чинний `admin-order-one-c-sync.openapi.yaml`.

| operationId | Method/path | Запит/відповідь |
| --- | --- | --- |
| `getAdminWorkspaceOrders` | GET `/api/admin/order-workspace/orders` | `searchTerm?,counterpartyId?,sort?,page?,pageSize?` → `pagedInfo, value[]` |
| `getAdminWorkspaceOrderById` | GET `/api/admin/order-workspace/orders/{orderId}` | номер, клієнт, позиції, сума, дата, sync |
| `quoteAdminWorkspaceOrder` | POST `/api/admin/order-workspace/quote` | `counterpartyId, lines[{productId,quantity}]` → позиції з unitPrice/lineAmount, totalAmount, quoteFingerprint |
| `createAdminWorkspaceOrder` | POST `/api/admin/order-workspace/orders` | той самий набір + `quoteFingerprint`, optional comment, `Idempotency-Key` UUID → `orderId,orderNumber,totalAmount,syncStatus` |

Список має page 1.., pageSize 1..100, defaults 1/20; `sort` — whitelist R-003; `searchTerm` trim і maxLength 100. Рядок списку: `orderId,orderNumber,counterpartyId,counterpartyName,createdAtUtc,lineCount,totalAmount,syncStatus`. Деталі містять збережені `productId,productName,quantity,amount`. Сервер не приймає ціну чи суму у create body. `counterpartyId` у GET є точним фільтром для деталей клієнта.

Усі операції: bearer/cookie session за чинним Admin transport, `CanManageOrders`; 400 validation, 401/403 access, 404 detail/customer, 409 fingerprint або idempotency conflict, 500 без внутрішніх даних. Для create визначити точну семантику 201 та 200 на ідемпотентний повтор у OpenAPI. `quoteFingerprint` має бути незворотним відбитком нормалізованого набору, клієнта та значень цін із серверного розрахунку, без довіри до переданої клієнтом суми; відбиток підтверджує бачений набір цін, але не є засобом авторизації; сервер завжди перераховує фактичні ціни.
