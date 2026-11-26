# Site quick order and «Замовлення з сайту»

**Static code check:** 2026-11-26. **Module:** Sales. **Specs:** [sales 009 — site orders in the Admin](../../specs/sales/009-site-orders-in-admin/README.md), [platform 006, R-007](../../specs/platform/006-transactional-outbox/README.md).

Public `POST /api/orders`: a buyer without an account sends a name, a phone and cart lines. The order is kept in `sales."QuickOrders"` together with a Telegram outbox message in one save, and sent to the managers' Telegram chat as an Excel file. People see the order by its number, the first 8 hex digits of the id (`№ 52992220`): on the site after sending, in the Telegram caption, in the file name `order_52992220.xlsx` and its «Заявка» cell, and in the Admin, where search finds it. Anonymous, rate-limited (`QuickOrder`: 60 per 10 minutes per calling address). The Admin page «Замовлення з сайту» lists the orders, opens one, shows when Telegram has not got it and why, and sends a failed one again.

## Structure and relationships

**Place.** [QuickOrdersEndpoints](../../../../src/Modules/Sales/Sales.Api/Endpoints/QuickOrders/QuickOrdersEndpoints.cs) maps the request to `PlaceQuickOrderCommand`. [PlaceQuickOrderValidator](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/Commands/PlaceQuickOrder/PlaceQuickOrderValidator.cs) checks it (400 with field errors `FirstName`, `Phone`, `Lines`; name ≤ 30, phone ≤ 20, product id ≤ 64, title ≤ 500, price ≥ 0, quantity 1–1000). [PlaceQuickOrderHandler](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/Commands/PlaceQuickOrder/PlaceQuickOrderHandler.cs) builds the [QuickOrder](../../../../src/Modules/Sales/Sales.Domain/QuickOrders/QuickOrder.cs) aggregate (root; lines `QuickOrderItem` change only through it; `Total` = Σ unit price × quantity) and saves it with its `QuickOrderPlaced` message through [QuickOrderStore](../../../../src/Modules/Sales/Sales.Infrastructure/Persistence/QuickOrders/QuickOrderStore.cs). Once the input is valid, the save and the attempt after it ignore the request's cancellation token: a buyer, the site or the proxy that stops waiting cannot cancel a valid order (spec 009, R-002).

**Telegram delivery.** [QuickOrderTelegramSender](../../../../src/Modules/Sales/Sales.Infrastructure/QuickOrders/QuickOrderTelegramSender.cs) sends the file and records on the order `TelegramSentAtUtc`, or `TelegramLastError` and `TelegramLastAttemptAtUtc` after a failed attempt (for example «Forbidden: bot was kicked from the group chat»). The outbox retries 1, 5, 15, 30 minutes, then hourly, ten attempts in all (about 7 hours); after the tenth [QuickOrderTelegramGiveUpHandler](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/Messages/QuickOrderTelegramGiveUpHandler.cs) sets `TelegramFailedAtUtc`. No Telegram alert is sent for a given-up quick order: the same broken chat would swallow it; the Admin list is the signal. Retries run in the scheduled «Досилання повідомлень» task, which must be on in «Фонові задачі».

**Admin.** Group `api/admin/quick-orders` (tag `AdminQuickOrders`, policy `CanManageOrders`):

| Endpoint | operationId | What it does | Rate limit |
| --- | --- | --- | --- |
| `GET /api/admin/quick-orders?page&pageSize&search&delivery` | `getAdminQuickOrders` | Page newest first; `search` matches every word against name or phone, and a word that is an order number (the first 8 hex digits of the id, shown on the site, in Telegram and in the Admin) or a whole id against the id as a range on the primary key; `delivery=problem` keeps orders not delivered with an error or given up | global |
| `GET /api/admin/quick-orders/{orderId}` | `getAdminQuickOrderById` | Buyer, lines with amounts, total, delivery state; 404 when unknown | global |
| `POST /api/admin/quick-orders/{orderId}/telegram-resend` | `resendAdminQuickOrderToTelegram` | Only a `failed` order: puts its outbox message back in the queue with attempts from zero (`OutboxWriter.RequeueAsync`) and clears the failure in one transaction, 202; `sent` → 409 `already_sent`, still retrying → 409 `delivery_in_progress`; a parallel resend loses on `xmin` → 409 | `HeavyOperation` |

`telegramStatus` on the wire is `sent`, `pending` or `failed`. Reads go through [QuickOrderReadService](../../../../src/Modules/Sales/Sales.Infrastructure/Persistence/QuickOrders/QuickOrderReadService.cs) as `AsNoTracking` projections paged in SQL, never through the aggregate.

**Repeats ([idempotency rules](../../standards/idempotency-rules.md)).** The site sends an optional `Idempotency-Key` made once per order attempt and reuses it when it sends the same order again (a second press, a retry after a timeout). The key becomes the order id and the outbox message id, so both primary keys reject a second copy: a repeat, also one that loses the race, answers 200 with the first order id and sends nothing to Telegram again; the same key with other data answers 409 (problem `conflict`, detail «Це замовлення вже надіслано з іншими даними. Оновіть сторінку.»). Repeats are recognised for as long as the order is kept. Without the header every request is a new order. The resend is a state transition: only `failed` can be resent.

**Data.** Tables `QuickOrders` and `QuickOrderItems` in schema `sales` ([data map](../../architecture/data-model/sales-orders.md#site-orders)). Name and phone are personal data: kept with the order like Sales orders, never written to logs (log lines carry the order id, line count, total, and for a resend the staff user id). Processed outbox rows are still deleted after 30 days; the order stays. Migration `AddQuickOrders` copied the orders that existed only in the outbox at release.

**To 1C (spec 009, R-013).** The same save stores a second outbox message [QuickOrderToOneC](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/Messages/QuickOrderToOneC.cs) under `QuickOrder.OneCOrderMessageId`. [CreateOneCOrderFromQuickOrderHandler](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/ToOneC/CreateOneCOrderFromQuickOrderHandler.cs) makes an ordinary Sales [Order](../../../../src/Modules/Sales/Sales.Domain/Orders/Order.cs) on the counterparty from `Sales:SiteOrders:CounterpartyId` ([SiteOrderOneCOptions](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/ToOneC/SiteOrderOneCOptions.cs), default `000001588` «РОЗНИЦА ХАЛТУРИНА»): the site lines at the buyer's prices, comment from [QuickOrderOneCComment](../../../../src/Modules/Sales/Sales.Application/Features/QuickOrders/ToOneC/QuickOrderOneCComment.cs) (`Сайт № 52992220, Іван, +380…, 26.11.2026 12:15`, Kyiv time). `IOrderCreationStore.AddFromOutboxAsync` saves it inside the dispatch transaction to get its number (`SO-yyyyMMdd-id`) and adds its `OrderPlaced` message; the site order keeps the link `OneCOrderId` (`LinkOneCOrder`, set once), so a repeated message makes nothing. From there it is an [admin order](admin-orders.md): sent to 1C in the 1C lane and sending window, visible in «Замовлення» with its 1C status. A missing counterparty or a non-numeric product id fails the attempt; the outbox retries and gives up after ten. Site orders placed before 2026-11-26 have no message and are not sent.

## Code structure

```text
src/Modules/Sales/
├── Sales.Domain/QuickOrders/                             # QuickOrder (root), QuickOrderItem, ids, QuickOrderTelegramStatus, QuickOrderErrors (six files, flat)
├── Sales.Application/Abstractions/QuickOrders/           # IQuickOrderStore (write), IQuickOrderReadService (read)
├── Sales.Application/Features/QuickOrders/
│   ├── Commands/PlaceQuickOrder/                         # command, validator, handler
│   ├── Commands/ResendQuickOrderToTelegram/              # command, validator, handler, result
│   ├── Queries/GetQuickOrderList/ and GetQuickOrderById/ # queries, validators, handlers, DTOs
│   ├── Messages/                                         # QuickOrderPlaced (outbox type "sales.quick-order-placed"), QuickOrderLine, its give-up handler; QuickOrderToOneC ("sales.quick-order-to-1c")
│   └── ToOneC/                                           # handler that makes the 1C order, comment, options (counterparty)
├── Sales.Infrastructure/Persistence/QuickOrders/         # EF configurations, store, read service
├── Sales.Infrastructure/QuickOrders/                     # Excel file, Telegram caption and sender
└── Sales.Api/Endpoints/QuickOrders/                      # route groups; PlaceQuickOrder/, GetQuickOrderList/, GetQuickOrderById/, ResendQuickOrderToTelegram/
```

## Structure diagram

```mermaid
flowchart LR
    A["POST /api/orders"] --> B["PlaceQuickOrderHandler"]
    B --> C[("sales.QuickOrders + QuickOrderItems")]
    B --> M[("sales.OutboxMessages")]
    B -->|"right after the commit"| D["Outbox dispatcher"]
    E["«Досилання повідомлень» task"] --> D
    M --> D
    D --> F["QuickOrderTelegramSender"]
    F --> G["Excel file + caption to the managers' chat"]
    F -->|"sent / last error"| C
    D -->|"10th failure"| H["QuickOrderTelegramGiveUpHandler"] -->|"failed"| C
    I["Admin «Замовлення з сайту»"] -->|"list, detail"| C
    I -->|"resend failed"| M
    D --> J["CreateOneCOrderFromQuickOrderHandler"]
    J -->|"Order on 000001588 + OrderPlaced"| K[("sales.Orders")]
    J -->|"OneCOrderId"| C
    K -->|"1C lane"| L["SendOrderToOneCHandler → 1C"]
```

## Tests

- [QuickOrderTests](../../../../tests/UnitTests/Sales/QuickOrders/QuickOrderTests.cs): factory errors, trimmed buyer, total, delivery state transitions, resend only after give-up, repeat comparison.
- [PlaceQuickOrderRepeatTests](../../../../tests/UnitTests/Sales/QuickOrders/PlaceQuickOrderRepeatTests.cs): the key is the order id, the order and its message are saved together, a cancelled request still saves and sends, a repeat returns the id without saving or sending, other data is a conflict, the loser of a race answers with the winner.
- [ResendQuickOrderToTelegramTests](../../../../tests/UnitTests/Sales/QuickOrders/ResendQuickOrderToTelegramTests.cs): failed → queued once, sent → 409, unknown → 404, a lost row version → 409.
- [QuickOrderNumberSearchTests](../../../../tests/UnitTests/Sales/QuickOrders/QuickOrderNumberSearchTests.cs): the number of an id, the id range of a number or a whole id, other words.
- [QuickOrderTelegramCaptionTests](../../../../tests/UnitTests/Sales/QuickOrders/QuickOrderTelegramCaptionTests.cs): HTML escaping, order id, Kyiv time, file name.
- [QuickOrderApiTests](../../../../tests/IntegrationTests/Sales/QuickOrders/QuickOrderApiTests.cs): sent at once while Telegram works, stored and sent by the next dispatch while it is down, 400 and no row for invalid input, repeats and parallel copies store one order, the same key with other lines answers 409.
- [QuickOrderAdminApiTests](../../../../tests/IntegrationTests/Sales/QuickOrders/QuickOrderAdminApiTests.cs): the real sender over a fake Telegram API — sent order listed with lines; a refused order shows the reason, becomes `failed` after ten attempts, is resent once and then delivered; search by name, phone, order number and whole id; 404 and 400; 401/403 without the orders policy.
- [CreateOneCOrderFromQuickOrderTests](../../../../tests/UnitTests/Sales/QuickOrders/ToOneC/CreateOneCOrderFromQuickOrderTests.cs) and [QuickOrderOneCCommentTests](../../../../tests/UnitTests/Sales/QuickOrders/ToOneC/QuickOrderOneCCommentTests.cs): order on the retail counterparty with lines, amounts and comment, link, no second order on a repeat, missing counterparty or non-numeric product fails the attempt.
- [QuickOrderToOneCPostgresTests](../../../../tests/IntegrationTests/Sales/QuickOrders/QuickOrderToOneCPostgresTests.cs): on PostgreSQL, the Telegram and 1C messages in one batch make one linked order that the 1C lane sends; without the counterparty nothing is made.
- Filter `FullyQualifiedName~QuickOrder` (unit and integration).

## Related sources

[Contract](../../contracts/openapi.json) · [Outbox](../../architecture/platform/building-blocks/infrastructure.md) · [Sales orders](admin-orders.md) · [rate limits](../../standards/rate-limit-rules.md).
