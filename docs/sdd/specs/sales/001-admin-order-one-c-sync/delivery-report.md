# Admin order and 1C synchronization — delivery report

**Delivery status:** Ready for controlled deployment

**Prepared:** 2026-09-09

**Completed verification tasks:** T170, T180, T185

## Delivered behavior

- `POST /api/admin/orders` persists a manager order and its durable 1C synchronization state without waiting for SOAP delivery.
- `GET /api/admin/orders/{orderId}/sync-status` exposes the safe delivery status and the accepted 1C document number.
- `POST /api/admin/orders/{orderId}/sync/retry` schedules an audited retry from `BusinessError` or `DeadLetter`; the regular job performs the SOAP call later.
- `sync-one-c-orders` processes due records in bounded batches, respects the Kyiv sending window, paces SOAP calls, retries transport failures, recovers stale `Sent` claims, prevents concurrent double-send, and handles dead-letter notifications.
- The SOAP adapter distinguishes accepted, business-error, and transport-error outcomes and stores only bounded sanitized diagnostics.

## Changed implementation areas

- Domain: `src/Sales/Sales.Domain/Orders/Entities/Order.cs` and `OneCOrderSync.cs`, including delivery transitions, stable identifiers, retry scheduling, and immutable 1C document numbers.
- Application: `src/Sales/Sales.Application/Features/Orders/` and `Contracts/`, including create, status, and retry commands plus persistence and 1C boundaries.
- API: `src/Sales/Sales.Api/Controllers/AdminOrdersController.cs` and `Contracts/Orders/`.
- Infrastructure: Sales EF configurations, `SalesDbContext`, migrations through `20260909154836_AddOrderSyncRetryAudit`, the order stores, SOAP adapter, job services, notification service, and validated `Sales:OneCOrderSync` options.
- Hosts and deployment: `Host.Jobs` registration/configuration and `.github/workflows/deploy-cloudrun.yml`.
- Tests: Sales suites under `tests/UnitTests/Sales/` and `tests/IntegrationTests/Sales/`, live OpenAPI checks, and architecture tests.
- Documentation: this feature specification, the Sales OpenAPI contract and root registry, configuration reference, Host.Jobs reference, and Sales 1C runbook.

## Completed task IDs

- Readiness checklist: T001, T002, T005–T010; domain: T010, T020, T030.
- Persistence and integration: T040, T050, T060, T080.
- Application and API: T090, T100, T110, T120, T130, T135, T136, T140.
- Background delivery: T145, T150, T155, T160.
- Verification and handoff: T170, T180, T185.

The detailed task state remains in the phase files under [`tasks/`](tasks/README.md).

## Verification evidence

| Check | Result |
| --- | --- |
| `dotnet restore KedrStore.sln` | Passed on 2026-09-09. |
| `dotnet build KedrStore.sln --no-restore` | Passed with 0 errors. |
| `dotnet test KedrStore.sln --no-build --no-restore` | UnitTests 50 passed; ArchitectureTests 10 passed; IntegrationTests 49 passed and 2 skipped because Docker was unavailable locally. |
| Sales EF database update | Passed; the configured local PostgreSQL database contains all migrations through `20260909154836_AddOrderSyncRetryAudit`. |
| EF pending-model check | Passed; no model changes remain outside migrations. |
| Controlled `sync-one-c-orders` execution | Passed with accepted, business-error, and transport-error records; details are in [T170](tasks/verification/06.1-build-tests.md). |
| Live test 1C SOAP write | Three accepted test documents are recorded in [T185](tasks/test/06.3-one-c-write-smoke-test.md). |
| Delivery checklist | Complete in [`checklist/delivery-readiness.md`](checklist/delivery-readiness.md). |

The skipped tests use PostgreSQL Testcontainers to prove concurrent worker exclusion and atomic manual-retry audit persistence. They fail instead of skipping when `CI=true`, so CI with Docker must pass before production promotion.

## Runtime and Cloud Run configuration

The deployment workflow maintains Cloud Run Job `onec-orders-sync` with the current `Host.Jobs` image, `--job=sync-one-c-orders`, one task, parallelism `1`, platform retries `0`, and a 10-minute timeout. It binds database, 1C, and Telegram values from Secret Manager. It does not execute the order job during deployment.

| Setting | Delivered default |
| --- | --- |
| `Sales:OneCOrderSync:BatchSize` | `20` |
| `Sales:OneCOrderSync:InterCallDelay` | `00:00:05` |
| `Sales:OneCOrderSync:MaximumAttempts` | `10` |
| `Sales:OneCOrderSync:SendingWindowStart` | `07:00:00` Europe/Kyiv |
| `Sales:OneCOrderSync:SendingWindowEnd` | `20:00:00` Europe/Kyiv, exclusive |
| `Sales:OneCOrderSync:StaleSentRecoveryDelay` | `00:15:00` |

The operator configures Cloud Scheduler cadence and its service account manually. Dead-letter diagnosis, notification handling, stopping delivery, and manager retry are documented in the [Sales 1C runbook](../../../operations/jobs/sales-one-c-runbook.md).

## Deployment order

1. Ensure the database, 1C, and Telegram secrets exist in Secret Manager.
2. Run the migration job and verify Sales migrations through `20260909154836_AddOrderSyncRetryAudit`.
3. Deploy the API and `Host.Jobs` image; let the workflow create or update `onec-orders-sync` without executing it.
4. Verify the runtime configuration and execute one controlled non-production job pass.
5. Configure or resume Cloud Scheduler with the intended cadence and service account.
6. Confirm status, retry, logs, metrics, and Telegram dead-letter notification behavior before production promotion.

Rollback starts by pausing Cloud Scheduler. Roll back the API/job image while retaining the additive Sales tables and delivery records so queued orders and audit history remain recoverable.

## Residual risks and production gates

- Admin order create, status, and retry endpoints remain intentionally `[AllowAnonymous]`. Before public production exposure, apply `PolicyNames.CanManageOrders` and add authenticated, unauthenticated, and forbidden API tests.
- Cloud Scheduler cadence and service account are operator-managed and cannot be verified from this repository.
- The two PostgreSQL Testcontainers tests were skipped in the local run because Docker was unavailable; CI with Docker must pass them.
- NuGet reports known vulnerabilities for `Microsoft.OpenApi` 2.3.0 and `Scriban` 7.2.0. Dependency upgrades remain a separate platform task.
- T185 proves the configured test 1C accepted real writes on 2026-08-18 and 2026-08-21. The 1C team still owns business verification of created document contents.
