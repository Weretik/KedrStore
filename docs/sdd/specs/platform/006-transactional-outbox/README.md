# Platform 006: Transactional outbox

**Status:** accepted 2026-11-14 (owner: the outbox is its own stage of the architecture restructure, after the jobs audit). **Owner:** platform (shared code), Sales (first messages). **Rules:** [module rules: transactional outbox](../../../standards/module-rules.md#transactional-outbox), [database rules](../../../standards/database-rules.md).

## Goal

A use case that saves data and must then tell the outside world (Telegram, 1C, another module) does it reliably: the message is stored in the same transaction as the data, sent right after the commit, and retried by a dispatcher until it goes through or is marked failed. Nothing is lost when Telegram or 1C is down for a minute, and nothing is sent for a change that rolled back.

**Today (jobs audit 2026-11-14):**

- The site quick order (`POST /api/orders`, Catalog) is not saved anywhere: the request builds an Excel file and sends it to the managers' Telegram chat. When Telegram fails, the customer sees an error and the order is gone.
- Sales already has two hand-made outboxes: `OneCOrderSyncs` (order → 1C, sent by the `onec-orders-sync` Cloud Run Job) and `PendingCounterpartyIdentityDeletions` (Sales → Identity, drained by the customer sync). Each has its own retry code.
- The 1C read synchronizations (catalog, prices, stocks, customers), the CDN media sync and the attribute jobs re-run from their source and do not need an outbox.

**Out of scope here:** an admin page of all outbox messages (the orders table shows the 1C delivery of each order, R-011; other messages are seen in logs and the database); a message bus.

**Stage 2 (TS-005, TS-006, owner 2026-11-15):** «один outbox, убери свои механизмы», and the 1C state of every order stays visible in the admin orders table (success or problem, with the reason, attempts, last and next attempt) with a manual «Повторити». Rules R-008…R-014, scenarios SC-008…SC-017.

## Rules

- R-001 Shared code in `BuildingBlocks`: the message row, the dispatcher and the handler registry. Each module owns its rows in its own table `<schema>."OutboxMessages"` and its handlers. A module whose tables are still in `public` (Sales, Catalog, Identity) puts its outbox table in its own named schema (`sales`), the target schema of the module.
- R-002 A use case adds the message in the **same `SaveChanges`** as its data. A message is a record of plain values (ids, names, numbers), serialized as JSON with its type name; it never carries an entity.
- R-003 Right after the commit the request tries its own messages once. A failure there never fails the request: the customer gets the success answer, and the message waits for the dispatcher.
- R-004 The dispatcher takes due rows (not processed, not failed, next attempt due) in small batches with `FOR UPDATE SKIP LOCKED`, so two dispatch calls never send the same row at the same time. Success sets `ProcessedAtUtc`. A failure increments `Attempts`, stores the error text (cut to 1,000 characters) and moves `NextAttemptAtUtc` by 1, 5, 15, 30, 60 minutes and then hourly; after 10 attempts the row gets `FailedAtUtc` and an error log entry, and is not tried again. Rows are never deleted silently: processed rows are removed after 30 days, failed rows stay until someone handles them.
- R-005 Handlers are idempotent where the receiver allows it: the receiver gets a stable key (the message id or a business key). Telegram has no such key, so a crash between the send and the mark can repeat a notification; the message text carries the order id so managers see a repeat.
- R-006 On Cloud Run there is no in-process loop, timer or Hangfire. The scheduled dispatch is one call of `IOutboxDispatcher.DispatchDueAsync`, which processes due rows of every module and returns at once when none are due. Which scheduler calls it, and through which entry point, is decided by the shared background-jobs dispatcher (owner request 2026-11-14: all jobs run from one Scheduler-triggered dispatcher and are managed in the admin); this spec adds no Cloud Scheduler entry. It needs a short cadence (every minute or a few minutes), because a message that failed right after its request waits for it.
- R-007 The site quick order belongs to Sales (orders are Sales data). `POST /api/orders` keeps its route, tag `Order`, request and response schemas, rate limit and anonymous access, so the OpenAPI document does not change. The order is stored as an outbox message `QuickOrderPlaced` (id, name, phone, lines, placed at) in `sales."OutboxMessages"`; its handler builds the Excel file and sends it to the managers' chat as today.

### Stage 2: 1C orders and the customer deletion on the outbox

- R-008 A handler answers how the delivery went instead of throwing: `OutboxHandlingResult.Delivered`, `Failed(error)` (counts as an attempt, R-004 applies) or `RetryAt(time)` (not an attempt: the row waits until that time, `Attempts` stays). An exception still means `Failed`. Every attempt stores `LastAttemptAtUtc`.
- R-009 A module may register a give-up handler for a message type (`IOutboxGiveUpHandler<TMessage>`). It runs in the same transaction when the last allowed attempt fails and may store follow-up messages; its failure is logged and does not undo the give-up.
- R-010 An order created in the admin or the mobile app stores the outbox message `OrderPlaced` (`sales.order-placed`, the order id) in the same transaction as the order; the order keeps the message id (`Orders."OneCMessageId"`), so its delivery row is found by key. The tables `OneCOrderSyncs`, `PendingCounterpartyIdentityDeletions`, the task «Замовлення в 1С» (`orders`) and the `sync-one-c-orders` command are removed. The request does not try the message right after the commit: 1C answers slowly and an admin order never waited for it; the scheduled dispatch sends it (every 5 minutes).
- R-011 The 1C answer is a fact of the order, not of the delivery: `Order` stores `OneCDocumentNumber` + `OneCAcceptedAtUtc`, or `OneCRejection` + `OneCRejectedAtUtc`. The handler sends only in the Kyiv window 07:00–20:00 (`Sales:OneCOrderSync:SendingWindowStart/End`), otherwise `RetryAt(next window start)`. 1C accepted → order accepted, `Delivered`; 1C rejected → order rejected, `Delivered` (a business error is not retried); transport error → `Failed`. 1C recognises a repeat by the stable order number.
- R-012 The 1C state shown to the admin and the mobile app is computed: accepted → `Accepted`; rejected → `BusinessError` with the reason; message failed → `DeadLetter` with the last error; message with failed attempts → `RetryScheduled`; otherwise `Pending`. With it go the error, attempts, last and next attempt. `Sent` and `TransportError` are no longer produced (a send in progress is a locked row). The admin list and detail add these fields to `syncStatus` (additive change of the contract).
- R-013 «Повторити» (`POST /api/admin/orders/{orderId}/sync/retry`, reason required) is allowed for `BusinessError` and `DeadLetter`: it clears the rejection, puts the message back in the queue with zero attempts (a conditional `UPDATE` on the processed or failed row, or a new row with the same id when retention removed it) and writes `OrderSyncRetryAudits` (who, why, previous state) in one transaction; a second press or a race answers 409. When the message gives up (R-009), a `sales.order-one-c-delivery-failed` message sends the managers the Excel file to Telegram as before.
- R-014 Removing a stale customer stores `CounterpartyRemoved` (`sales.counterparty-removed`: customer id and Identity user id) in the same transaction; its handler deletes the Identity user through `Identity.Contracts`, unless another customer still uses that user. Deleting a user that is already gone counts as done.

Data migration (one EF migration, same deploy): every `OneCOrderSyncs` row becomes order fields and an outbox row with the same timing — accepted and business-error rows fill the order fields only; pending, sent and retry rows become due messages keeping their attempts and error (sent ones are due now); dead-letter rows become failed messages, and those whose Telegram signal was not sent yet also get a `sales.order-one-c-delivery-failed` message. `OrderSyncRetryAudits` loses its `OneCOrderSyncId`. Every `PendingCounterpartyIdentityDeletions` row becomes a due `sales.counterparty-removed` message. Then both old tables are dropped. Down restores empty tables only (rollback is a new deploy of the previous release plus a database restore if data matters).

## Scenarios

- SC-001 A customer sends a quick order while Telegram works: 200 with the order id, the managers get the Excel file within a second, the row is processed.
- SC-002 A customer sends a quick order while Telegram is down: 200 with the order id; the row stays due with attempt 1 and the error; the next dispatch after Telegram recovers sends it and marks it processed.
- SC-003 Two dispatch calls run at once: each row is sent once.
- SC-004 A handler fails 10 times: the row is marked failed, an error is logged with the message id and type, and it is not tried again.
- SC-005 The scheduled dispatch cannot be started by an anonymous caller.
- SC-006 An invalid quick order (empty phone, no lines): 400 `ProblemDetails` as today, and no outbox row.
- SC-007 `GET /openapi/v1.json` is unchanged.
- SC-008 An admin order is created: the order and its `sales.order-placed` message are saved together; the list shows `Pending`.
- SC-009 The dispatch inside the window, 1C accepts: the order has the document number, the message is processed, the list shows `Accepted`.
- SC-010 1C rejects: the order keeps the reason, the message is processed and not retried, the list shows `BusinessError` with the reason.
- SC-011 1C is unreachable: the message has attempt 1, the error, last and next attempt; the list shows `RetryScheduled` with them.
- SC-012 The dispatch outside 07:00–20:00 Kyiv: nothing is sent, the message waits until 07:00, attempts stay 0.
- SC-013 Ten transport failures: the message fails, the list shows `DeadLetter`, and the managers get the Telegram file once.
- SC-014 «Повторити» on `BusinessError` or `DeadLetter`: the state is `Pending` again, an audit row is written; on any other state, or a second press, 409.
- SC-015 The migration moves pending, retry, accepted, rejected and dead-letter rows without losing attempts, errors or document numbers.
- SC-016 A stale customer is removed: its Identity user is deleted by the outbox; a failure is retried; a user still used by another customer is kept.
- SC-017 The «Фонові задачі» page no longer lists «Замовлення в 1С»; «Досилання повідомлень» names 1C orders.

## Design

```text
src/BuildingBlocks/BuildingBlocks.Application/Outbox/
├── IOutboxMessage.cs               planned: marker for a message record
├── IOutboxMessageHandler.cs        planned: Handle(message, messageId, ct)
└── IOutboxDispatcher.cs            planned: DispatchAsync(ids) after commit; DispatchDueAsync() for the endpoint
src/BuildingBlocks/BuildingBlocks.Infrastructure/Outbox/
├── OutboxMessage.cs                planned: row (id, type, payload, created, attempts, next attempt, processed, failed, last error)
├── OutboxMessageConfiguration.cs   planned: ApplyOutbox(modelBuilder, schema) → <schema>."OutboxMessages" + due index
├── OutboxWriter.cs                 planned: Add(dbContext, message) in the module store
├── OutboxMessageTypes.cs           planned: type name ↔ CLR type and handler, registered per module
├── ModuleOutbox.cs                 planned: one module's table: lock due rows, run handler, mark
├── OutboxDispatcher.cs             planned: IOutboxDispatcher over every module's ModuleOutbox
├── OutboxRetrySchedule.cs          planned: R-004 delays and limits
└── DependencyInjection/OutboxRegistration.cs  planned: AddOutbox<TDbContext>(schema), AddOutboxHandler<TMessage, THandler>()
src/Modules/Sales/
├── Sales.Application/Features/QuickOrders/Commands/PlaceQuickOrder/   done (moved from Catalog): command, validator, handler
├── Sales.Application/Features/QuickOrders/Messages/                   done: QuickOrderPlaced ("sales.quick-order-placed"), QuickOrderLine
├── Sales.Application/Abstractions/QuickOrders/                        done: IQuickOrderStore
├── Sales.Infrastructure/QuickOrders/                                  done: store (outbox write), Excel file, Telegram caption, sender (the outbox handler)
├── Sales.Infrastructure/Persistence/SalesDbContext.cs                 done: ApplyOutbox("sales")
├── Sales.Infrastructure/Migrations/20261114122151_AddSalesOutbox.cs   done: schema sales, table OutboxMessages
└── Sales.Api/Endpoints/QuickOrders/                                   done (moved from Catalog.Api/Endpoints/Orders): same route, tag, records
src/Modules/Catalog/…/Orders, Integrations/Excel, Integrations/Telegram         removed
tests/UnitTests/BuildingBlocks/Outbox/                                  done: retry schedule
tests/UnitTests/Sales/QuickOrders/                                      done: caption and file name
tests/IntegrationTests/Platform/Outbox/                                 done: same transaction, dispatch once, retry, failed, SKIP LOCKED, retention; endpoint auth with TS-002
tests/IntegrationTests/Sales/QuickOrders/                               done: SC-001, SC-002, SC-006
```

### Stage 2 files

```text
src/BuildingBlocks/BuildingBlocks.Application/Outbox/
├── OutboxHandlingResult.cs          Delivered | Failed(error) | RetryAt(time) (R-008)
└── IOutboxGiveUpHandler.cs          runs when the last attempt fails (R-009)
src/BuildingBlocks/BuildingBlocks.Infrastructure/Outbox/
├── Persistence/OutboxMessage.cs     + LastAttemptAtUtc, Postpone
└── Persistence/OutboxWriter.cs      + RequeueAsync (conditional UPDATE, or a new row with the same id)
src/Modules/Sales/Sales.Domain/Orders/Order.cs        + OneCMessageId, AcceptByOneC, RejectByOneC, ReopenOneCDelivery
src/Modules/Sales/Sales.Application/Features/Orders/OneCDelivery/
├── Messages/                        OrderPlaced, OrderOneCDeliveryFailed
├── Send/                            SendOrderToOneCHandler (R-011), OneCSendingWindow, its options
└── GiveUp/                          OrderOneCGiveUpHandler, OrderOneCDeliveryFailedHandler (Telegram file)
src/Modules/Sales/Sales.Application/Abstractions/Orders/  IOrderOneCDeliveryStore, OneCMessageState; Sync/ OneCOrderSyncStatus (moved from Domain), OrderOneCSync
src/Modules/Sales/Sales.Infrastructure/Customers/  CounterpartyRemoved, RemovedCounterpartyIdentityDeleter (R-014; Identity.Contracts is reached from Infrastructure)
src/Modules/Sales/Sales.Infrastructure/Orders/OneCDelivery/    store, status projection used by every order reader, retry store
src/Modules/Sales/Sales.Infrastructure/Migrations/<ts>_MoveOneCOrdersToOutbox.cs   data migration
removed: Sales.Domain OneCOrderSync*, Integrations/OneC/Orders/*, Jobs/SyncOneCOrdersJob, PendingCounterpartyIdentityDeletion, Host.Jobs OrdersTaskRunner and sync-one-c-orders, Scheduling task `orders`
```

Expired `OrderIdempotencyRecords` (24 h) were removed by the old job; the order create now removes expired ones before it saves.

## Risks

| Risk | Guard |
| --- | --- |
| The new table is a production migration | Additive only (new schema and table); applied by the deploy migration job; coordinator is told before the merge |
| A new Cloud Scheduler job is a cloud change and a paid resource past three jobs | None is added; the shared background-jobs dispatcher runs the dispatch (TS-004) |
| The quick order phone number now stays in the database | Processed rows are deleted after 30 days (R-004) |
| A Telegram repeat after a crash | Rare; the order id in the caption shows it (R-005) |
| The quick order move changes the OpenAPI document | Records keep their names; snapshot test (SC-007) |
| Stage 2 drops two tables in production | Data migration first in the same migration (SC-015); the coordinator is told before the merge; migration job runs before the new revision |
| Slow 1C calls inside the tick's transaction | One batch of at most 50 rows per tick; usually a few orders; rows are locked with `SKIP LOCKED`, nothing else waits for them |
| The old Cloud Run job still sending | The `onec-orders-sync` job was already removed from the deploy (2026-11-14); the `orders` task disappears from the catalogue, so the tick never starts it |

## Tasks

- TS-001 Shared outbox (BuildingBlocks), Sales outbox table and migration, `IOutboxDispatcher`; integration tests for SC-002…SC-004 with a test message. **Done** (#214).
- TS-002 Entry point of the scheduled dispatch (SC-005), fitted to the shared background-jobs dispatcher when it is designed.
- TS-003 Quick order moves to Sales and goes through the outbox (R-007, SC-001, SC-002, SC-006, SC-007); Catalog order code removed; feature cards updated. **Done.** The Telegram sender is the outbox handler itself (no extra notifier port). Field errors of a 400 are now keyed `FirstName`, `Phone`, `Lines` (were `Request.FirstName`…); the site already reads these names.
- TS-004 (cloud, through the coordinator and the owner) The shared background-jobs dispatcher runs the outbox dispatch on its schedule.
- TS-005 Order → 1C on the shared outbox (R-008…R-013; keeps the sending window, the stable order number key and the dead-letter Telegram signal); the `orders` task and `sync-one-c-orders` command removed; the logic moves from Sales.Infrastructure to Sales.Application (jobs audit J-1). Admin (kedr-web): the 1C column keeps its labels and shows the reason, attempts, last and next attempt of a problem; the order panel shows them with «Повторити». **Backend done** (this change); kedr-web follows after the contract sync.
- TS-006 Customer → Identity deletion on the shared outbox (R-014). **Done.**

Each task: Red (integration test), Green, then the module's tests and architecture tests.

## Rollout and rollback

TS-001…TS-003 deploy without cloud changes: messages go out right after the request (R-003), the same as today, and a failed one waits for the dispatcher. Until TS-004 a failed message waits in the table (today such a quick order is lost). Rollback: revert the PR; the `sales."OutboxMessages"` table can stay (nothing reads it).
