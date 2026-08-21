# Admin order and 1C synchronization — design: Infrastructure

## Persistence and integrations

`Sales.Infrastructure` maps `Order`, `OrderLine`, and `OneCOrderSync` through EF Core in `SalesDbContext`. It supplies repositories/query abstractions required by Application and a transaction boundary that persists the order with the initial `Pending` delivery record atomically.

Due-record claiming uses PostgreSQL's `xmin` optimistic concurrency token on `OneCOrderSync`. A worker changes a due record to `Sent` and saves before issuing SOAP; a `DbUpdateConcurrencyException` means another worker won the claim and the worker skips that record. This keeps the claim provider-native and prevents concurrent SOAP sends for the same delivery record.

A new Sales write adapter wraps `OneCSoapClientFactory` and the generated `CreateSiteRequestAsync(RequestData)` operation. The adapter maps from an Application-owned delivery DTO to generated WCF types at the Infrastructure boundary; `Generated/Reference.cs` remains unedited. It interprets the agreed accepted and business-error response forms, translates transport exceptions, and never logs credentials or raw sensitive payloads.

`SyncOneCOrdersJob` is registered in Sales Infrastructure and invoked by the existing one-run `Host.Jobs` host. It claims due records safely, obeys the Kyiv delivery window, sends the configured batch with pacing, records outcomes, and applies the configured retry schedule. Its structured logs include `OrderId`, `AttemptCount`, `Status`, and correlation context; metrics cover accepted, transport/business errors, retry queue, and dead-letter queue.

When a delivery enters `DeadLetter`, a Sales Infrastructure Telegram adapter sends exactly one failure notification. Its message identifies the customer, order, and final sanitized error; it attaches an Excel file with the persisted line snapshots. The notifier and Excel exporter use Sales Application abstractions and must not couple Sales to Catalog's public quick-order request types.

## Migration and rollout

This feature requires a new EF Core migration for the Sales database. It must be generated through the repository's EF CLI path, reviewed for indexes and destructive operations, and applied to an empty PostgreSQL-compatible test database before release.

Rollout order: deploy the schema before enabling the scheduled sender; deploy the API and job code; then enable the scheduler only within the configured Kyiv window. Rollback is application rollback plus disabling the job; the new order and delivery tables are retained to avoid losing pending business records. Schema rollback is not automatic after orders may have been created.

The existing `SALES_1C_SYNC_INSTRUCTION.txt` is the feature's operational source for retries, rate limiting, scheduling, idempotency, and diagnostics. Deployment-level Cloud Scheduler configuration remains outside this repository scope.
