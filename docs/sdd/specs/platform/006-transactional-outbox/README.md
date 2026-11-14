# Platform 006: Transactional outbox

**Status:** accepted 2026-11-14 (owner: the outbox is its own stage of the architecture restructure, after the jobs audit). **Owner:** platform (shared code), Sales (first messages). **Rules:** [module rules: transactional outbox](../../../standards/module-rules.md#transactional-outbox), [database rules](../../../standards/database-rules.md).

## Goal

A use case that saves data and must then tell the outside world (Telegram, 1C, another module) does it reliably: the message is stored in the same transaction as the data, sent right after the commit, and retried by a dispatcher until it goes through or is marked failed. Nothing is lost when Telegram or 1C is down for a minute, and nothing is sent for a change that rolled back.

**Today (jobs audit 2026-11-14):**

- The site quick order (`POST /api/orders`, Catalog) is not saved anywhere: the request builds an Excel file and sends it to the managers' Telegram chat. When Telegram fails, the customer sees an error and the order is gone.
- Sales already has two hand-made outboxes: `OneCOrderSyncs` (order → 1C, sent by the `onec-orders-sync` Cloud Run Job) and `PendingCounterpartyIdentityDeletions` (Sales → Identity, drained by the customer sync). Each has its own retry code.
- The 1C read synchronizations (catalog, prices, stocks, customers), the CDN media sync and the attribute jobs re-run from their source and do not need an outbox.

**Out of scope here:** moving order → 1C and the customer-deletion drain onto the shared outbox (TS-005, TS-006, separate PRs after the dispatcher runs in production); an admin page of failed messages (rule of three: logs and the database are enough for one message type); a message bus.

## Rules

- R-001 Shared code in `BuildingBlocks`: the message row, the dispatcher and the handler registry. Each module owns its rows in its own table `<schema>."OutboxMessages"` and its handlers. A module whose tables are still in `public` (Sales, Catalog, Identity) puts its outbox table in its own named schema (`sales`), the target schema of the module.
- R-002 A use case adds the message in the **same `SaveChanges`** as its data. A message is a record of plain values (ids, names, numbers), serialized as JSON with its type name; it never carries an entity.
- R-003 Right after the commit the request tries its own messages once. A failure there never fails the request: the customer gets the success answer, and the message waits for the dispatcher.
- R-004 The dispatcher takes due rows (not processed, not failed, next attempt due) in small batches with `FOR UPDATE SKIP LOCKED`, so two dispatch calls never send the same row at the same time. Success sets `ProcessedAtUtc`. A failure increments `Attempts`, stores the error text (cut to 1,000 characters) and moves `NextAttemptAtUtc` by 1, 5, 15, 30, 60 minutes and then hourly; after 10 attempts the row gets `FailedAtUtc` and an error log entry, and is not tried again. Rows are never deleted silently: processed rows are removed after 30 days, failed rows stay until someone handles them.
- R-005 Handlers are idempotent where the receiver allows it: the receiver gets a stable key (the message id or a business key). Telegram has no such key, so a crash between the send and the mark can repeat a notification; the message text carries the order id so managers see a repeat.
- R-006 On Cloud Run there is no in-process loop, timer or Hangfire. The scheduled dispatch is one call of `IOutboxDispatcher.DispatchDueAsync`, which processes due rows of every module and returns at once when none are due. Which scheduler calls it, and through which entry point, is decided by the shared background-jobs dispatcher (owner request 2026-11-14: all jobs run from one Scheduler-triggered dispatcher and are managed in the admin); this spec adds no Cloud Scheduler entry. It needs a short cadence (every minute or a few minutes), because a message that failed right after its request waits for it.
- R-007 The site quick order belongs to Sales (orders are Sales data). `POST /api/orders` keeps its route, tag `Order`, request and response schemas, rate limit and anonymous access, so the OpenAPI document does not change. The order is stored as an outbox message `QuickOrderPlaced` (id, name, phone, lines, placed at) in `sales."OutboxMessages"`; its handler builds the Excel file and sends it to the managers' chat as today.

## Scenarios

- SC-001 A customer sends a quick order while Telegram works: 200 with the order id, the managers get the Excel file within a second, the row is processed.
- SC-002 A customer sends a quick order while Telegram is down: 200 with the order id; the row stays due with attempt 1 and the error; the next dispatch after Telegram recovers sends it and marks it processed.
- SC-003 Two dispatch calls run at once: each row is sent once.
- SC-004 A handler fails 10 times: the row is marked failed, an error is logged with the message id and type, and it is not tried again.
- SC-005 The scheduled dispatch cannot be started by an anonymous caller.
- SC-006 An invalid quick order (empty phone, no lines): 400 `ProblemDetails` as today, and no outbox row.
- SC-007 `GET /openapi/v1.json` is unchanged.

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

## Risks

| Risk | Guard |
| --- | --- |
| The new table is a production migration | Additive only (new schema and table); applied by the deploy migration job; coordinator is told before the merge |
| A new Cloud Scheduler job is a cloud change and a paid resource past three jobs | None is added; the shared background-jobs dispatcher runs the dispatch (TS-004) |
| The quick order phone number now stays in the database | Processed rows are deleted after 30 days (R-004) |
| A Telegram repeat after a crash | Rare; the order id in the caption shows it (R-005) |
| The quick order move changes the OpenAPI document | Records keep their names; snapshot test (SC-007) |

## Tasks

- TS-001 Shared outbox (BuildingBlocks), Sales outbox table and migration, `IOutboxDispatcher`; integration tests for SC-002…SC-004 with a test message. **Done** (#214).
- TS-002 Entry point of the scheduled dispatch (SC-005), fitted to the shared background-jobs dispatcher when it is designed.
- TS-003 Quick order moves to Sales and goes through the outbox (R-007, SC-001, SC-002, SC-006, SC-007); Catalog order code removed; feature cards updated. **Done.** The Telegram sender is the outbox handler itself (no extra notifier port). Field errors of a 400 are now keyed `FirstName`, `Phone`, `Lines` (were `Request.FirstName`…); the site already reads these names.
- TS-004 (cloud, through the coordinator and the owner) The shared background-jobs dispatcher runs the outbox dispatch on its schedule.
- TS-005 Order → 1C on the shared outbox (keeps the sending window, the stable order number key and the dead-letter Telegram signal); then the `onec-orders-sync` job and its schedule are removed through the coordinator. Also moves that logic from Sales.Infrastructure to Sales.Application (jobs audit J-1).
- TS-006 Customer → Identity deletion on the shared outbox.

Each task: Red (integration test), Green, then the module's tests and architecture tests.

## Rollout and rollback

TS-001…TS-003 deploy without cloud changes: messages go out right after the request (R-003), the same as today, and a failed one waits for the dispatcher. Until TS-004 a failed message waits in the table (today such a quick order is lost). Rollback: revert the PR; the `sales."OutboxMessages"` table can stay (nothing reads it).
