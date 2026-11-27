# 009 — Site orders stored and shown in the Admin

- **Module:** Sales (extends the [site quick order](../../../features/sales/quick-order.md))
- **Consumer:** React Admin page «Замовлення з сайту» and the public site (kedr-web spec `admin/site-orders/001-site-orders-list`)
- **Status:** Accepted by the owner 2026-11-24; implemented 2026-11-24 (branch `claude/project-thread-0x1stn`)
- **Source:** owner request in the project thread of 2026-11-24, after a buyer saw «Підтвердження не отримано» on 24.11 about 09:45 Kyiv and the order did not reach Telegram

## Goal and scope

Every order a buyer sends from the site is kept as a site order in the database, whether or not Telegram delivers it. Managers and the owner see the list in the Admin, open an order to see who ordered what, and see when Telegram delivery has a problem and why. Telegram delivery keeps retrying, and a failed delivery can be sent again from the Admin.

Given a buyer sends an order from the site, when Telegram is down, the bot was removed from the group, or the site stopped waiting for the answer after the order was saved, then the order is in «Замовлення з сайту» and the managers can still work it.

**Out of scope:** statuses of handling an order (new, in work, done), editing or deleting a site order, a menu badge or «Огляд» counter, changing the Telegram message format, a new Telegram group (an operations step, not code).

## Rules

- **R-001.** A valid `POST /api/orders` (validator also caps product id at 64, title at 500 characters and rejects a negative price, the new column limits) stores a site order (`QuickOrder` aggregate: id, first name, phone, lines with product id, title, unit price and quantity, total, placed time) and its `QuickOrderPlaced` outbox message in one `SaveChanges`.
- **R-002.** Once validation passed, the save does not take the request's cancellation token: a buyer, the site or the proxy that stops waiting cannot cancel a half-made order. The answer for a cancelled request may be lost; the order stays.
- **R-003.** Idempotency stays as in the [quick order card](../../../features/sales/quick-order.md): the `Idempotency-Key` is the order id. A repeat is now found in `sales."QuickOrders"` (kept, not deleted after 30 days); the same key with other data answers 409 `conflict` with the existing detail. A copy that loses the race on the primary key answers with the winner.
- **R-004.** The total is computed by the aggregate from the lines (Σ unit price × quantity, two decimals); it is a copy of what the buyer saw, not a price check (prices are confirmed by the manager, as today).
- **R-005.** Telegram delivery state lives on the order: `TelegramSentAtUtc` when delivered; `TelegramLastError` and `TelegramLastAttemptAtUtc` after each failed attempt; `TelegramFailedAtUtc` when the outbox gave up (10 attempts, about 7 hours: 1, 5, 15, 30 minutes, then hourly). A successful delivery clears the error.
- **R-006.** `GET /api/admin/quick-orders` returns a page of site orders, newest first, with id, placed time, first name, phone, line count, total and the delivery state (`sent`, `pending`, `failed`) with the last error. It searches by name and phone (each word an escaped `ILIKE`) and filters `delivery=problem` (pending with an error, or failed).
- **R-007.** `GET /api/admin/quick-orders/{id}` returns the order with its lines and delivery state; an unknown id answers 404.
- **R-008.** `POST /api/admin/quick-orders/{id}/telegram-resend` is allowed only when delivery failed (`TelegramFailedAtUtc` set): it clears the failure and puts the order's outbox message back in the queue with attempts from zero (`OutboxWriter.RequeueAsync`, the message id stays the order id) in one transaction, answers 202 with the new state, and does not call Telegram in the request. Already sent → 409 `already_sent`; still retrying → 409 `delivery_in_progress`; unknown → 404. A repeat after success sees `pending` and answers 409 `delivery_in_progress` (no second message).
- **R-009.** The three Admin endpoints require policy `CanManageOrders` (Manager, Seo, Admin; Customer gets 403, anonymous 401).
- **R-010.** Site orders made before this release are copied once from `sales."OutboxMessages"` rows of type `sales.quick-order-placed` (the last 30 days) with their delivery state.
- **R-011.** Name and phone are personal data: they are never written to logs; the log line of a placed or resent order carries only the order id, the line count and the total.
- **R-012.** (2026-11-24, owner request) The order number people see is the first 8 hex digits of the order id in upper case (`QuickOrderNumberSearch.NumberOf`): the site shows it after sending, Telegram shows `№ 52992220` without the whole id, the Excel file is named `order_52992220.xlsx` and carries the number in its «Заявка» cell, the Admin shows it instead of the whole id. A list search word that is such a number (optionally with `№` or `#`), or a whole id, also matches the orders whose id starts with it, as a range on the primary key; the word still matches name and phone too, so a phone fragment of 8 digits keeps working. The site sends a random `Idempotency-Key`, so numbers of site orders differ; an order placed without a key gets a time-ordered id, and two such orders in the same minute can share a number (the search then shows both).

- **R-013.** (2026-11-26, owner request) Every new site order also becomes an ordinary Sales order that goes to 1C. The site order is saved with a second outbox message `QuickOrderToOneC` (type `sales.quick-order-to-1c`, id `QuickOrder.OneCOrderMessageId`) in the same save; its handler, run by the general outbox task (and once right after the request), makes an `Order` on the counterparty `Sales:SiteOrders:CounterpartyId` (default `000001588`, «РОЗНИЦА ХАЛТУРИНА») with the site lines at the prices the buyer saw (`Amount` = unit price × quantity, UAH retail), comment `Сайт № 52992220, Іван, +380…, 26.11.2026 12:15` (order number, name, phone, Kyiv time), and the order's own `OrderPlaced` message in the 1C lane, then links it (`QuickOrder.OneCOrderId`). A repeated message sees the link and makes nothing; a missing counterparty or a product id that is not a catalog number fails the attempt and the outbox retries. No `EmployeeId`: the current 1C method has no such field. Site orders placed before this release have no message id and are not sent. One dispatch transaction creates the `Order` root and changes the `QuickOrder` root (the link): the link must commit with the order, or a retried message would make a second 1C order; both roots are in Sales. The comment carries the buyer's name and phone into `sales."Orders"` and 1C (owner request); they are kept like other orders and still never logged.

- **R-014.** (2026-11-26, owner request) The buyer may leave an optional comment on the site order (`comment` in `POST /api/orders`, 800 characters at most, trimmed; a blank comment is stored as none; the validator and the aggregate both answer 400 `Comment` when it is longer). It is kept in `QuickOrders.Comment`, shown in the Telegram caption (`💬`, HTML-escaped) and the Excel file, returned by `GET /api/admin/quick-orders/{id}`, and appended to the 1C order comment as «. Коментар покупця: …» ([R-013](#rules)). The limit is below the 1000 characters of the 1C order comment, so the line with the buyer's comment always fits. The comment is personal data like the name and phone and is never logged ([R-011](#rules)); the same key with another comment is an idempotency conflict.
- **R-015.** (2026-11-27, owner request) A site order and the order made from it for 1C ([R-013](#rules)) name each other in the Admin reads. `GET /api/admin/quick-orders` and `GET /api/admin/quick-orders/{id}` return `oneCOrderId`, `oneCOrderNumber` (`SO-…`, as in «Історія замовлень») and `oneCDocumentNumber` (the 1C document number, once 1C accepts the order); all three are null until the order is made. `GET /api/admin/order-workspace/orders` and `GET /api/admin/order-workspace/orders/{orderId}` return `siteOrderId` and `siteOrderNumber` (the 8-character number of [R-012](#rules)); null for a manager's order. The link is the existing `QuickOrders.OneCOrderId` column; the reads look it up with a correlated subquery per row (primary key of `Orders`, index `IX_QuickOrders_OneCOrderId` the other way), both in the Sales read side, so no schema change and no new rate limit decision (cheap signed-in reads under the global limit).
## Scenarios

| ID | Scenario |
| --- | --- |
| **SC-001** | A valid order with Telegram working: one `QuickOrders` row, one outbox row, Telegram gets one file, the order shows `sent`. |
| **SC-002** | Telegram throws (bot removed from the group): the buyer still gets 200 with the order id; the order shows `pending` with the error text; after the 10th failed attempt it shows `failed`. |
| **SC-003** | The request is cancelled after validation (client abort): the order and its outbox row are still saved. |
| **SC-004** | Repeat with the same key and data → 200 with the same id, one row, one Telegram file; same key with other lines → 409; two parallel copies → one row. |
| **SC-005** | Admin list: newest first, paging, search «Ірина» and «4315238» find the order; `delivery=problem` shows only pending-with-error and failed orders. |
| **SC-006** | Admin detail returns lines and total; unknown id → 404. |
| **SC-007** | Resend of a failed order → 202, its outbox row is queued again, state `pending`; resend of a sent or pending order → 409; a second resend right after the first → 409, Telegram gets the order once. |
| **SC-008** | Customer role → 403 and anonymous → 401 on all three Admin endpoints. |
| **SC-009** | The migration copies existing quick-order outbox rows: a processed row becomes `sent`, a failed row `failed` with its error, a waiting row `pending`; running the copy again adds nothing. |
| **SC-010** | Domain: `Create` rejects no lines, an empty name or phone, a quantity ≤ 0 and a negative price with typed errors; the total is the sum of the lines; `RequestTelegramResend` works only from failed. |
| **SC-011** | Search by the order number, by `№` + number in lower case, and by the whole id finds exactly that order. |
| **SC-012** | A site order with the retail counterparty present: one `Orders` row on `000001588` with the comment and the lines, linked from the site order, its `OrderPlaced` message delivered to 1C; a repeated message adds nothing; without the counterparty the attempt fails and no order is made. |
| **SC-013** | A buyer writes a comment in the checkout: the order keeps it, Telegram and the Excel file show it, and the 1C order comment ends with «Коментар покупця: …»; no comment changes nothing; more than 800 characters answers 400. |

## Design

**Aggregate.** `QuickOrder` (aggregate root, `Sales.Domain/QuickOrders/`): `QuickOrderId` (`readonly record struct` over `Guid`, = the idempotency key or a new v7; items have `QuickOrderItemId`), derived `TelegramStatus` (`QuickOrderTelegramStatus`: `Pending`, `Sent`, `Failed`), `FirstName`, `Phone`, `PlacedAtUtc`, `Total`, lines `QuickOrderItem` (child entity, changed only through the root; `ProductId` stays the site's string as the buyer sent it, `Title`, `UnitPrice`, `Quantity`). Factory `Create(...)` returns `DomainResult<QuickOrder>` with `QuickOrderErrors`. Behaviour: `MarkSentToTelegram(now)`, `RecordTelegramFailure(error, now)`, `GiveUpTelegram(error, now)`, `RequestTelegramResend()` (`DomainResult`, only from failed). The name differs from the Application message line `QuickOrderLine` on purpose.

**Write side (CQRS).**
- `PlaceQuickOrderHandler` uses the `IQuickOrderStore` port (find by id, add the order with its outbox message); one `SaveChanges` with `CancellationToken.None` after validation (R-002); the duplicate key is caught as today (`UniqueViolation`). The attempt right after the commit stays.
- `QuickOrderTelegramSender` (outbox handler) sends as today, then loads the order in the same scope and calls `MarkSentToTelegram`; on an exception it records `RecordTelegramFailure` and returns `Failed(error)`. A message whose order is missing (should not happen after R-010) is still sent. Repeat: Telegram may get a second file only if the commit after a successful send fails, as today (at least once).
- `QuickOrderTelegramGiveUpHandler` (`IOutboxGiveUpHandler<QuickOrderPlaced>`) calls `GiveUpTelegram`. The general DeadLetter Telegram alert is not used for quick orders: the same broken chat would swallow it; the Admin list is the signal.
- `ResendQuickOrderToTelegram` command: state check (R-008); the store requeues the outbox row and saves the order (checked by `xmin`) in one transaction.

**Read side (CQRS).** `GetQuickOrderList` and `GetQuickOrderById` queries read `SalesDbContext` with `AsNoTracking` projections and page in SQL; they never load the aggregate.

**Data.** Schema `sales`, tables `QuickOrders` (PK `Id uuid`; `FirstName varchar(30)`, `Phone varchar(20)`, `PlacedAtUtc`, `Total numeric(18,2)`, `TelegramSentAtUtc`, `TelegramLastAttemptAtUtc`, `TelegramFailedAtUtc`, `TelegramLastError varchar(1000)`) and `QuickOrderItems` (PK `Id`, FK `QuickOrderId` cascade inside the aggregate, `ProductId varchar(64)`, `Title varchar(500)`, `UnitPrice numeric(18,2)`, `Quantity int`). Indexes: `IX_QuickOrders_PlacedAtUtc_Id` (list order), `gin_trgm_ops` on `FirstName` and on `Phone` (search), partial `IX_QuickOrders_Problem` on `PlacedAtUtc` where `TelegramSentAtUtc IS NULL AND TelegramLastError IS NOT NULL` (problem filter). Concurrency: the primary key guards the create; the outbox requeue is a conditional `UPDATE` and the order's transition is guarded by its `xmin` row version, so two parallel resends queue the message once. Migration is additive (expand only): the previous release keeps working because it only writes outbox rows; the copy (R-010) is an idempotent `INSERT … SELECT … ON CONFLICT DO NOTHING` from the outbox JSON. Rollback: revert the code; the tables can stay.

**HTTP contract** (`docs/sdd/contracts/openapi.json`, generated):

| Method and route | operationId | Answers | Rate limit |
| --- | --- | --- | --- |
| `GET /api/admin/quick-orders?page&pageSize&search&delivery` | `getAdminQuickOrders` | 200 page (`items`, `page`, `pageSize`, `totalCount`), 400 | global (signed-in cheap read) |
| `GET /api/admin/quick-orders/{id}` | `getAdminQuickOrderById` | 200, 404 | global |
| `POST /api/admin/quick-orders/{id}/telegram-resend` | `resendAdminQuickOrderToTelegram` | 202, 404, 409 | `HeavyOperation` (sends a message outside, as `retryAdminOrderSync`); idempotency by state check (R-008) |
| `POST /api/orders` | `createQuickOrder` | unchanged | `QuickOrder`, unchanged |

Tag `AdminQuickOrders`; FluentValidation with Ukrainian messages for list paging (`page ≥ 1`, `pageSize` 1–100), `search` ≤ 100 characters, `delivery` ∈ {`problem`}.

**Architecture questions.**
- Quality targets: [quality targets](../../../architecture/overview/quality-targets.md), system default (reads P95 ≤ 1 s, writes ≤ 2 s). Volume: a few orders a day, list of thousands of rows at most in years; the create gains one small insert.
- New library, database, external service or broker: ➖ none (Telegram already used). No ADR; C4 unchanged.
- Money, stock, limits, promo codes: ➖ the total is a copy for display, not a charge.
- Access: policy `CanManageOrders` on the group; no owner check (orders are shared by the sales team).
- User data from the token: resend logs the staff user id from the token; nothing about price or state comes from the request.
- Personal data: name and phone are kept with the order for the sales history (no automatic deletion, as Sales orders); added to the personal-data note in the feature card; never logged (R-011).
- Rate limit: see the contract table.
- Logs: `Quick order {QuickOrderId} placed with {LineCount} lines, total {Total}`; `Quick order {QuickOrderId} resent to Telegram by {UserId}`.
- Action journal: ➖ resend does not change business data; the log line is enough.
- Monthly cost: ➖ none.

## Planned file map

```text
src/Modules/Sales/
├── Sales.Domain/QuickOrders/                          planned: QuickOrder.cs (root), QuickOrderItem.cs, QuickOrderId.cs, QuickOrderItemId.cs,
│                                                      QuickOrderTelegramStatus.cs, QuickOrderErrors.cs (six files, flat: the axiom splits only above six)
├── Sales.Application/Abstractions/QuickOrders/        IQuickOrderStore.cs (find, add with outbox, save resend), IQuickOrderReadService.cs
├── Sales.Application/Features/QuickOrders/
│   ├── Commands/PlaceQuickOrder/                      existing: handler saves the aggregate (R-001, R-002)
│   ├── Commands/ResendQuickOrderToTelegram/           planned: command, validator, handler
│   ├── Queries/GetQuickOrderList/                     planned: query, request, validator, handler, list item DTO
│   ├── Queries/GetQuickOrderById/                     planned: query, validator, handler, detail and item DTOs
│   └── Messages/                                      existing: QuickOrderPlaced, QuickOrderLine (payload unchanged); planned: give-up handler
├── Sales.Infrastructure/QuickOrders/                  existing: Excel, caption, sender (+ delivery state)
├── Sales.Infrastructure/Persistence/QuickOrders/      planned: configurations, store (moved from QuickOrders/), read service
├── Sales.Infrastructure/Migrations/                   planned: AddQuickOrders (+ copy from outbox)
└── Sales.Api/Endpoints/QuickOrders/                   existing route groups + PlaceQuickOrder/; planned GetQuickOrderList/, GetQuickOrderById/, ResendQuickOrderToTelegram/ (one folder per endpoint)
tests/UnitTests/Sales/QuickOrders/                     domain + handlers (SC-002, SC-004, SC-007, SC-010)
tests/IntegrationTests/Sales/QuickOrders/              API + PostgreSQL (SC-001…SC-009)
```

`Sales.Infrastructure/QuickOrders` keeps three files after the store moves to `Persistence/QuickOrders`.

## Tasks

| ID | Task | Depends on |
| --- | --- | --- |
| **TS-001** | Tests first: domain (SC-010), place handler with cancellation and repeat (SC-003, SC-004) — Red | — |
| **TS-002** | Domain aggregate, id, errors; EF configurations, repository, migration with the copy (SC-009) — Green | TS-001 |
| **TS-003** | Place handler and store on the aggregate; sender and give-up handler update the delivery state (SC-001, SC-002) | TS-002 |
| **TS-004** | Admin list and detail queries and endpoints (SC-005, SC-006, SC-008) | TS-002 |
| **TS-005** | Resend command and endpoint (SC-007) | TS-003 |
| **TS-006** | OpenAPI regenerated; feature card, data model `sales-orders.md`, feature map, module map, rate limit table entry | TS-004, TS-005 |
| **TS-007** | Architecture audit of the feature (`backend-code-audit`); fix every finding in the same PR | TS-006 |

## Verification evidence

2026-11-24, local .NET SDK 10.0.115 and PostgreSQL 16 (`KEDR_SALES_TEST_POSTGRES_CONNECTION`):

- Unit tests: `FullyQualifiedName~QuickOrder` 27 passed (SC-002 domain part, SC-003, SC-004, SC-007, SC-010); whole `UnitTests` 550 passed.
- Integration: `FullyQualifiedName~QuickOrder` 13 passed (SC-001, SC-002, SC-004…SC-008); all `IntegrationTests.Sales` 99 passed.
- Architecture tests 74 passed (folders, aggregate roots, validators, `OpenApiDocumentRulesTests`, `RateLimitCoverageTests`); `Host.Api` build regenerated `openapi.json`.
- SC-009: database migrated to `MoveSalesTablesToSalesSchema`, three quick-order outbox rows inserted (processed, failed after 10 attempts with «Forbidden: bot was kicked», waiting with «chat not found»), then `AddQuickOrders` applied: three orders with totals 261.00, 7209.00, 1.00 and states sent / failed / pending with the errors, four lines.
- Documentation: `check_doc_links.py`, `check_module_maps.py`.

## Feature audit (TS-007)

2026-11-24, `backend-code-audit` on the staged change.

- **Step 0.** [Quality targets](../../../architecture/overview/quality-targets.md) exist with measured reads P95 328 ms and writes P95 1 159 ms; «orders per day» is still «Not measured» there (existing gap, not from this feature). The create adds one small insert to a write of a few orders a day; the list is a paged indexed read. No new speed risk.
- **AF-001 (Medium, fixed).** `Sales.Domain/QuickOrders` had `ValueObjects/` (2 files) and `Errors/` (1 file) with only six files in the aggregate: the folder axiom splits only above six and forbids one-file folders. Flattened to six files.
- **AF-002 (Medium, fixed).** `Features/QuickOrders/Delivery/` held one file (new one-file folder). The give-up handler moved next to its message in `Messages/`.
- **AF-003 (Low, fixed during delivery).** An endpoint helper file at the endpoint feature root broke `EndpointFeatureFolders_HoldOnlyTheRouteGroup_AndOneFolderPerEndpoint`; the three endpoints map the enum inline.
- Module boundaries, layers, CQRS level 1, one save per command (place: one `SaveChanges`; resend: one transaction for the outbox requeue and the order), outbox for the Telegram side effect with the delivery state saved in the dispatch transaction, per-word trigram search, named indexes, `xmin` and conditional requeue for races, idempotency (request key on create, state check on resend), rate limits, access policy, personal data and logs, Ukrainian messages: no findings. No ADR or C4 change: no new module, external system or way to run work.
- Fresh evidence after the fixes: unit `QuickOrder` 27, integration `QuickOrder` 13, architecture 74, all passed.

## Feature audit of R-013 (2026-11-26)

`backend-code-audit` on the staged change.

- **Step 0.** [Quality targets](../../../architecture/overview/quality-targets.md): writes P95 1 159 ms against 2 s. `POST /api/orders` now also tries the 1C message right after the commit (two small inserts and a key lookup in the dispatch transaction, next to the Telegram upload that already runs there); site orders are a few a day. No new speed risk; «orders per day» stays «Not measured» (existing gap).
- **AF-004 (Medium, fixed).** The [idempotency audit table](../../../standards/idempotency-rules.md#audit-2026-11-15-write-operations-and-handlers) did not list the new at-least-once handler. Added `CreateOneCOrderFromQuickOrderHandler`: a repeat sees `OneCOrderId` and makes nothing; the order, its 1C message and the link commit together.
- **AF-005 (Medium, fixed).** A handler that changes two roots needs its reason in the SDD; added to R-013 with the personal-data note.
- **AF-006 (Low, accepted, owner's call).** The 1C order amounts are the cart prices the site sent (R-004: a copy, not a price check). A forged request could put a wrong amount in the 1C order; the manager confirms the order in 1C as with the Excel file. Pricing the lines from the catalogue is a separate change if wanted.
- Module boundaries (Sales only, no new Contracts), layers, CQRS level 1, typed `OrderId` reference without navigation, `LinkOneCOrder` checked in the root, `Order.Create`/`OrderLine.Create` returning results, outbox for the side effect, savepoint and detach on a failed save inside the batch, additive migration (two nullable columns, FK `SET NULL`) that keeps the previous release working, no new endpoint (no rate limit or contract change), folder axiom, logs without name or phone: no findings. No ADR or C4 change: no new module, external system or way to run work.
- Fresh evidence: unit `QuickOrder|CreateOrderCommandHandler` 49, integration `QuickOrder|OrderOneCDelivery|AdminOrderCreation` 25, architecture 74, all passed; `check_doc_links.py`, `check_module_maps.py` pass.

## Feature audit of R-015 (2026-11-27)

Scope: the read-only link between a site order and its 1C order. Quality targets: admin reads stay under the cheap signed-in read targets in [quality targets](../../../architecture/overview/quality-targets.md); the change adds two primary-key subqueries per row to a page of at most 100 rows and one indexed subquery (`IX_QuickOrders_OneCOrderId`) per order row, all in the same SQL statement, so no N+1 and no new round trip. Boundaries: both aggregates and both reads are in Sales, reads are `AsNoTracking` projections and never load an aggregate (CQRS level 1). No write, no migration, no new endpoint (no rate limit or idempotency change). Folders touched hold at most six files. Contract regenerated (`openapi.json`, nullable fields added). Findings: none open.

## Verification plan

Unit `FullyQualifiedName~QuickOrder`, integration `FullyQualifiedName~QuickOrder` (Docker), architecture tests, `OpenApiDocumentRulesTests`, `RateLimitCoverageTests`, `check_doc_links.py`, `check_module_maps.py`.

## Traceability

| Rule | Scenarios | Task |
| --- | --- | --- |
| R-001, R-004 | SC-001, SC-010 | TS-002, TS-003 |
| R-002 | SC-003 | TS-003 |
| R-003 | SC-004 | TS-003 |
| R-005 | SC-001, SC-002 | TS-003 |
| R-006, R-007, R-009 | SC-005, SC-006, SC-008 | TS-004 |
| R-008 | SC-007 | TS-005 |
| R-010 | SC-009 | TS-002 |
| R-011 | code review in TS-007 | TS-007 |
| R-013 | SC-012 | `CreateOneCOrderFromQuickOrderTests`, `QuickOrderToOneCPostgresTests` (filter `FullyQualifiedName~QuickOrder`) |
| R-015 | SC-013 (link) | `QuickOrderToOneCPostgresTests`, `QuickOrderAdminApiTests` (filter `FullyQualifiedName~QuickOrder`) |
| R-014 | SC-013 | `QuickOrderTests` (comment), `QuickOrderOneCCommentTests`, `QuickOrderTelegramCaptionTests`, `QuickOrderApiTests` (filter `FullyQualifiedName~QuickOrder`) |
