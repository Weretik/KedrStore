# Sales/1C Integration Runbook

Sales has two independent 1C flows:

- customer reference-data synchronization imports counterparties and category pricing rules;
- manager order delivery sends already-saved Sales orders to 1C.

Both flows use the shared low-level SOAP transport. Their coordination, persistence, retries, and operational state remain in the Sales infrastructure layer. `Host.Jobs` runs one specified operation and exits; recurring execution is provided by the environment scheduler.

## Customer reference-data synchronization

Before synchronizing customers, read the [counterparty and Identity flow](../../architecture/integrations/one-c/sales-counterparty-identity-sync.md). During reconciliation it creates, updates, and may delete local Identity users.

| Command | Operation |
| --- | --- |
| `--job=counterparties` | Imports, updates, and restores counterparties according to Sales service rules |
| `--job=counterparty-category-price-types` | Imports customer/category price-type rules |
| `--job=sales-customers-full` | Runs counterparties first, then pricing rules |

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=sales-customers-full
```

Before accepting the refresh, check the imported, updated, restored, deleted, and skipped counters in the final logs.

## Manager order delivery

`sync-one-c-orders` processes manager orders created through the Sales admin API. The HTTP request saves the `Order` and a `OneCOrderSync` record and does not wait for a 1C response. Later, the job selects due records, sends them through the Sales 1C adapter, stores the result, processes due dead-letter notifications, and exits.

Detailed business rules and state transitions are documented in the [feature specification](../../specs/sales/001-admin-order-one-c-sync/requirements/one-c-delivery.md). This runbook describes operation of the implemented job.

### Required configuration

For local Development, provide secrets through `Host.Jobs` User Secrets; for deployment, use the environment's secret store. Operational parameters have defaults in `Host.Jobs/appsettings.json` and can be overridden with environment variables.

| Parameter | Purpose |
| --- | --- |
| `ConnectionStrings:Default` / `ConnectionStrings__Default` | Sales data persistence in PostgreSQL and synchronization state |
| `OneCSoap:Endpoint` / `OneCSoap__Endpoint` | 1C SOAP endpoint for the current environment |
| `OneCSoap:Username` / `OneCSoap__Username` | 1C SOAP authentication |
| `OneCSoap:Password` / `OneCSoap__Password` | 1C SOAP authentication |
| `Telegram:BotToken` / `Telegram__BotToken` | Transport for dead-letter notifications |
| `Telegram:ChatId` / `Telegram__ChatId` | Dead-letter notification recipient |
| `Sales:OneCOrderSync:BatchSize` / `Sales__OneCOrderSync__BatchSize` | Maximum due orders per run; default `20` |
| `Sales:OneCOrderSync:InterCallDelay` / `Sales__OneCOrderSync__InterCallDelay` | Delay between SOAP calls as `hh:mm:ss`; default `00:00:05` |
| `Sales:OneCOrderSync:MaximumAttempts` / `Sales__OneCOrderSync__MaximumAttempts` | Transport-attempt limit before `DeadLetter`; default `10` |
| `Sales:OneCOrderSync:SendingWindowStart` / `Sales__OneCOrderSync__SendingWindowStart` | Sending window start in Kyiv time as `hh:mm:ss`; default `07:00:00` |
| `Sales:OneCOrderSync:SendingWindowEnd` / `Sales__OneCOrderSync__SendingWindowEnd` | Exclusive sending window end in Kyiv time as `hh:mm:ss`; default `20:00:00` |
| `Sales:OneCOrderSync:StaleSentRecoveryDelay` / `Sales__OneCOrderSync__StaleSentRecoveryDelay` | Age after which an unfinished `Sent` record is recovered; default `00:15:00` |

Apply Sales migrations before enabling recurring delivery. The job must receive all listed configuration even when the current batch contains no new dead-letter record.

### Local execution

Use a non-production database and an approved test 1C endpoint. Execution may create real documents in the configured 1C database.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=sync-one-c-orders
```

Exit code `0` and `[SUCCESS] Job finished OK` mean the pass completed without an unhandled exception. They do not mean every order was accepted. Check the structured final counters `Processed`, `Accepted`, `BusinessError`, `TransportError`, `Deferred`, and `DeadLetter`.

### Runtime behavior

- One run processes at most `BatchSize` due records and exits.
- `InterCallDelay` is observed between SOAP calls.
- Sending is allowed within the configured `Europe/Kyiv` window; the end of the window is exclusive. Outside the window, records are deferred without a SOAP call.
- Transport failures schedule retries after 5, 15, and 30 minutes, then 1 hour, then 3 hours. After `MaximumAttempts`, delivery becomes `DeadLetter`.
- A business rejection becomes `BusinessError` and is not retried automatically.
- An accepted response becomes `Accepted`; the returned 1C document number is stored.
- The stable Sales `OrderNumber` is reused as the 1C request key on every transport retry.
- Multiple workers use optimistic concurrency when claiming a record. The Cloud Run Job is additionally configured with one task and parallelism `1`.
- Cloud Run platform retries must remain at `0`; durable retry scheduling belongs to `OneCOrderSync`.

Invalid values stop the host at startup: batch size and attempt limit must be positive, delays nonnegative, and the sending-window end later than its start within one day. After changing an operational parameter, perform one controlled run and check its summary before resuming the scheduler.

### Cloud Run execution

The deployment process creates or updates `onec-orders-sync` with the Host.Jobs image, argument `--job=sync-one-c-orders`, one task, parallelism `1`, zero platform retries, and a 10-minute timeout. The process does not run the delivery job or create a Cloud Scheduler schedule.

Before enabling the schedule, check the job revision, arguments, secret bindings, task count, parallelism, retries, timeout, and service account. Configure Cloud Scheduler and its calling service account separately in the deployment environment. Keep schedule names and frequency in the environment's operational registry; this repository does not define them.

To stop outbound delivery, pause or disable the scheduler that runs `onec-orders-sync`. Let an execution already in progress finish, or cancel it through the Cloud Run operator interface if an immediate stop is needed. Do not delete the Cloud Run Job or synchronization records: incomplete state is needed for recovery.

After deployment or a configuration change, perform one controlled run on approved data and confirm the final counters and persisted synchronization state before enabling recurring execution.

### Status interpretation

| Status | Meaning | Operator action |
| --- | --- | --- |
| `Pending` | The record is saved and due when `NextAttemptAtUtc` is reached | Wait for or run the next scheduled pass |
| `Sent` | An attempt was claimed, but its result has not been saved | A record younger than `StaleSentRecoveryDelay` is protected; an older one is retried automatically with the same duplicate-safe `OrderNumber` |
| `Accepted` | 1C accepted the order and returned a document number | Do not resend; use the local order number and stored 1C document number for tracing |
| `BusinessError` | 1C responded but did not create a document | Check the safe diagnostic code and source data; there is no automatic retry |
| `TransportError` or `RetryScheduled` | Delivery becomes due after `NextAttemptAtUtc` | Fix connectivity/configuration and let the next pass retry |
| `DeadLetter` | The transport-attempt limit was reached | Follow the `DeadLetter` procedure below; there is no automatic resend to 1C |

### Diagnostics

Search logs by `OrderId`, `OrderNumber`, and `CorrelationId`. Record the attempt number, status, safe diagnostic code, next-attempt time, job revision, and migration version. The final summary contains batch counters.

Do not include SOAP request bodies, response comments, authorization headers, credentials, customer data, or Telegram tokens in logs or incident records. Transport diagnostics name the exception type; business diagnostics use a constrained application code. Raw 1C diagnostic data is intentionally excluded.

If a record has remained `Sent` for less than `StaleSentRecoveryDelay`, do not intervene: the original execution may still be running. Older records are recovered automatically. Repeated concurrency warnings require investigation before manually changing state.

### Dead-letter procedure

When delivery enters `DeadLetter`, the job saves that state first and only then attempts to send a Telegram notification. It contains safe order context and an Excel export of the saved lines. Notification delivery has its own five-attempt limit with delays of 5, 15, and 30 minutes, then 1 and 3 hours; its failure does not resend the order to 1C or change `DeadLetter`.

1. Find the record by local ID or order number and confirm the attempt count and final safe diagnostic code.
2. Check whether the Telegram notification was delivered, is awaiting another attempt, or exhausted its attempts.
3. Use the attached line export and saved order data to check the endpoint, credentials, network, counterparty, and products without copying secrets or raw SOAP request data.
4. Before creating a new record, ask the 1C operator whether a document already exists for the stable local `OrderNumber`.
5. Record the incident outcome and the 1C document number if one already exists.

After the cause is resolved, a manager can schedule a retry for `BusinessError` or `DeadLetter` through `POST /api/admin/orders/{orderId}/sync/retry` with a non-empty JSON `reason`. A `202` response means the transition to `RetryScheduled` was saved and audited; it does not mean 1C accepted the order. The next normal `sync-one-c-orders` run sends it with the same `OrderNumber`. `409` means the current state is not eligible for retry; `404` means the order was not found.

Do not retry `Accepted`, `Sent`, or an already active delivery, and do not reset synchronization records through SQL. If the saved order data is wrong and cannot be corrected through a supported business operation, create a new order instead of resending the same invalid snapshot. The retry endpoint temporarily remains anonymous along with the other admin order endpoints; before production access it must be protected by `PolicyNames.CanManageOrders`.

## Related instructions

- [Host.Jobs CLI](host-jobs-cli.md)
- [Job troubleshooting](troubleshooting.md)
- [Configuration and secrets](../configuration/configuration-and-secrets.md)
- [Manual 1C write smoke test](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md)
