# Manager order read API — design: Infrastructure

## Persistence and integrations

Infrastructure will provide no-tracking read projections from the Sales database. The collection joins only the counterparty name and sync summary needed for compact rows and calculates `lineCount` and `totalAmount` in the query. Detail projects lines, current counterparty name/phone, and the safe sync summary. The optional counterparty scope filters exactly by `Orders.CounterpartyId`; neither operation queries 1C. Collection ordering is `CreatedAtUtc` descending, then `OrderId` descending, with page `1`, page size `20`, and maximum `100` defaults/bounds applied by the Application contract.

## Migration and rollout

No migration is planned because the feature reads existing tables and the existing `Orders.CounterpartyId` index. Before implementation, validate the selected list query against representative order-line volumes and add an index only if evidence shows the existing model cannot meet an agreed performance target. The API is additive and must be deployed with its OpenAPI aggregate reference and focused contract tests.
