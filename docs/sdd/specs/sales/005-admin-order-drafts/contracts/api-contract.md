# Планований API-контракт чернеток

**Статус:** операції реалізовано в робочому дереві. [OpenAPI 0.1.0](../../../../contracts/sales/admin-order-drafts.openapi.yaml) включено до агрегованого `docs/sdd/contracts/openapi.yaml`. Усі маршрути — нові, окремі від `/api/admin/orders`.

| operationId | Method/path | Поведінка |
| --- | --- | --- |
| `getAdminOrderDrafts` | GET `/api/admin/order-workspace/drafts` | `searchTerm?,counterpartyId?,sort?,page?,pageSize?` → `pagedInfo,value[]` |
| `createAdminOrderDraft` | POST `/api/admin/order-workspace/drafts` | `title,note?,counterpartyId,lines[{productId,quantity}]` → 201, draftId |
| `getAdminOrderDraftById` | GET `/api/admin/order-workspace/drafts/{draftId}` | повні дані, актуальна quote, version |
| `updateAdminOrderDraft` | PUT `/api/admin/order-workspace/drafts/{draftId}` | повна заміна редагованих полів + concurrency token → повні дані |
| `deleteAdminOrderDraft` | DELETE `/api/admin/order-workspace/drafts/{draftId}` | concurrency token → 204 |

Рядок списку: `draftId,title,counterpartyId,counterpartyName,updatedAtUtc,lineCount`. За доступної ціни деталі містять актуальний розрахунок. Якщо після збереження ціна, товар, залишок або клієнт недоступні, GET detail однаково повертає збережені поля, `pricingStatus=Unavailable`, `pricingErrorCode` і `null` для цін/суми. Список не виконує N+1 розрахунків цін.

Помилки: 400 validation, 401/403 access, 404 невідома чернетка/клієнт, 409 stale version, 500 safe error. `title` trim, 1..200; `note` maxLength=2000. Редагування/видалення не створює Order та 1C sync. Авторизація — `CanManageOrders`; POST вимагає UUID `Idempotency-Key`: той самий ключ і тіло повертають той самий draft, інше тіло — 409.
