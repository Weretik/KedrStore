# 007 — Reconcile Sales customers with the complete 1C list

- **Module:** Sales, 1C integration; linked local Identity user
- **Status:** Reconciliation and durable Identity deletion queue are implemented; PostgreSQL/FK and concurrency scenarios are not yet confirmed by tests
- **Created:** 2026-10-22
- **Shared product increment:** `order-workspace-2026-10-22`; Admin frontend uses the current customer list through existing `getAdminCustomers`

## Goal and scope

Every **successful full** customer import from 1C must bring local Sales data in line with the source: add new customers once, update existing customers by stable `CounterpartyId`, and physically delete missing customers together with their local orders and drafts. Replaying the same response must not create duplicates. Sync reads 1C locally; it does not modify 1C customer records. No new HTTP endpoint is required; `GET /api/admin/customers` reads the result of the existing job.

## Product rules

- **R-001.** The stable reconciliation key is normalized 1C `CounterpartyId`. In one successful full pass, each key maps to at most one local customer and one linked Identity user. Repeating the same data does not create a customer, user, or duplicate pricing rules.
- **R-002.** Changes to a customer's name, contacts, or price type in 1C update the same local record. An invalid 1C row or failed Identity provisioning is recorded as `Skipped`, but **the presence of its ID in the source is not grounds for deletion** of the local customer.
- **R-003.** After a complete snapshot is established, a customer whose ID is truly absent from 1C is physically deleted from Sales together with local orders, drafts, lines, category price rules, idempotency records, sync/retry state, and any other dependencies found by FK audit. Local deletion happens immediately, even if an order is being sent to 1C or was already sent. The linked Identity user is also scheduled for deletion, without affecting a user linked to another customer.
- **R-004.** Confirmed contract: `GetCounterpartiesAsync()` returns the full list without paging. An empty list, transport failure, or row without a valid ID does not trigger bulk deletion. A non-empty snapshot is reconciled against all trimmed source IDs collected before validating other fields. A shrink threshold and operator gate from EN-001 are no longer prerequisites.
- **R-005.** Sales deletion and creation of a pending Identity deletion record are atomic in one Sales transaction for the selected customers. Identity deletion retry is idempotent; failures remain queued with the last-attempt time and a safe error description. A retry must not delete an Identity user linked to another current Counterparty. PostgreSQL rollback/concurrency behavior still requires separate integration evidence.
- **R-006.** Each pass records imported, updated, unchanged, deleted, skipped, and skip/deletion-stop reasons without personal data or SOAP bodies. The same snapshot run twice yields no new or deleted records on the second pass.

## Acceptance scenarios

| ID | Given / When / Then |
| --- | --- |
| **SC-001** | Given a full 1C list containing a new and an existing customer; when the job runs twice with that list; then the new customer is created once, the existing one is updated by ID, and there are no duplicates. |
| **SC-002** | Given 1C changed a customer's name/price; when the full import completes; then the same ID has current fields and no old duplicate appears. |
| **SC-003** | Given a customer ID is present in 1C but its email is invalid or Identity provisioning fails; when the job processes the snapshot; then the customer is not deleted and the operator can see `Skipped` and its reason. |
| **SC-004** | Given a verified full snapshot no longer contains a customer with local Order/Draft records, including an order being sent to or already sent to 1C; when the safe reconciliation phase deletes it; then the customer and local dependencies are physically absent immediately, other customers/records are unchanged, and the worker does not restore the deleted order. |
| **SC-005** | Given 1C returns an empty snapshot, a row without ID, or a transport failure; when the job runs; then local customers, orders, and drafts are not deleted, and missing IDs are logged. No shrink threshold applies because the operation contract returns a complete list. |
| **SC-006** | Given Sales deletion succeeded but Identity deletion failed; when the job runs again; then the pending record retains user/counterparty IDs, retries deletion, and is removed only after success or confirmation the user is linked to a current customer. |

## Design and data

`OneCCounterpartiesSyncService` owns reconciliation; Identity remains the account owner. Under the confirmed 1C source contract, the SOAP operation returns all counterparties in one array. The service builds `sourceCounterpartyIds` from non-empty trimmed IDs before normalization, provisioning, and Identity checks. If any ID is empty, the deletion set is empty; an empty response also returns before any Sales changes. Source ID presence, rather than successful row import, determines whether a local customer may be considered absent.

`StaleCounterpartyRemoval.RemoveAsync` opens a Sales transaction. It first deletes `OrderSyncRetryAudits` and `OrderIdempotencyRecords` for the customer's orders; then deletes `Orders`, `OrderDrafts`, category rules, and `Counterparties`, and adds `PendingCounterpartyIdentityDeletions`. Database cascades delete Order/Draft lines and order sync/draft idempotency records. After saving and committing the transaction, `DrainIdentityDeletionsAsync` calls the Identity service. The outbox uses a new Guid `Id` primary key and a unique index on `CounterpartyId` + `IdentityUserId`. EF migration: `20261022104036_AddPendingCounterpartyIdentityDeletions`.

`sync-one-c-orders` preloads due sync rows and Orders, then optimistically claims a row through PostgreSQL `xmin` before the external send. Deletion does not recreate Orders: after commit, the worker only updates the sync row, and a missing row is not restored. A worker that already claimed an order may finish its external SOAP call after local deletion; this concurrency and rollback behavior is not yet confirmed by a dedicated PostgreSQL test. Do not replace `Restrict` with uncontrolled cascade. Deleting an order already accepted by 1C affects **only** the local record; this job does not cancel the external document.

## Implementation paths

| Path | State | Responsibility |
| --- | --- | --- |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/OneCCounterpartiesSyncService.cs` | Implemented | Source ID set, upsert/restore, fail-closed for empty/incomplete IDs, and deletion call |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/SalesOneCReadClient.cs` | Existing | Maps the complete SOAP array to Sales DTOs; does not return a snapshot marker |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/StaleCounterpartyRemoval.cs` | Implemented | Transactional Sales dependency deletion, outbox record, and Identity deletion retry |
| `src/Sales/Sales.Infrastructure/DataBase/Entities/PendingCounterpartyIdentityDeletion.cs` | Implemented | Persisted IDs and Identity deletion retry state |
| `src/Sales/Sales.Infrastructure/Configurations/PendingCounterpartyIdentityDeletionConfiguration.cs` | Implemented | Outbox table mapping and index by Identity user ID |
| `src/Sales/Sales.Infrastructure/Migrations/20261022104036_AddPendingCounterpartyIdentityDeletions.cs` | Implemented | Creates durable outbox table with Guid PK and unique CounterpartyId/IdentityUserId pair |
| `src/Sales/Sales.Infrastructure/Configurations/OrderConfiguration.cs`, `OrderDraftConfiguration.cs`, `OrderSyncRetryAuditConfiguration.cs`, `OrderIdempotencyRecordConfiguration.cs` | Checked | Restrict/cascade FKs that determine deletion order |
| `src/Sales/Sales.Infrastructure/Integrations/OneC/Services/SyncOneCOrdersService.cs` | Existing | Loads, optimistically claims through `xmin`, and sends; no dedicated customer-deletion concurrency test |
| `src/Identity/Identity.Infrastructure/Services/ImportedCounterpartyIdentityProvisioningService.cs` | Existing | Idempotent deletion of a user who is already absent |
| `tests/IntegrationTests/Sales/Persistence/CounterpartyReconciliationPostgresTests.cs` | Added; skipped without Docker | Order/Draft deletion and Identity retry scenario; PostgreSQL result not yet obtained |
| `docs/sdd/features/sales/customers.md` | Updated | Current sync and deletion feature map |
## Delivery status and unverified scenarios

The source snapshot contract is confirmed; source-ID reconciliation, transactional Sales deletion, and the pending Identity deletion queue are implemented. Migration `20261022104036_AddPendingCounterpartyIdentityDeletions` is in code but was not run here.

Specialized PostgreSQL integration tests do not yet confirm rollback after FK/SaveChanges failure, concurrent deletion of related Order/Draft/idempotency/retry records, two concurrent customer sync jobs, or interaction with a worker that already passed `TryStartAttemptAsync`. The current worker may complete a SOAP send using a preloaded Order after the local deletion commits; it does not recreate that Order, but this case has not been verified in PostgreSQL. Treat these scenarios as unverified until checked.

For implementation paths, task dependencies, and evidence history, see the source records in this specification and the [customer feature map](../../../features/sales/customers.md).
