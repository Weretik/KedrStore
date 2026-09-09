# Sales OneC runbook

Sales owns two independent OneC workflows:

- customer reference-data synchronization imports counterparties and category price rules;
- manager-order delivery sends already persisted Sales orders to OneC.

Both workflows use the shared low-level SOAP transport. Their orchestration, persistence, retries, and operational state remain inside Sales Infrastructure. `Host.Jobs` runs one requested operation and exits; recurring execution is owned by the deployment scheduler.

## Customer reference-data synchronization

Before running customer synchronization, read [the counterparty and Identity flow](../../architecture/integrations/one-c/sales-counterparty-identity-sync.md). This workflow creates, updates, and can delete local Identity users as part of reconciliation.

| Command | Operation |
| --- | --- |
| `--job=counterparties` | imports, updates, and restores counterparties according to Sales service policy |
| `--job=counterparty-category-price-types` | imports customer/category price-type rules |
| `--job=sales-customers-full` | runs counterparties first, then price rules |

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=sales-customers-full
```

Review imported, updated, restored, deleted, and skipped counts in the final logs before accepting the refresh.

## Manager-order delivery

`sync-one-c-orders` processes manager orders created through the Sales admin API. The HTTP request persists an `Order` and its `OneCOrderSync` record; it never waits for OneC. The job later selects due synchronization records, sends them through the Sales OneC adapter, persists the result, processes every due dead-letter notification, and exits.

The detailed business and state-transition rules are maintained in the [feature specification](../../specs/sales/001-admin-order-one-c-sync/requirements/one-c-delivery.md). This runbook covers operation of the implemented job.

### Required configuration

Supply secrets through Host.Jobs User Secrets for local Development or through the deployment secret store. Operational settings have safe defaults in `Host.Jobs/appsettings.json` and can be overridden by environment variables.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:Default` / `ConnectionStrings__Default` | Sales PostgreSQL persistence and synchronization state |
| `OneCSoap:Endpoint` / `OneCSoap__Endpoint` | environment-specific OneC SOAP endpoint |
| `OneCSoap:Username` / `OneCSoap__Username` | OneC SOAP authentication |
| `OneCSoap:Password` / `OneCSoap__Password` | OneC SOAP authentication |
| `Telegram:BotToken` / `Telegram__BotToken` | dead-letter notification transport |
| `Telegram:ChatId` / `Telegram__ChatId` | dead-letter notification destination |
| `Sales:OneCOrderSync:BatchSize` / `Sales__OneCOrderSync__BatchSize` | maximum due orders per run; default `20` |
| `Sales:OneCOrderSync:InterCallDelay` / `Sales__OneCOrderSync__InterCallDelay` | pause between SOAP calls as `hh:mm:ss`; default `00:00:05` |
| `Sales:OneCOrderSync:MaximumAttempts` / `Sales__OneCOrderSync__MaximumAttempts` | transport attempts before `DeadLetter`; default `10` |
| `Sales:OneCOrderSync:SendingWindowStart` / `Sales__OneCOrderSync__SendingWindowStart` | Kyiv-local window start as `hh:mm:ss`; default `07:00:00` |
| `Sales:OneCOrderSync:SendingWindowEnd` / `Sales__OneCOrderSync__SendingWindowEnd` | exclusive Kyiv-local window end as `hh:mm:ss`; default `20:00:00` |
| `Sales:OneCOrderSync:StaleSentRecoveryDelay` / `Sales__OneCOrderSync__StaleSentRecoveryDelay` | age after which an unfinished `Sent` attempt can be recovered; default `00:15:00` |

Apply Sales migrations before enabling scheduled delivery. The job must resolve all configuration above even when the current batch contains no new dead-letter record.

### Run locally

Use a non-production database and an approved test OneC endpoint. A run can create real documents in the configured OneC database.

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=sync-one-c-orders
```

Exit code `0` and `[SUCCESS] Job finished OK` mean the pass completed without an unhandled exception. They do not mean every order was accepted. Review the structured completion counts for `Processed`, `Accepted`, `BusinessError`, `TransportError`, `Deferred`, and `DeadLetter`.

### Runtime behavior

- One run processes at most the configured `BatchSize` due records and then exits.
- SOAP calls are separated by the configured `InterCallDelay`.
- Sending is allowed during the configured `Europe/Kyiv` window. Its end is exclusive. Outside that window, due records are deferred to the next window without a SOAP call.
- Transport failures are scheduled after 5 minutes, 15 minutes, 30 minutes, 1 hour, and then 3 hours. Delivery becomes `DeadLetter` at the configured `MaximumAttempts`.
- A business rejection becomes `BusinessError` and is not retried automatically.
- An accepted response becomes `Accepted`; the returned OneC document number is persisted.
- The stable Sales `OrderNumber` is reused as the OneC request key on every transport retry.
- Multiple workers use optimistic concurrency when claiming a record. The Cloud Run Job is additionally configured with one task and parallelism `1`.
- Cloud Run platform retries must remain `0`; durable retry timing belongs to `OneCOrderSync`.

Invalid values stop the host during startup: batch size and maximum attempts must be positive, delays cannot be negative, and the sending-window end must be after its start within one day. After changing an operational setting, run one controlled execution and inspect its summary before restoring the scheduler.

### Cloud Run operation

The deployment workflow creates or updates `onec-orders-sync` with the Host.Jobs image, argument `--job=sync-one-c-orders`, one task, parallelism `1`, platform retries `0`, and a 10-minute timeout. The workflow does not execute the order job and does not create its Cloud Scheduler schedule.

Before enabling a schedule, verify the deployed job revision, arguments, secret bindings, task count, parallelism, retry setting, timeout, and service account. Configure the scheduler and its invocation service account manually in the deployment environment. Keep scheduler names and cadence in the environment's operational inventory because they are not defined by this repository.

To stop outbound order delivery, pause or disable the scheduler that invokes `onec-orders-sync`. Let an already running execution finish or cancel that execution through the Cloud Run operator interface if an immediate stop is required. Do not delete the Cloud Run Job or synchronization rows: pending state is required for recovery.

After deployment or configuration changes, execute one controlled run against approved data and confirm the completion counts and persisted synchronization state before enabling recurring execution.

### Status interpretation

| Status | Meaning | Operator action |
| --- | --- | --- |
| `Pending` | persisted and eligible when `NextAttemptAtUtc` is due | wait for or execute the next scheduled pass |
| `Sent` | an attempt was claimed and its outcome has not been persisted yet | a record younger than `StaleSentRecoveryDelay` remains protected; an older record is automatically retried with the same duplicate-safe `OrderNumber` |
| `Accepted` | OneC accepted the order and returned a document number | no replay; use the local order number and stored OneC document number for tracing |
| `BusinessError` | OneC responded but did not create a document | investigate the safe diagnostic code and source data; no automatic retry |
| `TransportError` or `RetryScheduled` | delivery is eligible after `NextAttemptAtUtc` | fix connectivity or configuration and allow a later pass to retry |
| `DeadLetter` | the transport-attempt limit was reached | follow the dead-letter procedure below; no automatic OneC resend |

### Diagnosis

Search logs by `OrderId`, `OrderNumber`, and `CorrelationId`. Retain the attempt number, status, safe diagnostic code, next-attempt time, job revision, and migration version. The final summary provides batch-level counts.

Do not add SOAP request bodies, response comments, authorization headers, credentials, customer data, or Telegram tokens to logs or incident records. A transport diagnostic identifies the exception type; a business diagnostic identifies the bounded application code. Raw OneC diagnostics are intentionally excluded.

If a record remains `Sent` for less than the configured `StaleSentRecoveryDelay`, do not intervene: the original execution may still be active. Older records are recovered automatically. Repeated concurrency warnings still require investigation before changing state.

### Dead-letter procedure

When delivery transitions to `DeadLetter`, the job persists that state before attempting a Telegram notification. The notification contains safe order context and an Excel export of the persisted order lines. Notification delivery has its own maximum of five attempts with delays of 5 minutes, 15 minutes, 30 minutes, 1 hour, and 3 hours; a notification failure does not resend the OneC order or change its `DeadLetter` status.

1. Find the record by local order ID or order number and confirm its attempt count and final safe diagnostic code.
2. Check whether Telegram notification succeeded, is waiting for its next attempt, or exhausted its notification attempts.
3. Use the attached line export and the stored order data to investigate the endpoint, credentials, network, counterparty, and products without copying secrets or raw SOAP payloads.
4. Confirm with the OneC operator whether a document already exists for the stable local `OrderNumber` before considering another write.
5. Record the incident outcome and the OneC document number if a document already exists.

After correcting the cause, a manager can schedule `BusinessError` or `DeadLetter` through `POST /api/admin/orders/{orderId}/sync/retry` with a non-empty JSON `reason`. A `202` response means the audited transition to `RetryScheduled` was persisted; it does not mean that 1C accepted the order. The next regular `sync-one-c-orders` execution sends it with the same `OrderNumber`. A `409` means the current state is not eligible, and `404` means the order was not found.

Do not retry `Accepted`, `Sent`, or an already active delivery, and do not reset synchronization rows with SQL. If the persisted order data itself is wrong and cannot be corrected through a supported business operation, create a new order instead of repeatedly sending the same invalid snapshot. The retry endpoint remains temporarily anonymous with the other admin-order endpoints and must receive `PolicyNames.CanManageOrders` before production exposure.

## Related procedures

- [Host.Jobs CLI](host-jobs-cli.md)
- [Job troubleshooting](troubleshooting.md)
- [Configuration and secrets](../configuration/configuration-and-secrets.md)
- [Manual OneC write smoke verification](../../specs/sales/001-admin-order-one-c-sync/tasks/test/06.3-one-c-write-smoke-test.md)
