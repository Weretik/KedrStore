# Sales/1C Integration Runbook

Sales has two independent 1C flows:

- customer reference-data synchronization imports counterparties and category pricing rules;
- manager order delivery sends already-saved Sales orders to 1C through the shared outbox.

Both flows use the shared low-level SOAP transport. Customer synchronization runs in `Host.Jobs` (one operation, then exit); order delivery runs in the outbox tick of `Host.Api`.

## Customer reference-data synchronization

Before synchronizing customers, read the [counterparty and Identity flow](../../architecture/integrations/one-c/sales-counterparty-identity-sync.md). During reconciliation it creates, updates, and may delete local Identity users.

| Command | Operation |
| --- | --- |
| `--job=counterparties` | Imports, updates, and restores counterparties according to Sales service rules |
| `--job=counterparty-category-price-types` | Imports customer/category price-type rules |
| `--job=sales-customers-full` | Runs counterparties first, then pricing rules |

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs.csproj -- --job=sales-customers-full
```

Before accepting the refresh, check the imported, updated, restored, deleted, and skipped counters in the final logs.

## Manager order delivery

Manager orders (admin and mobile) go to 1C through the shared Sales outbox ([platform spec 006](../../specs/platform/006-transactional-outbox/README.md), R-008…R-013). The HTTP request saves the `Order` and an `sales.order-placed` row in `sales."OutboxMessages"` in one transaction and does not wait for 1C. The order's `OneCMessageId` is the id of that row. There is no separate job or table for order delivery.

### How it runs

- Cloud Scheduler calls `POST /internal/scheduler/tick` every 5 minutes; the background task «Досилання повідомлень» on «Фонові задачі» dispatches due outbox rows (batch 50, `FOR UPDATE SKIP LOCKED`, so two ticks never take the same row).
- Sending is allowed only inside the `Europe/Kyiv` window (end exclusive). Outside it the row is postponed to the next opening; this is not counted as an attempt.
- 1C answers with a document number: it is stored on the order (`OneCDocumentNumber`, `OneCAcceptedAtUtc`), status `Accepted`.
- 1C refuses: the reason is stored on the order (`OneCRejection`), status `BusinessError`. There is no automatic retry.
- Transport failure (network, timeout, 5xx): the row stays, status `RetryScheduled`, next attempts after 1, 5, 15, 30, 60 minutes, then hourly. 1C recognises a repeat by the stable `OrderNumber`.
- After 10 failed attempts the row is marked failed, status `DeadLetter`, and in the same transaction a `sales.order-one-c-delivery-failed` message is queued. It sends the managers a Telegram message with an Excel file of the saved lines, with its own outbox retries; it never resends the order to 1C.

To stop outbound delivery, switch «Досилання повідомлень» off on «Фонові задачі». This also stops the site quick order Telegram messages and the removal of Identity users of deleted customers; their rows wait and are sent when the task is switched back on.

### Configuration

| Parameter | Purpose |
| --- | --- |
| `ConnectionStrings:Default` / `ConnectionStrings__Default` | Sales database with the orders and the outbox |
| `OneCSoap:Endpoint`, `OneCSoap:Username`, `OneCSoap:Password` | 1C SOAP endpoint and authentication (secrets) |
| `Telegram:BotToken` / `Telegram:ChatId` | Alerts for orders that never reached 1C (secrets) |
| `Sales:OneCOrderSync:SendingWindowStart` / `Sales__OneCOrderSync__SendingWindowStart` | Sending window start in Kyiv time as `hh:mm:ss`; default `07:00:00` |
| `Sales:OneCOrderSync:SendingWindowEnd` / `Sales__OneCOrderSync__SendingWindowEnd` | Exclusive sending window end in Kyiv time as `hh:mm:ss`; default `20:00:00` |

These are read by `Host.Api`, which runs the outbox tick. An invalid window (end not later than start within one day) stops the host at startup.

### Status interpretation

The admin order list and detail show the status with its reason, attempts, last attempt and next attempt (`syncStatus`, `syncError`, `syncAttempts`, `syncLastAttemptAtUtc`, `syncNextAttemptAtUtc`).

| Status | Meaning | Operator action |
| --- | --- | --- |
| `Pending` | Saved, waiting for the next tick or the sending window | Nothing |
| `RetryScheduled` | A transport attempt failed; the next one is at `syncNextAttemptAtUtc` | Fix connectivity or configuration if it repeats; the outbox retries by itself |
| `Accepted` | 1C created the document | Do not resend; trace by the order number and the 1C document number |
| `BusinessError` | 1C refused the order; `syncError` is its reason | Fix the data in 1C or the order, then «Повторити» |
| `DeadLetter` | 10 transport attempts failed; managers got the Telegram alert | Follow the procedure below |

### Diagnostics

Search logs by `OrderNumber` and the outbox message id (`OneCMessageId`). The outbox row keeps `Attempts`, `LastError`, `LastAttemptAtUtc`, `NextAttemptAtUtc`, `ProcessedAtUtc` and `FailedAtUtc`. Do not include SOAP bodies, credentials, customer data or Telegram tokens in incident records.

### Dead-letter procedure

1. Find the order by its number and check `syncError` and the attempts.
2. Check that the Telegram alert arrived (its own outbox row `sales.order-one-c-delivery-failed`).
3. Check the endpoint, credentials, network, counterparty and products without copying secrets.
4. Ask the 1C operator whether a document already exists for this `OrderNumber`.
5. After the cause is fixed, press «Повторити» on the order.

«Повторити» is `POST /api/admin/orders/{orderId}/sync/retry` with a non-empty JSON `reason`; it accepts only `BusinessError` and `DeadLetter`. A `202` means the same outbox row was put back in the queue (attempts from zero, the 1C refusal cleared) and the request was written to `OrderSyncRetryAudits`; the next tick sends it with the same `OrderNumber`. `409` means the current state cannot be retried; `404` means the order was not found. Do not reset outbox rows through SQL. The endpoint requires `PolicyNames.CanManageOrders`.

## Related instructions

- [Host.Jobs CLI](host-jobs-cli.md)
- [Job troubleshooting](troubleshooting.md)
- [Configuration and secrets](../configuration/configuration-and-secrets.md)
- [Manual 1C write smoke test](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md)
- [Background tasks](background-tasks.md)
